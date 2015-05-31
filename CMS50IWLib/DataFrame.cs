using System;

namespace SpO2App.Droid
{
	public class DataFrame
	{
		public readonly string time;

		public int Spo2Percentage { get; set; }

		public bool IsFingerOutOfSleeve { get; set; }

		public int PulseRate { get; set; }

		public int PulseIntensity { get; set; }

		public int PulseWaveForm { get; set;}

		public override string ToString ()
		{
			return string.Format ("time:{0}, spo2Percentage:{1}, pulseRate:{2}, pulseWaveForm:{3}, pulseIntensity:{4}," +
				" isFingerOutOfSleeve:{5}",time, Spo2Percentage, PulseRate, PulseWaveForm, PulseIntensity, IsFingerOutOfSleeve);
		}

		public DataFrame ()
		{
			time = DateTime.Now.ToLongTimeString();
		}
	}
}

