using System;
using Java.Util;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using System.IO;
using Java.IO;


namespace SpO2App.Droid
{
	public class AndroidBluetoothConnectionComponents
	{
		private static readonly string TAG = "AndroidBluetoothConnectionComponents";
		private static readonly string BLUETOOTH_IS_NOT_SUPPORTED_ON_THIS_ANDROID_DEVICE_MESSAGE = "Bluetooth is not supported on this android device!!";
		private static readonly string BLUETOOTH_IS_NOT_ENABLED_MESSAGE = "Bluetooth is not enabled. Please go to Settings and enable Bluetooth on this Android device.";
		private static readonly string SETTING_UP_BROADCAST_RECEIVER_MESSAGE = "Setting up BroadcastReceiver";
		private static readonly string DONE_REGISTERING_BROADCAST_RECEIVER_MESSAGE = "Done registering BroadcastReceiver.";
		private static readonly string CANCELING_PREVIOUS_BLUETOOTH_DISCOVERY_MESSAGE = "Canceling previous Bluetooth discovery.";
		private static readonly string INITIATING_BLUETOOTH_DISCOVERY_OF_CMS50_FW_DEVICE_MESSAGE = "Initiating bluetooth discovery of CMS50FW device.";
		private static readonly string BLUETOOTH_IS_NOT_TURNED_ON_MESSAGE = "Could not start bluetooth discovery. Bluetooth is not in STATE_ON on this device";
		private static readonly string JUST_STARTED_BLUETOOTH_DISCOVERY_MESSAGE = "Just started Bluetooth discovery";
		private static readonly string COULD_NOT_WRITE_COMMAND_MESSAGE = "Could not write command %d to output stream. Bluetooth socket is not connected.";
		private static readonly string STARTING_RESET_MESSAGE = "Starting reset";
		private static readonly string CLOSING_BLUETOOTH_SOCKET_AND_IO_STREAMS_MESSAGE = "Closing Bluetooth socket and I/O streams.";
		private static readonly string OUTPUT_STREAM = "OutputStream";
		private static readonly string INPUT_STREAM = "InputStream";
		private static readonly string BLUETOOTH_SOCKET = "Bluetooth Socket";
		private static readonly string RESET_COMPLETE_MESSAGE = "Reset complete";
		private static readonly string CLOSED_FORMAT_STRING = "Closed %s";
		private static readonly string COULD_NOT_CLOSE_FORMAT_STRING = "Could not close %s";
		private static readonly UUID DEFAULT_BLUETOOTH_SERVICE_UUID = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
		private static readonly string COULD_NOT_UNREGISTER_BROADCAST_RECEIVER_PROCEEDING_ANYWAY_MESSAGE = "Could not unregister BroadcastReceiver because it was apparently never registered. Proceeding anyway.";
		private UUID bluetoothServiceUUID = DEFAULT_BLUETOOTH_SERVICE_UUID;
		private static readonly int COMMAND_ONE_TWENTY_NINE = 129;
		private readonly string androidBluetoothDeviceName;
		private bool broadcastReceiverIsRegistered = false;
		private CMS50IWBluetoothConnectionManager cms50IWBluetoothConnectionManager = null;
		private ICMS50IWConnectionListener cms50IWConnectionListener = null;
		private volatile BluetoothDevice cms50IWDevice = null;
		public volatile BluetoothSocket bluetoothSocket = null;
		private BroadcastReceiver broadcastReceiver = null;
		private BluetoothAdapter bluetoothAdapter = null;
		public volatile Stream outputStream  = null;
		public volatile Stream inputStream  = null;
		public volatile bool okToReadData;

		/**
     * Registers a default, logging-only, CMS50FWConnectionListener which you
     * should replace later, using {@link #setCms50FWConnectionListener(CMS50FWConnectionListener)}.
     *
     * @param cms50FWBluetoothConnectionManager the front end of this framework
     * @param cms50FWConnectionListener callbacks for the client app
     * @param androidBluetoothDeviceName the string which represents the name of the bluetooth device we're looking for (e.g. "SpO202")
     */
		public	AndroidBluetoothConnectionComponents (CMS50IWBluetoothConnectionManager cms50iWBluetoothConnectionManager,
		                                             ICMS50IWConnectionListener cms50iWConnectionListener,
		                                             string androidBtDeviceName)
		{
			this.cms50IWBluetoothConnectionManager = cms50iWBluetoothConnectionManager;
			this.androidBluetoothDeviceName = androidBtDeviceName;
			this.cms50IWConnectionListener = cms50iWConnectionListener;

		}

		/*
		 * Cleans up bluetooth connections, sockets, streams, etc.
		 */
		public void dispose (Context context)
		{
			unregisterBroadcastReceiver (context);
			reset ();
		}

		/**
		 * 
     	 * Allows the BroadcastReceiver to unregister itself after it has completed
     	 * its work.
     	 *
     	 * @param context a {@link Context} used temporarily and then discarded
     	 */
		public void unregisterBroadcastReceiver (Context context)
		{
			if (broadcastReceiver != null && broadcastReceiverIsRegistered) {
				try {
					context.UnregisterReceiver (broadcastReceiver);
				} catch (Java.Lang.IllegalArgumentException e) {
					Log.Warn (TAG, COULD_NOT_UNREGISTER_BROADCAST_RECEIVER_PROCEEDING_ANYWAY_MESSAGE+"| "+e.Message);
				}
				broadcastReceiverIsRegistered = false;
			}
		}

		/**
     	 * Bring the Bluetooth plumbing back to the state it was in before the
     	 * CMS50FW device was discovered and a connection to it was opened.
     	 */
		public void reset ()
		{
			logEvent (STARTING_RESET_MESSAGE);
			okToReadData = false;

			cancelDiscovery ();

			logEvent (CLOSING_BLUETOOTH_SOCKET_AND_IO_STREAMS_MESSAGE);
			if (bluetoothAdapter != null && bluetoothAdapter.IsEnabled &&
			    bluetoothSocket != null && bluetoothSocket.IsConnected) {
				close ((ICloseable)outputStream, OUTPUT_STREAM);
				outputStream = null;
				close ((ICloseable)inputStream, INPUT_STREAM);
				inputStream = null;
				close (bluetoothSocket, BLUETOOTH_SOCKET);
				bluetoothSocket = null;
			}

			cms50IWDevice = null;
			cms50IWConnectionListener.onConnectionReset ();

			logEvent (RESET_COMPLETE_MESSAGE);
		}

		/**
     	 * Calls back to the callback listener's onLogEvent method, supplying a timestamp
     	 * in the process.
     	 *
     	 * @param message a message to be sent to the client app or made visible to the user somehow
     	 */
		private void logEvent (string message)
		{
			cms50IWConnectionListener.onLogEvent (DateTime.Now.Millisecond, message);
		}

		/**
     	 * Cancel bluetooth device discovery on this android device if possible.
     	 */
		public void cancelDiscovery ()
		{
			if (bluetoothAdapter != null && bluetoothAdapter.IsDiscovering) {
				bluetoothAdapter.CancelDiscovery ();
			}
		}

		/**
     	 * Close an IO stream or socket.
     	 *
     	 * @param objectRef the stream or socket
     	 * @param objectName the name, for logging message purposes
     	 */
		private void close (Java.IO.ICloseable objectRef, string objectName)
		{ //TODO INTERFACE Comeback here
			if (objectRef != null) {
				try {
					objectRef.Close ();
					logEvent (Util.formatString (CLOSED_FORMAT_STRING, objectName));
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, Util.formatString (COULD_NOT_CLOSE_FORMAT_STRING, objectName), e);
				}
			}
		}

		/**
     	 * Set the custom instance of {@link com.albertcbraun.cms50fwlib.CMS50FWConnectionListener} for your
     	 * app here. This is really useful because it informs your app about the state
     	 * of the bluetooth adapter, connection, progress reading data, etc.
     	 *
     	 * @param cms50FWConnectionListener callbacks for the client app
     	 */
		public void setCms50FWConnectionListener (ICMS50IWConnectionListener cms50iWConnectionListener)
		{
			this.cms50IWConnectionListener = new ConnectionListenerForwarder (cms50iWConnectionListener);
		}

		/**
     	 * A convenient way to provide the callback object to other classes that need it.
    	 *
     	 * @return a listener object for communicating important data back to the client app
     	 */
		public ICMS50IWConnectionListener getCMS50FWConnectionListener ()
		{
			return cms50IWConnectionListener;
		}

		/**
     	 * A convenient way to provide the connection manager to other classes that need it.
     	 *
     	 * @return the frontend object of this library used by the client app
     	 */
		public CMS50IWBluetoothConnectionManager getCMS50FWBluetoothConnectionManager ()
		{
			return cms50IWBluetoothConnectionManager;
		}

		/**
     	 * Verifies that Bluetooth connections are possible from the current Android device.
     	 * Then, registers a temporary, custom BroadcastReceiver and starts bluetooth discovery.
     	 * The rest of the work of obtaining the Bluetooth device, connecting, obtaining a
     	 * Bluetooth socket, and obtaining IO streams will be done after the device
     	 * is actually discovered, in the CMS50FWBroadcastReceiver.
     	 */
		public void findAndConnect (Context context)
		{
			try {
				bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
				if (bluetoothAdapter == null) {
					Log.Warn (TAG, BLUETOOTH_IS_NOT_SUPPORTED_ON_THIS_ANDROID_DEVICE_MESSAGE);
					logEvent (BLUETOOTH_IS_NOT_SUPPORTED_ON_THIS_ANDROID_DEVICE_MESSAGE);
					throw new BluetoothNotAvailableException ();
				} else {
					if (!bluetoothAdapter.IsEnabled) {
						Log.Warn (TAG, BLUETOOTH_IS_NOT_ENABLED_MESSAGE);
						logEvent (BLUETOOTH_IS_NOT_ENABLED_MESSAGE);
						throw new BluetoothNotEnabledException ();
					}
				}

				// cancel any existing discovery which may be ongoing
				if (bluetoothAdapter.IsDiscovering) {
					logEvent (CANCELING_PREVIOUS_BLUETOOTH_DISCOVERY_MESSAGE);
					bluetoothAdapter.CancelDiscovery ();
				}

				// remove previous broadcast receiver if it is still registered
				unregisterBroadcastReceiver (context);

				// set up broadcast receiver - will be torn down after making connection
				this.logEvent (SETTING_UP_BROADCAST_RECEIVER_MESSAGE);
				broadcastReceiver = new CMS50IWBroadcastReceiver (this);
				IntentFilter filter = new IntentFilter (BluetoothDevice.ActionFound);
				filter.AddAction (BluetoothDevice.ActionUuid);
				context.RegisterReceiver (broadcastReceiver, filter);
				broadcastReceiverIsRegistered = true;
				logEvent (DONE_REGISTERING_BROADCAST_RECEIVER_MESSAGE);

				// initiate Bluetooth discovery, which will invoke the BroadcastReceiver later
				logEvent (INITIATING_BLUETOOTH_DISCOVERY_OF_CMS50_FW_DEVICE_MESSAGE);
				if (!bluetoothAdapter.StartDiscovery ()) {
					logEvent (BLUETOOTH_IS_NOT_TURNED_ON_MESSAGE);
				}
				Log.Verbose (TAG, JUST_STARTED_BLUETOOTH_DISCOVERY_MESSAGE);
			} catch (BluetoothNotAvailableException btNotAvailableEx) {
				throw btNotAvailableEx;
			} catch (BluetoothNotEnabledException btNotEnabledEx) {
				throw btNotEnabledEx;
			}
		}


		/**
     	 * Verifies that the various components (socket, streams, etc) needed
     	 * for a useful connection to the Bluetooth device are still viable.
     	 *
     	 * @return true if the plumbing to the Bluetooth device appears to still be working.
     	 */
		public bool connectionAlive ()
		{
			return bluetoothAdapter.IsEnabled && cms50IWDevice != null &&
			inputStream != null && outputStream != null &&
			bluetoothSocket != null && bluetoothSocket.IsConnected;
		}

		/**
     	 *
     	 * @param command a single command from the custom enum {@link com.albertcbraun.cms50fwlib.CMS50FWCommand}
      	 * @param dataByte an additional byte. if the command does not require any additional data,
     	 *                 this should be {@link com.albertcbraun.cms50fwlib.CMS50FWCommand#PADDING}.
     	 * @throws IOException if the write attempt fails and the command is not written back to the device
     	 *          (because, for example, of a broken Bluetooth connection caused by a Bluetooth device
     	 *          which has been shut down or moved out of Bluetooth range).
     	 *
     	 */
		//		@SuppressWarnings("SameParameterValue")
		public void writeCommand (CMS50IWCommand command, CMS50IWCommand dataByte)
		{
			try {
				if (connectionAlive ()) {
					//, 0, buffer.length
					byte[] buffer = new byte[9] { 
						CMS50IWCommand.COMMAND_FOLLOWS.asByte(), 
						(byte)COMMAND_ONE_TWENTY_NINE, 
						command.asByte(), 
						dataByte.asByte(), 
						CMS50IWCommand.PADDING.asByte(), 
						CMS50IWCommand.PADDING.asByte(), 
						CMS50IWCommand.PADDING.asByte(), 
						CMS50IWCommand.PADDING.asByte(), 
						CMS50IWCommand.PADDING.asByte()
					};
//					outputStream.Write (CMS50IWCommand.COMMAND_FOLLOWS.asInt ()); // mark the beginning of command bytes
//					outputStream.Write (COMMAND_ONE_TWENTY_NINE);                // 0x81 - not sure what this is
//					outputStream.Write (command.asInt ());                        // the actual command
//					outputStream.Write (dataByte.asInt ());                       // sometimes a particular byte must follow the command, but not always
//					outputStream.Write (CMS50IWCommand.PADDING.asInt ());
//					outputStream.Write (CMS50IWCommand.PADDING.asInt ());
//					outputStream.Write (CMS50IWCommand.PADDING.asInt ());
//					outputStream.Write (CMS50IWCommand.PADDING.asInt ());
//					outputStream.Write (CMS50IWCommand.PADDING.asInt ());
					outputStream.Write(buffer, 0, buffer.Length);
					outputStream.Flush ();
				} else {
					Log.Warn (TAG, COULD_NOT_WRITE_COMMAND_MESSAGE);
				}

			} catch (Java.IO.IOException ex) {
				throw ex; 
			}
		}

		/**
     	 * Use this instead of {@link #writeCommand(CMS50FWCommand, CMS50FWCommand)} if there
     	 * is no additional data following the command.
     	 *
     	 * @param command a single command from the custom enum {@link com.albertcbraun.cms50fwlib.CMS50FWCommand}
     	 * @throws IOException if the write attempt fails and the command is not written back to the device
     	 *          (because, for example, of a broken Bluetooth connection caused by a Bluetooth device
     	 *          which has been shut down or moved out of Bluetooth range).
     	 */
		public void writeCommand(CMS50IWCommand command) {
			try {
				writeCommand(command, CMS50IWCommand.PADDING);
			} catch (Java.IO.IOException ex) {
				throw ex;
			}

		}

		/**
     * This does the real work of finding, connecting to the CMS50FW Bluetooth device, and
     * opening sockets and streams. This is invoked when the system creates a broadcast
     * in response to a successful Bluetooth discovery command.
     */
		private class CMS50IWBroadcastReceiver: BroadcastReceiver {

			private static readonly string ATTEMPTING_TO_CONNECT_TO_CMS50FW_MESSAGE = "Attempting to connect to CMS50FW.";
			private static readonly string RETRIEVING_UUIDS_FROM_BLUETOOTH_DEVICE_FORMAT = "Retrieving UUIDs from BluetoothDevice: Name:%s, Address:%s, BluetoothClass:%s";
			private static readonly string RETRIEVED_UUID_FROM_CMS50_FW_WILL_USE_MESSAGE = "Retrieved UUID from CMS50FW. Will use this UUID instead of default:";
			private static readonly string ATTEMPTING_TO_GET_NEW_BLUETOOTH_SOCKET_TO_CMS50_FW_DEVICE_MESSAGE = "Attempting to get new bluetoothSocket to CMS50FW device";
			private static readonly string ATTEMPTING_TO_CONNECT_ON_BLUETOOTH_SOCKET_MESSAGE = "Attempting to connect on bluetoothSocket";
			private static readonly string BLUETOOTH_SOCKET_CONNECTED_SUCCESSFULLY_MESSAGE = "BluetoothSocket connected successfully.";
			private static readonly string SET_REFERENCES_TO_INPUT_AND_OUTPUT_STREAMS_MESSAGE = "Set references to input and output streams.";
			private static readonly string DISCOVERY_AND_CONNECTION_COMPLETE_MESSAGE = "Discovery and connection complete.";
			private static readonly string IO_EXCEPTION_TRYING_TO_GET_AND_CONNECT_BLUETOOTH_SOCKET_MESSAGE = "IOException trying to get and connect BluetoothSocket";
			private static readonly string ERROR_CONNECT_ATTEMPT_FAILED_PLEASE_TRY_AGAIN_MESSAGE = "Error: connect attempt failed. Please try again.";
			private static readonly string A_BLUETOOTH_DEVICE_HAS_BEEN_FOUND_MESSAGE = "A Bluetooth device has been found.";
			private static readonly string BLUETOOTH_DEVICE_FOUND_FORMAT = "BluetoothDevice found: Name:%s, Address:%s, BluetoothClass:%s";
			private static readonly string CMS50FW_BLUETOOTH_DEVICE_FOUND_MESSAGE = "The Bluetooth device found is the CMS50FW";
			AndroidBluetoothConnectionComponents outerComponentRef;

			public CMS50IWBroadcastReceiver(AndroidBluetoothConnectionComponents andBtComp){
				this.outerComponentRef=andBtComp;
			}

			public override void OnReceive(Context context, Intent intent) {
				string action = intent.Action;
				// When discovery finds a device
				if (BluetoothDevice.ActionFound.Equals(action)) {
					outerComponentRef.logEvent(A_BLUETOOTH_DEVICE_HAS_BEEN_FOUND_MESSAGE);
					// Get the BluetoothDevice object from the Intent
					BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra (BluetoothDevice.ExtraDevice);
					// BluetoothDevice found: Name:SpO202, Address:00:0E:..., BluetoothClass:1f00
					Log.Verbose(TAG, Util.formatString(BLUETOOTH_DEVICE_FOUND_FORMAT,
						device.Name, device.Address, device.BluetoothClass));
					if (device.Name != null && device.Name.Equals(outerComponentRef.androidBluetoothDeviceName)) {
						outerComponentRef.logEvent(CMS50FW_BLUETOOTH_DEVICE_FOUND_MESSAGE);
						outerComponentRef.cms50IWDevice = device;

						// we've found our device. so no  more need for discovery. save radio resources!
						outerComponentRef.bluetoothAdapter.CancelDiscovery();

						// TODO: reinstate and test on appropriate devices, both above and below API 19
						// Problem: I'm not certain that all CMS50FWs in the field share this Bluetooth PIN.
						// Requires API 19, so verify the API level here.
						// byte[] pinCode = new byte[]{7,7,6,2};
						// cms50FWDevice.setPin(pinCode);

						if (!outerComponentRef.connectionAlive()) {
							outerComponentRef.logEvent(ATTEMPTING_TO_CONNECT_TO_CMS50FW_MESSAGE);
							outerComponentRef.cms50IWConnectionListener.onConnectionAttemptInProgress();

							Log.Verbose(TAG, Util.formatString(RETRIEVING_UUIDS_FROM_BLUETOOTH_DEVICE_FORMAT,
								outerComponentRef.cms50IWDevice.Name, outerComponentRef.cms50IWDevice.Address, outerComponentRef.cms50IWDevice.BluetoothClass));

							// update the UUID with the one from the actual, physical device, if available
							ParcelUuid[] uuidArray = outerComponentRef.cms50IWDevice.GetUuids();
							if (uuidArray != null) {
								for (int i = 0; i < uuidArray.Length; i++) {
									if (i == 0 && uuidArray.Length > 0) {
										// assume 0th uuid is the uuid for the service we want
										outerComponentRef.bluetoothServiceUUID = uuidArray[i].Uuid;
										Log.Verbose(TAG, RETRIEVED_UUID_FROM_CMS50_FW_WILL_USE_MESSAGE +
											uuidArray[i].Uuid);
									}
								}
							}

							// get socket and connect
							try {
								outerComponentRef.logEvent(ATTEMPTING_TO_GET_NEW_BLUETOOTH_SOCKET_TO_CMS50_FW_DEVICE_MESSAGE);
								outerComponentRef.bluetoothSocket = outerComponentRef.cms50IWDevice.CreateRfcommSocketToServiceRecord(outerComponentRef.bluetoothServiceUUID);
								outerComponentRef.logEvent(ATTEMPTING_TO_CONNECT_ON_BLUETOOTH_SOCKET_MESSAGE);
								outerComponentRef.bluetoothSocket.Connect();
								outerComponentRef.logEvent(BLUETOOTH_SOCKET_CONNECTED_SUCCESSFULLY_MESSAGE);
								outerComponentRef.inputStream = outerComponentRef.bluetoothSocket.InputStream;
								outerComponentRef.outputStream = outerComponentRef.bluetoothSocket.OutputStream;
								outerComponentRef.logEvent(SET_REFERENCES_TO_INPUT_AND_OUTPUT_STREAMS_MESSAGE);
								outerComponentRef.cms50IWConnectionListener.onConnectionEstablished();
								outerComponentRef.logEvent(DISCOVERY_AND_CONNECTION_COMPLETE_MESSAGE);
							} catch (Java.IO.IOException e) {
								Log.Error(TAG, IO_EXCEPTION_TRYING_TO_GET_AND_CONNECT_BLUETOOTH_SOCKET_MESSAGE, e);
								outerComponentRef.logEvent(ERROR_CONNECT_ATTEMPT_FAILED_PLEASE_TRY_AGAIN_MESSAGE);
							}

							// remove this broadcast receiver right away. we don't want it hanging around
							// because we do not have an easy way to unregister it later (unless we hold
							// a reference to a Context, which we do not want to do.)
							outerComponentRef.unregisterBroadcastReceiver(context);
						}
					}
				}
			}
		}
	}
}

