using System;
using Java.Util;
using Java.IO;
using Android.Bluetooth;
using Android.Content;
using Java.Util.Concurrent;
using SpO2App.Interface;
using SpO2App.Exceptions;
using SpO2App.Datamodel;

namespace SpO2App.Droid
{
	public class CMS50IWBluetoothConnectionManager:ICMS50IWBluetoothConnectionManager
	{
//		private static readonly string TAG = "CMS50FWBluetoothConnectionManager";
		private static readonly int STAY_CONNECTED_PERIOD_SEC = 5;

		private AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents = null;
		private ICMS50IWConnectionListener cms50FWConnectionListener = null;
		private bool keepAliveTaskRunning;

		// They don't all have to be scheduled ExecutorServices but I
		// made them all the same for simplicity and consistency
		private IScheduledExecutorService generalPurposeExecutor = null;     // runs ResetTask and StopDataTask
		private IScheduledExecutorService readDataExecutor = null;           // runs and re-runs StartDataTask in an indefinite loop
		private IScheduledExecutorService keepAliveExecutor = null;          // runs KeepAliveTask every 5 minutes


		/**
     	 * Main constructor. You need an instance of this object in order to use
     	 * this library.
     	 *
     	 * @param bluetoothName try using: SpO202
     	 */
		public CMS50IWBluetoothConnectionManager(String bluetoothName) {
			this.cms50FWConnectionListener = new CMS50IWConnectionLogger();
			this.androidBluetoothConnectionComponents = new AndroidBluetoothConnectionComponents(this,
				this.cms50FWConnectionListener, bluetoothName);
		}

		public void connect ()
		{
			this._connect (Android.App.Application.Context);
		}

		public void close ()
		{
			this.dispose (Android.App.Application.Context);
		}

		public void setCMS50IWConnectionListener (ICMS50IWConnectionListener cMS50IWCallbacks)
		{
			this.cms50FWConnectionListener = cMS50IWCallbacks;//new ConnectionListenerForwarder(cMS50IWCallbacks);
			this.androidBluetoothConnectionComponents.setCms50FWConnectionListener(this.cms50FWConnectionListener);
		}
			
		private void _setCMS50FWConnectionListener(ICMS50IWConnectionListener cms50FWConnectionListener) {
//			this.cms50FWConnectionListener = new ConnectionListenerForwarder(cms50FWConnectionListener);
//			this.androidBluetoothConnectionComponents.setCms50FWConnectionListener(this.cms50FWConnectionListener);
		}
			
		private void _connect(Context context) {
			try {
				androidBluetoothConnectionComponents.findAndConnect(context);
			} catch (BluetoothNotAvailableException btNotAvailex) {
				throw btNotAvailex;
			}
			catch (BluetoothNotEnabledException btNotEnabledex) {
				throw btNotEnabledex;
			}
		}

		/**
     * Request data from the CMS50FW by issuing a start command on the
     * input stream. Also start the keep-alive service which pings the
     * CMS50FW every 5 seconds to ensure that its Bluetooth connection
     * remains alive.
     */
		public void startData() {
			androidBluetoothConnectionComponents.okToReadData = true;
			if (keepAliveExecutor == null ||
				keepAliveExecutor.IsShutdown ||
				keepAliveExecutor.IsTerminated) {
				keepAliveExecutor = Executors.NewSingleThreadScheduledExecutor ();
			}
			if (!keepAliveTaskRunning) {
				keepAliveExecutor.ScheduleAtFixedRate(new KeepAliveTask(androidBluetoothConnectionComponents),
					0, STAY_CONNECTED_PERIOD_SEC, TimeUnit.Seconds);
				keepAliveTaskRunning = true;
			}
			if (readDataExecutor == null) {
				readDataExecutor = Executors.NewSingleThreadScheduledExecutor();
			}
			readDataExecutor.Submit(new StartDataTask(androidBluetoothConnectionComponents));
		}

		/**
     * Ask the CMS50FW to stop sending data by issuing a stop command on the
     * input stream. Also shutdown the keep-alive service.
     */
		public void stopData() {
			Util.safeShutdown(keepAliveExecutor);
			Util.safeShutdown(readDataExecutor);
			keepAliveTaskRunning = false;
//			submitToGeneralExecutor(new StopDataTask(androidBluetoothConnectionComponents));
			androidBluetoothConnectionComponents.okToReadData = false;
			if (androidBluetoothConnectionComponents.connectionAlive()) {
				try {
					androidBluetoothConnectionComponents.writeCommand(CMS50IWCommand.STOP_DATA);
					Util.log(androidBluetoothConnectionComponents.getCMS50FWConnectionListener(), "Wrote stop command");
				} catch (Java.IO.IOException e) {
					Android.Util.Log.Error("BluetoothConnectionManager", "Could not write stop", e);
				}
			} else {
				Util.log(androidBluetoothConnectionComponents.getCMS50FWConnectionListener(), "Best to reset");
			}
		}

		/**
     * Stop the data. Cancel discovery if ongoing. Shutdown the keep-alive service. Close IO
     * streams, etc. Resets and/or nullifies the Bluetooth connection and other components related
     * to it.
     * <p/>
     * In order to read data again after this method has been called, {@link #connect(android.content.Context)}
     * must be called again.
     */
		public void reset() {
			stopData();
			androidBluetoothConnectionComponents.reset ();
//			submitToGeneralExecutor(new ResetTask(androidBluetoothConnectionComponents));
		}

		/**
     * Shutdown and dispose of the executors and the
     * bluetooth connection manager object.
     */
		private void dispose(Context context) {
			Util.safeShutdown(keepAliveExecutor);
			Util.safeShutdown(readDataExecutor);
			Util.safeShutdown(generalPurposeExecutor);

			// since all executors have been shut down, call dispose on UI thread
			androidBluetoothConnectionComponents.dispose(context);
		}

		private void submitToGeneralExecutor(Java.Lang.IRunnable task) {
			if (generalPurposeExecutor == null || generalPurposeExecutor.IsShutdown || generalPurposeExecutor.IsTerminated) {
				generalPurposeExecutor = Executors.NewSingleThreadScheduledExecutor();
			}
			generalPurposeExecutor.Submit(task);
		}
	}
}

