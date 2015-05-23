using System;
using System.Collections.Generic;

namespace SpO2App.Droid
{
	public class CMS50IWCommand
	{

		public static readonly CMS50IWCommand START_DATA = new CMS50IWCommand (((byte)0xA1));// 161
		public static readonly CMS50IWCommand STOP_DATA = new CMS50IWCommand (((byte)0xA2));// 162
		public static readonly CMS50IWCommand STAY_CONNECTED = new CMS50IWCommand (((byte)0xAF));// 175
		public static readonly CMS50IWCommand SEND_USER_INFORMATION = new CMS50IWCommand (((byte)0xAB));// 171
		public static readonly CMS50IWCommand PADDING = new CMS50IWCommand (((byte)0x80));// 128
		public static readonly CMS50IWCommand COMMAND_FOLLOWS = new CMS50IWCommand (((byte)0x7D));// 125

		private readonly byte command;


		public static IEnumerable<CMS50IWCommand> Values
		{
			get
			{
				yield return START_DATA;
				yield return STOP_DATA;
				yield return STAY_CONNECTED;
				yield return SEND_USER_INFORMATION;
				yield return PADDING;
				yield return COMMAND_FOLLOWS;
			}
		}

		public CMS50IWCommand(byte command) {
			this.command = command;
		}

		public int asInt() {
			return (int) command;
		}

		public byte asByte(){
			return command;
		}
//		public int getLength(){
//			return command;
//		}

	}
}

