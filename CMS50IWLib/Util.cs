using System;
//using java.text.SimpleDateFormat;
//import java.util.Locale;
//import java.util.concurrent.ExecutorService;
using Java.Util.Concurrent;

namespace SpO2App.Droid
{
	public static class Util
	{
		public static readonly Java.Text.SimpleDateFormat DATE_FORMAT = new Java.Text.SimpleDateFormat("HH:mm:ss");

		/**
     * Verifies that the ExecutorService in question is still active, and if so,
     * requests an immediate shutdown.
     *
     * @param executorService a service which should be shut down immediately.
     */
		public static void safeShutdown(IExecutorService executorService) {
			if (executorService != null && !executorService.IsTerminated && !executorService.IsShutdown) {
				executorService.ShutdownNow ();
			}
		}

		/**
     * Convenience method for logging via {@link com.albertcbraun.cms50fwlib.CMS50FWConnectionListener#onLogEvent(long, String)}
     * with a timestamp.
     * @param listener the callback implemented by the client of this library
     * @param message any message which the client may wish to see logged
     */
		public static void log(ICMS50IWConnectionListener listener, String message) {
			listener.onLogEvent(DateTime.Now.ToLongTimeString().ToString(), message);
		}

		public static String formatString(String format, Object obj1) {
			return String.Format(format,obj1);
		}

		public static String formatString(String format, Object obj1, Object obj2) {
			return String.Format (format, obj1, obj2);
		}

		public static String formatString(String format, Object obj1, Object obj2, Object obj3) {
			return String.Format (format, obj1, obj2, obj3);
		}

		public static String formatString(String format, Object obj1, Object obj2, Object obj3, Object obj4) {
			object[] args = new object[4]{obj1,obj2,obj3,obj4};
			return String.Format(format,args);
		}
	}
}

