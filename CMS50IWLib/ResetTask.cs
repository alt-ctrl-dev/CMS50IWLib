using System;
using Java.Lang;

namespace SpO2App.Droid
{
	public class ResetTask: Java.Lang.Object, IRunnable
	{
		private AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents = null;

		public ResetTask(AndroidBluetoothConnectionComponents androidBluetoothConnectionComponents) {
			this.androidBluetoothConnectionComponents = androidBluetoothConnectionComponents;
		}
			
		public void Run() {
			androidBluetoothConnectionComponents.reset();
		}
	}
}

