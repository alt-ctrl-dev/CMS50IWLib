using System;
using SpO2App.Interface;
using SpO2App.Datamodel;

namespace SpO2App.Droid
{
	public class StopDataTask: Java.Lang.Object, Java.Lang.IRunnable
	{
		private static readonly String TAG = "StopDataTask";
		private static readonly String WROTE_STOP_DATA_COMMAND_TO_OUTPUT_STREAM = "wrote stop data command to outputStream";
		private static readonly String COULD_NOT_WRITE_STOP_DATA_COMMAND_TO_OUTPUT_STREAM_MESSAGE = "Could not write stop data command to output stream";
		private static readonly String OUTPUT_STREAM_IS_NULL_PROBABLY_BEST_TO_RESET_AND_REINITIALIZE_MESSAGE = "Output Stream is null. Probably best to reset and reinitialize.";

		private readonly AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents;
		private readonly ICMS50IWConnectionListener cms50IWConnectionListener;

		public StopDataTask(AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents) {
			this.androidBluetoothConnectionComponents = androidBluetoothConnectionComponents;
			this.cms50IWConnectionListener = androidBluetoothConnectionComponents.getCMS50FWConnectionListener();
		}
			
		public void Run() {
			androidBluetoothConnectionComponents.okToReadData = false;
			if (androidBluetoothConnectionComponents.connectionAlive()) {
				try {
					androidBluetoothConnectionComponents.writeCommand(CMS50IWCommand.STOP_DATA);
					Util.log(cms50IWConnectionListener, WROTE_STOP_DATA_COMMAND_TO_OUTPUT_STREAM);
				} catch (Java.IO.IOException e) {
					Android.Util.Log.Error(TAG, COULD_NOT_WRITE_STOP_DATA_COMMAND_TO_OUTPUT_STREAM_MESSAGE, e);
				}
			} else {
				Util.log(cms50IWConnectionListener, OUTPUT_STREAM_IS_NULL_PROBABLY_BEST_TO_RESET_AND_REINITIALIZE_MESSAGE);
			}
		}
	}
}

