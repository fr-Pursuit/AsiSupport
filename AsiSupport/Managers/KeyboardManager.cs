using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PursuitLib;
using Rage;

namespace AsiSupport.Managers
{
	public class KeyboardManager : NativeWindow
	{
		public delegate void KeyboardHandler(uint key, ushort repeats, byte scanCode, bool isExtended, bool isWithAlt, bool wasDownBefore, bool isUpNow);

		private const int GwlpWndproc = -4;
		private const int WmKeydown = 0x0100;
		private const int WmKeyup = 0x0101;
		private const int WmSyskeydown = 0x0104;
		private const int WmSyskeyup = 0x0105;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		private List<KeyboardHandler> handlers;

		public KeyboardManager()
		{
			IntPtr windowHandle = IntPtr.Zero;

			while(windowHandle == IntPtr.Zero)
			{
				windowHandle = FindWindow("grcWindow", null);
				GameFiber.Sleep(100);
			}

			this.AssignHandle(windowHandle);
			this.handlers = new List<KeyboardHandler>();

			Log.Info("KeyboardManager initialized");
		}

		public void RegisterHandler(KeyboardHandler handler)
		{
			Log.Info("Registering keyboard handler");
			this.handlers.Add(handler);
		}

		public void UnregisterHandler(KeyboardHandler handler)
		{
			this.handlers.Remove(handler);
		}

		protected override void WndProc(ref Message m)
		{
			if(!Game.Console.IsOpen && (m.Msg == WmKeydown || m.Msg == WmKeyup || m.Msg == WmSyskeydown || m.Msg == WmSyskeyup))
			{
				foreach(KeyboardHandler handler in this.handlers)
					handler((uint)m.WParam, (ushort)((ulong)m.LParam & 0xFFFF), (byte)(((ulong)m.LParam >> 16) & 0xFF), (((ulong)m.LParam >> 24) & 1) == 1, (m.Msg == WmSyskeydown || m.Msg == WmSyskeyup), (((ulong)m.LParam >> 30) & 1) == 1, (m.Msg == WmSyskeyup || m.Msg == WmKeyup));
			}

			base.WndProc(ref m);
		}
	}
}