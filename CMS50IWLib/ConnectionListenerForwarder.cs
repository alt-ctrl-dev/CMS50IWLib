using System;
using Android.OS;
using Java.Lang;

namespace SpO2App.Droid
{
	public class ConnectionListenerForwarder:ICMS50IWConnectionListener
	{
		private readonly string TAG = "ConnectionListenerForwarder";
		private ICMS50IWConnectionListener cms50IWConnectionListener = null;
		private Handler handler = null;
		private string timeMs;
		private string message;
		private DataFrame dataFrame;

		public ConnectionListenerForwarder (ICMS50IWConnectionListener cms50iWConnectionListener)
		{
			this.cms50IWConnectionListener = cms50iWConnectionListener;
			this.handler = new Handler (Looper.MainLooper);
		}

		private void postToUIThread (IRunnable runnable)
		{
			handler.Post (runnable);
		}

		public void onConnectionAttemptInProgress ()
		{
			postToUIThread (new Runnable(run_onConnectionAttemptInProgress));
//			postToUIThread(() => cms50IWConnectionListener.onConnectionAttemptInProgress ());
		}

		void run_onConnectionAttemptInProgress ()
		{
			cms50IWConnectionListener.onConnectionAttemptInProgress ();
		}

		public void onConnectionEstablished ()
		{
			postToUIThread (new Runnable(run_onConnectionEstablished));
//			postToUIThread (()=>cms50IWConnectionListener.onConnectionEstablished ());
		}

		void run_onConnectionEstablished ()
		{
			cms50IWConnectionListener.onConnectionEstablished ();
		}

		public void onDataReadAttemptInProgress ()
		{
			postToUIThread (new Runnable(run_onDataReadAttemptInProgress));
//			postToUIThread (()=>cms50IWConnectionListener.onDataReadAttemptInProgress ());
		}

		void run_onDataReadAttemptInProgress ()
		{
			cms50IWConnectionListener.onDataReadAttemptInProgress ();
		}

		public void onDataFrameArrived (DataFrame dFrame)
		{
			this.dataFrame = dFrame;
			postToUIThread (new Runnable(run_onDataFrameArrived));
//			postToUIThread (()=>cms50IWConnectionListener.onDataFrameArrived (dataFrame));
		}

		void run_onDataFrameArrived ()
		{
			cms50IWConnectionListener.onDataFrameArrived (dataFrame);
		}

		public void onDataReadStopped ()
		{
			postToUIThread (new Runnable(run_onDataReadStopped));
//			postToUIThread (()=>cms50IWConnectionListener.onDataReadStopped ());
		}
			
		void run_onDataReadStopped ()
		{
			cms50IWConnectionListener.onDataReadStopped ();
		}

		public void onBrokenConnection ()
		{
			postToUIThread (new Runnable(run_onBrokenConnection));
//			postToUIThread (()=>cms50IWConnectionListener.onBrokenConnection ());
		}

		void run_onBrokenConnection ()
		{
			cms50IWConnectionListener.onBrokenConnection ();
		}

		public void onConnectionReset ()
		{
			postToUIThread (new Runnable(run_onConnectionEstablished));
//			postToUIThread (()=>cms50IWConnectionListener.onConnectionReset ());
		}

		void run_onConnectionReset ()
		{
			cms50IWConnectionListener.onConnectionReset ();
		}

		public void onLogEvent (string msTime, string msg)
		{
			this.timeMs = msTime;
			this.message = msg;
			postToUIThread (new Runnable(run_onLogEvent));
//			postToUIThread (()=>cms50IWConnectionListener.onLogEvent (timeMs, message));
		}

		void run_onLogEvent ()
		{
			cms50IWConnectionListener.onLogEvent (timeMs, message);
		}

	}

	class Runnable : Java.Lang.Object, Java.Lang.IRunnable
	{
		Action action;
		public Runnable (Action action)
		{
			this.action = action;
		}
		public void Run ()
		{
			action ();
		}
	}

}

