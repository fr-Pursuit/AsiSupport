using PursuitLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Rage;

namespace AsiSupport
{
	/// <summary>
	/// Adapted from ScriptHookVDotNet's code
	/// https://github.com/crosire/scripthookvdotnet/blob/853982b99c2ef33b44cf702277a3a05e0087d149/LoaderMain.cpp
	/// </summary>
	public static class SHVDN
	{
		private delegate void KeyboardMethodDelegate(Keys key, bool status, bool statusCtrl, bool statusShift, bool statusAlt);

		public static bool IsActive => fiber != null && fiber.IsAlive;

		private static GameFiber fiber;
		private static List<Func<bool>> initMethods = new List<Func<bool>>();
		private static List<Action> tickMethods = new List<Action>();
		private static List<KeyboardMethodDelegate> keyboardMethods = new List<KeyboardMethodDelegate>();

		public static void Init()
		{
			Log.Info("SHVDN", "Loading SHVDN support...");
			Support.Instance.KeyboardManager.RegisterHandler(SendKeyboardMessage);

			initMethods.Clear();
			tickMethods.Clear();
			keyboardMethods.Clear();

			foreach(string filename in Directory.EnumerateFiles(Support.Instance.WorkingDirectory, "ScriptHookVDotNet-univ*.dll"))
			{
				Log.Info("File: " + filename);

				Assembly assembly;

				try
				{
					// Unblock file if it was downloaded from a network location
					if(File.Exists(filename + ":Zone.Identifier"))
						File.Delete(filename + ":Zone.Identifier");

					assembly = Assembly.LoadFrom(filename);
				}
				catch(Exception ex)
				{
					Log.Error("SHVDN", "FATAL: Unable to load '" + filename + "' due to the following exception:\n\n" + ex);
					continue;
				}

				Type main = assembly.GetType("ScriptHookVDotNet");

				if(main != null && main.IsAbstract)
				{
					Func<bool> initMethod = (Func<bool>) main.GetMethod("Init", BindingFlags.Public | BindingFlags.Static).CreateDelegate(typeof(Func<bool>));
					initMethods.Add(initMethod);
					Action tickMethod = (Action) (main.GetMethod("Tick", BindingFlags.Public | BindingFlags.Static).CreateDelegate(typeof(Action)));
					tickMethods.Add(tickMethod);
					KeyboardMethodDelegate keyboardMessageMethod = (KeyboardMethodDelegate) (main.GetMethod("KeyboardMessage", BindingFlags.Public | BindingFlags.Static).CreateDelegate(typeof(KeyboardMethodDelegate)));
					keyboardMethods.Add(keyboardMessageMethod);
				}
			}

			foreach(Func<bool> Init in initMethods)
				Init();

			fiber = new GameFiber(RunFiber);
			fiber.Start();
		}

		public static void Dispose()
		{
			if(fiber != null)
			{
				fiber.Abort();
				fiber = null;
			}

			Support.Instance.KeyboardManager.UnregisterHandler(SendKeyboardMessage);
		}

		private static void RunFiber()
		{
			while(true)
			{
				foreach(Action Tick in tickMethods)
					Tick();

				GameFiber.Yield();
			}
		}

		private static void SendKeyboardMessage(uint key, ushort repeats, byte scanCode, bool isExtended, bool isWithAlt, bool wasDownBefore, bool isUpNow)
		{
			foreach(KeyboardMethodDelegate KeyboardMethod in keyboardMethods)
				KeyboardMethod((Keys)key, !isUpNow, Game.IsControlKeyDownRightNow, Game.IsShiftKeyDownRightNow, Game.IsAltKeyDownRightNow);
		}
	}
}
