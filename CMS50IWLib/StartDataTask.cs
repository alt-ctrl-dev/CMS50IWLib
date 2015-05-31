using System;
using Android.Util;
using Java.IO;
using System.Threading.Tasks;
using System.IO;

namespace SpO2App.Droid
{
	public class StartDataTask: Java.Lang.Object, Java.Lang.IRunnable
	{
		private static readonly String TAG = "StartDataTask";
		private static readonly int BIT_0 = 1;
		private static readonly int BIT_1 = 2;
		private static readonly int BIT_2 = 4;
		private static readonly int BIT_3 = 8;
		private static readonly int BITS_ZERO_TO_THREE = BIT_0 | BIT_1 | BIT_2 | BIT_3;
		private static readonly int BIT_4 = 16;
		private static readonly int BIT_5 = 32;
		private static readonly int BIT_6 = 64;
		private static readonly int BITS_ZERO_TO_SIX = BIT_0 | BIT_1 | BIT_2 | BIT_3 | BIT_4 | BIT_5 | BIT_6;
		private static readonly int BIT_7 = 128;
		private static readonly int SIXTY_FOUR = 64;
		private static readonly int ONE_TWENTY_SEVEN = 127;
		private static readonly String COULD_NOT_PUT_STREAMING_DATA_INTO_A_NEW_DATA_FRAME = "Could not put streaming data into a new data frame.";
		private static readonly String ERROR_CONNECTION_IS_NOT_ALIVE_MESSAGE = "Error. Connection is not alive. ";
		private static readonly String BEGINNING_DATA_READ_OPERATIONS_MESSAGE = "Beginning data read operations.";
		private static readonly String IO_EXCEPTION_WITH_INPUT_STREAM_OR_OUTPUT_STREAM_OBJECT_MESSAGE = "IOException with InputStream or OutputStream object.";
		private static readonly String CONNECTION_TASK_COMPLETED_MESSAGE = "Connection completed.";

		private AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents = null;
		private ICMS50IWConnectionListener cms50FWConnectionListener = null;

		public StartDataTask (AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents)
		{
			this.cms50FWConnectionListener = androidBluetoothConnectionComponents.getCMS50FWConnectionListener ();
			this.androidBluetoothConnectionComponents = androidBluetoothConnectionComponents;
		}


		/**
     * Extract the frame of data representing one tick of the 60HZ data stream
     * transmitted via Bluetooth from the CMS50FW.
     *
     * @return a new DataFrame object whose values are derived from seven byte
     * sequences found in the data stream. Each seven byte sequence is
     * preceded by a single boundary byte so that each data frame is considered
     * to be eight bytes long.
     */
		private DataFrame getNextDataFrame ()
		{

			// separates each frame from previous frame
			//noinspection UnusedAssignment
			byte frameBoundary; // aka byte1

			// actual data is stored in these seven bytes
			//noinspection UnusedAssignment
			byte byte2; // ignored
			byte byte3;
			byte byte4;
			byte byte5;
			byte byte6;
			//noinspection UnusedAssignment
			byte byte7; // ignored
			//noinspection UnusedAssignment
			byte byte8; // ignored

			if (androidBluetoothConnectionComponents.inputStream != null) {
				try {
					// create a new empty data frame, ready to be filled in
					DataFrame dataFrame = new DataFrame ();
					Log.Debug (TAG, "Inside IF statement");

//					while (true) {
//						byte[] data = await waitForNextByte ();
////						BitConverter.ToString(data)
//						Log.Info ("BEN", " Buffer length is " + data.Length);
//						Log.Info ("BEN", " Buffer output is " + BitConverter.ToString (data));
//						Log.Info ("BEN", "--------------------------------------------------------------------------");
//					}
					// search the stream until the byte which signals the beginning of the next data frame is found
					while (true) {
						byte frameBoundaryCandidate = waitForNextByte ();
						Log.Info(TAG,"Byte received "+frameBoundaryCandidate.ToString("X2"));
						if ((frameBoundaryCandidate & BIT_7) == BIT_7) { // look for next byte with the 7 bit set
							//noinspection UnusedAssignment
							frameBoundary = frameBoundaryCandidate;
							Log.Info(TAG,"Inside IF assign statement | frameBoundary = " + frameBoundary.ToString("X2"));
								
							break;
						}
					}
//
//					// the next 7 bytes are the meaningful ones in the CMS50FW data stream
//					// but, in this code, byte2, byte7 and byte8 will not be used
//
//
////					Log.Debug(TAG,"AFTER While true statement "+counter+", waiting for next byte");
//					//noinspection UnusedAssignment
////					waitForNextByte();
					byte2 = waitForNextByte();

					// bytes we actually use
					byte3 = waitForNextByte();
					byte4 = waitForNextByte();
					byte5 = waitForNextByte();
					byte6 = waitForNextByte();

					//noinspection UnusedAssignment
					byte7 = waitForNextByte();
					//noinspection UnusedAssignment
					byte8 = waitForNextByte();

					Log.Info("BEN", " byte2 = " + byte2.ToString("X2"));
					Log.Info("BEN", " byte3 = " + byte3.ToString("X2"));
					Log.Info("BEN", " byte4 = " + byte4.ToString("X2"));
					Log.Info("BEN", " byte5 = " + byte5.ToString("X2"));
					Log.Info("BEN", " byte6 = " + byte6.ToString("X2"));
					Log.Info("BEN", " byte7 = " + byte7.ToString("X2"));
					Log.Info("BEN", " byte8 = " + byte8.ToString("X2"));
					Log.Info(TAG,"Final data received.");

					dataFrame.PulseWaveForm = (byte3 & BITS_ZERO_TO_SIX);
					dataFrame.PulseIntensity = (byte4 & BITS_ZERO_TO_THREE);
					dataFrame.PulseRate = (byte5 & BITS_ZERO_TO_SIX); // TODO: this does not allow pulseRate to be above 127.  But for lower values, it seems to be correct.
					dataFrame.Spo2Percentage = (byte6 & BITS_ZERO_TO_SIX);
					dataFrame.IsFingerOutOfSleeve = (dataFrame.PulseWaveForm == SIXTY_FOUR) &&
						(dataFrame.PulseRate == ONE_TWENTY_SEVEN) && (dataFrame.Spo2Percentage == ONE_TWENTY_SEVEN);

					return dataFrame;

				} catch (Java.IO.IOException e) {
					Log.Error (TAG, COULD_NOT_PUT_STREAMING_DATA_INTO_A_NEW_DATA_FRAME, e);
				}
			}
			return null;
		}

		/**
     * Spins until a byte becomes available on the input stream. Then it
     * reads and returns that single byte.
     *
     * @return the next byte from the input stream
     * @throws IOException if the Bluetooth connection is unexpectedly closed or
     * the stream can't be read for some reason.
     */
		private byte waitForNextByte ()
		{

			try {
				// It's important to check for a live connection frequently here because another thread
				// might close connection, causing an IOException to be thrown from this code

				//noinspection StatementWithEmptyBody
//				int byteRead;
				System.IO.Stream inStream = androidBluetoothConnectionComponents.inputStream;
//				androidBluetoothConnectionComponents.bluetoothSocket.InputStream.;
//				if (!inStream.CanRead){
//					continue;
//				}
				while (androidBluetoothConnectionComponents.connectionAlive () && !inStream.CanRead && !inStream.IsDataAvailable()) {
					// do nothing until a byte is available from input stream
//					byteRead = androidBluetoothConnectionComponents.inputStream.ReadByte();
					Log.Debug (TAG, "Doing nothing while waiting for next byte | CanRead = " + androidBluetoothConnectionComponents.inputStream.CanRead);
				}//IsDataAvailable
				int result = -1;

				if (androidBluetoothConnectionComponents.connectionAlive ()) {
//					androidBluetoothConnectionComponents.inputStream
					//outerComponentRef.inputStream.ReadAsync();
//					length = (int)inStream.Length;
//					result = new byte[length];
					result = inStream.ReadByte();
//					await inStream.ReadAsync (result, 0, length);
//					return result;
				}
//
				return (byte)result;
			} catch (Java.IO.IOException ex) {
				throw ex;
			}

		}

		public void Run ()
		{
			if (!androidBluetoothConnectionComponents.connectionAlive ()) {
				Util.log (cms50FWConnectionListener, ERROR_CONNECTION_IS_NOT_ALIVE_MESSAGE);
				return;
			}

			// allow client to know that work has begun. useful for disabling buttons, etc.
			cms50FWConnectionListener.onDataReadAttemptInProgress ();

			// tell the manager it's ok to read data
			androidBluetoothConnectionComponents.okToReadData = true;

			try {
				Util.log (cms50FWConnectionListener, BEGINNING_DATA_READ_OPERATIONS_MESSAGE);

				// writing to input stream in order to issue a command
				androidBluetoothConnectionComponents.writeCommand (CMS50IWCommand.START_DATA);
				Util.log (cms50FWConnectionListener, "BEN  | writeCommand done | androidBluetoothConnectionComponents.okToReadData = " + androidBluetoothConnectionComponents.okToReadData);
				while (androidBluetoothConnectionComponents.okToReadData) {
					Util.log (cms50FWConnectionListener, "BEN  | waiting for data frame...");
//					var df = getNextDataFrame ();
					cms50FWConnectionListener.onDataFrameArrived (getNextDataFrame ());
					Util.log (cms50FWConnectionListener, "BEN  | onDataFrameArrived done");
				}
			} catch (Java.IO.IOException ioe) {
				Util.log (cms50FWConnectionListener, IO_EXCEPTION_WITH_INPUT_STREAM_OR_OUTPUT_STREAM_OBJECT_MESSAGE);
				Log.Error (TAG, IO_EXCEPTION_WITH_INPUT_STREAM_OR_OUTPUT_STREAM_OBJECT_MESSAGE, ioe);
			} finally {
				Util.log (cms50FWConnectionListener, CONNECTION_TASK_COMPLETED_MESSAGE);
			}

			cms50FWConnectionListener.onDataReadStopped ();
		}
	}
}

