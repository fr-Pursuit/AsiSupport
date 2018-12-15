using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PursuitLib;
using PursuitLib.IO;
using Rage;

namespace AsiSupport.ASI
{
	public class AsiLoader
	{
		private const int MaxPath = 256;

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibraryA(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetModuleFileName(IntPtr hModule, StringBuilder filename, int nameSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeLibrary(IntPtr hModule);

		public string WorkingDir { get; private set; }
		public bool Loading { get; private set; }
		public List<AsiPlugin> LoadedPlugins { get; } = new List<AsiPlugin>();
		private readonly IntegrityMap integrityMap;

		public AsiLoader(string workingDir)
		{
			Log.Info("Initializing AsiLoader...");
			this.WorkingDir = Path.GetFullPath(workingDir);
			this.Loading = false;
			this.integrityMap = new IntegrityMap(Path.Combine(Support.Instance.DataDirectory, "Conversions"));
			this.integrityMap.Cleanup(this.WorkingDir);

			if(!Directory.Exists(this.WorkingDir))
				Directory.CreateDirectory(this.WorkingDir);
		}

		public void LoadAllPlugins()
		{
			Log.Info("Loading ASI plugins");

			foreach(string file in Directory.EnumerateFiles(Support.Instance.WorkingDirectory, "*.asi"))
			{
				string name = Path.GetFileNameWithoutExtension(file);

				if(!this.IsLoaded(name))
				{
					LoadPlugin(new AsiPlugin(name));
					GameFiber.Yield();
				}
			}

			foreach(string file in Directory.EnumerateFiles(this.WorkingDir, "*.uasi"))
			{
				string name = Path.GetFileNameWithoutExtension(file);

				if(!this.IsLoaded(name))
				{
					LoadPlugin(new AsiPlugin(name));
					GameFiber.Yield();
				}
			}

			this.integrityMap.Save();
			Log.Info("Finished loading ASI plugins");
		}

		public bool IsLoaded(string name)
		{
			foreach(AsiPlugin plugin in this.LoadedPlugins)
			{
				if(Path.GetFileNameWithoutExtension(plugin.UASIPath) == name)
					return true;
			}

			return false;
		}

		public void LoadPlugin(string name)
		{
			if(!this.IsLoaded(name))
			{
				if(AsiPlugin.Exists(name))
					this.LoadPlugin(new AsiPlugin(name));
				else Log.Info("Cannot load plugin \"" + name + "\" as it doesn't exist");
			}
			else Log.Info("Plugin \"" + name + "\" is already loaded.");
		}

		public void LoadPlugin(AsiPlugin plugin)
		{
			this.Loading = true;
			Log.Info("Loading \"" + plugin.Name + '"');

			try
			{
				//In case ScriptHookV is present and has already loaded the ASI
				Game.TerminateAllScriptsWithName(plugin.Name.ToLower() + ".asi");

				if(plugin.Type == AsiType.NonUniversal)
				{
					Log.Info("Non universal ASI detected. Converting it...");
					plugin.ConvertAsi();
					this.integrityMap.UpdateConversionHash(plugin.Name, IOUtil.GetFileChecksum(plugin.Name + ".asi"), IOUtil.GetFileChecksum(plugin.UASIPath));
					Log.Info("Plugin converted successfully.");
					GameFiber.Yield();
				}
				else if(plugin.Type == AsiType.UniversalConverted)
				{
					bool outdated = false;

					if(this.integrityMap.HasConversionHash(plugin.Name))
						outdated = IOUtil.GetFileChecksum(plugin.Name + ".asi") != this.integrityMap.GetAsiHash(plugin.Name) || IOUtil.GetFileChecksum(plugin.UASIPath) != this.integrityMap.GetUnivHash(plugin.Name);
					else outdated = true;

					if(outdated)
					{
						Log.Info("Outdated ASI detected. Converting it again...");
						plugin.ConvertAsi();
						this.integrityMap.UpdateConversionHash(plugin.Name, IOUtil.GetFileChecksum(plugin.Name + ".asi"), IOUtil.GetFileChecksum(plugin.UASIPath));
						Log.Info("Plugin converted successfully.");
					}

					GameFiber.Yield();
				}

				this.LoadedPlugins.Add(plugin);
				IntPtr module = LoadLibraryA(plugin.UASIPath);

				if(module != IntPtr.Zero)
				{
					if(plugin.ScriptThreads.Count > 0)
					{
						Log.Info("Plugin \"" + plugin.Name + "\" loaded successfully.");

						foreach(AsiThread thread in plugin.ScriptThreads)
							thread.Start();
					}
					else Log.Warn("Plugin \"" + plugin.Name + "\" has been loaded successfully, but didn't register any thread. Try rebooting your game to fix this issue.");
				}
				else
				{
					this.LoadedPlugins.Remove(plugin);
					Log.Error("Unable to load \"" + plugin.Name + "\", try rebooting your game: " + new Win32Exception(Marshal.GetLastWin32Error()));
				}

				this.Loading = false;

				if(this.integrityMap.Dirty)
					this.integrityMap.SaveMap();
			}
			catch(NotScriptException)
			{
				Log.Info("Skipping \"" + plugin.Name + "\" as it is not a ScriptHookV script.");
			}
			catch(Exception e)
			{
				if(this.LoadedPlugins.Contains(plugin))
					this.LoadedPlugins.Remove(plugin);

				Log.Error("Unable to load \"" + plugin.Name + "\": " + e);
			}
		}

		public AsiPlugin GetPlugin(string name)
		{
			foreach(AsiPlugin plugin in this.LoadedPlugins)
			{
				if(Path.GetFileNameWithoutExtension(plugin.UASIPath) == name)
					return plugin;
			}

			return null;
		}

		public AsiPlugin GetPlugin(IntPtr pluginModule)
		{
			if(pluginModule != IntPtr.Zero)
			{
				StringBuilder pathBuilder = new StringBuilder(MaxPath);
				GetModuleFileName(pluginModule, pathBuilder, MaxPath);
				string pluginPath = pathBuilder.ToString();

				foreach(AsiPlugin plugin in this.LoadedPlugins)
				{
					if(pluginPath.Equals(plugin.UASIPath))
						return plugin;
				}
			}

			return null;
		}

		public void UnloadPlugin(string name)
		{
			if(this.IsLoaded(name))
				this.UnloadPlugin(this.GetPlugin(name));
			else Log.Info("Plugin \"" + name + "\" not loaded.");
		}

		public void UnloadPlugin(AsiPlugin plugin)
		{
			string name = plugin.Name;

			Log.Info("Unloading \"" + name + '"');

			foreach(AsiThread thread in plugin.ScriptThreads)
			{
				if(thread.Fiber.IsAlive)
					thread.Fiber.Abort();
			}

			IntPtr module = plugin.Module;

			if(module != IntPtr.Zero)
			{
				if(FreeLibrary(module))
					Log.Info("Plugin \"" + name + "\" unloaded successfully.");
				else Log.Error("Unable to unload plugin \"" + name + "\", you will need to reboot your game to load it again: " + new Win32Exception(Marshal.GetLastWin32Error()));
			}
			else Log.Warn("Unable to release plugin module. Either the module was never loaded, or you will need to reboot your game to load it again.");

			this.LoadedPlugins.Remove(plugin);
		}

		public void UnloadAllPlugins()
		{
			Log.Info("Unloading all plugins...");
			List<AsiPlugin> plugins = new List<AsiPlugin>(this.LoadedPlugins);

			foreach(AsiPlugin plugin in plugins)
				this.UnloadPlugin(plugin);
		}
	}
}
