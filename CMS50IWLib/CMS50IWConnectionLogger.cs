using System;
using Android.Util;

namespace SpO2App.Droid
{
	public class CMS50IWConnectionLogger:ICMS50IWConnectionListener
	{
		private static readonly String TAG = "CMS50FWConnectionLogger";

		public void onConnectionAttemptInProgress() {
			Log.Verbose(TAG, "ConnectionAttemptInProgress");
		}

		public void onConnectionEstablished() {
			Log.Verbose(TAG, "ConnectionEstablished");
		}

		public void onDataReadAttemptInProgress() {
			Log.Verbose(TAG, "DataReadAttemptInProgress");
		}

		public void onDataFrameArrived(DataFrame dataFrame) {
			Log.Verbose(TAG, "DataFrameArrived:" + dataFrame);
		}

		public void onDataReadStopped() {
			Log.Verbose(TAG, "DataReadStopped");
		}

		public void onBrokenConnection() {
			Log.Verbose(TAG, "BrokenConnection");
		}

		public void onConnectionReset() {
			Log.Verbose(TAG, "ConnectionReset");
		}

		public void onLogEvent(string timeMs, String message) {
			Log.Verbose(TAG, String.Format("{0} {1}", timeMs, message));
		}
	}
}

