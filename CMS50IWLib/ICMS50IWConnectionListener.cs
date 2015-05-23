using System;

namespace SpO2App.Droid
{
	public interface ICMS50IWConnectionListener
	{
		/*
     * Attempting to connect over bluetooth socket.
     */
		void onConnectionAttemptInProgress();
		/**
     * Bluetooth connection on bluetooth socket succeeded.
     */
		void onConnectionEstablished();

		/**
     * System has started to try reading data from CMS50FW.
     * (The success of reading will be confirmed when the first {@link DataFrame}
     * object is handed to {@link #onDataFrameArrived(DataFrame)}).
     */
		void onDataReadAttemptInProgress();

		/**
     * A set of data representing one 60Hz data collection
     * cycle has arrived.
     *
     * @param dataFrame the set of data measurements output by one cycle of the CMS50FW
     */
		void onDataFrameArrived(DataFrame dataFrame);

		/**
     * System has stopped reading data from CMS50FW as the
     * result of a stop data command being sent to the CMS50FW.
     */
		void onDataReadStopped();

		/**
     * Bluetooth connection to CMS50FW has failed. CMS50FW may,
     * for example, have been turned off or moved out of
     * Bluetooth range.
     */
		void onBrokenConnection();

		/**
     * Successful cancellation of bluetooth discovery, executor service shutdown, and
     * IO stream closures have occurred.
     */
		void onConnectionReset();

		/**
     * Log a timestamped message from within the library.
     *
     * @param timeMs time stamp
     * @param message a message which the client or end user may wish to see logged
     */
		void onLogEvent(long timeMs, String message);
	}
}

