using System;
using Android.Util;
using SpO2App.Interface;
using SpO2App.Datamodel;

namespace SpO2App.Droid
{
	public class KeepAliveTask : Java.Lang.Object, Java.Lang.IRunnable
	{

		private static readonly String TAG = "KeepAliveTask";
		private static readonly String BROKEN_PIPE = "Broken pipe";
		private static readonly String COULD_NOT_WRITE_STAY_CONNECTED_COMMAND_MESSAGE = "Could not " +
			"write stay connected command because socket and/or output stream were not ready.";
		private static readonly String BROKEN_PIPE_LOG_MESSAGE = "Broken Connection to CMS50FW!";
		private static readonly String BROKEN_PIPE_COULD_NOT_WRITE_STAY_CONNECTED_COMMAND_MESSAGE = "Could not write stay connected command.";
		private static readonly String KEEP_ALIVE_TASK_EXITING_WITHOUT_WRITING_CMS50FW_COMMAND_MESSAGE =
			"Keep alive task exiting without writing command to CMS50FW because reading data is not currently enabled.";

		private AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents = null;
		private ICMS50IWConnectionListener cms50IWConnectionListener = null;

		public KeepAliveTask(AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents) {
			this.androidBluetoothConnectionComponents = androidBluetoothConnectionComponents;
			this.cms50IWConnectionListener = this.androidBluetoothConnectionComponents.getCMS50FWConnectionListener();
		}
			
		public void Run() {
			if (!androidBluetoothConnectionComponents.okToReadData ) {
				Util.log(cms50IWConnectionListener, KEEP_ALIVE_TASK_EXITING_WITHOUT_WRITING_CMS50FW_COMMAND_MESSAGE);
				return;
			}
			if (androidBluetoothConnectionComponents.connectionAlive()) {
				try {
					androidBluetoothConnectionComponents.writeCommand(CMS50IWCommand.STAY_CONNECTED);
				} catch (Java.IO.IOException e) {
					Log.Error(TAG, BROKEN_PIPE_COULD_NOT_WRITE_STAY_CONNECTED_COMMAND_MESSAGE, e);
					if (e.Message.Contains(BROKEN_PIPE)) {
						Util.log(cms50IWConnectionListener, BROKEN_PIPE_LOG_MESSAGE);
						cms50IWConnectionListener.onBrokenConnection();
					}
				}
			} else {
				Util.log(cms50IWConnectionListener, COULD_NOT_WRITE_STAY_CONNECTED_COMMAND_MESSAGE);
			}
		}
	}
}

