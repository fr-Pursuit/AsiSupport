using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsiSupport.ASI;
using PursuitLib;
using Rage.Attributes;
using Rage.ConsoleCommands;
using Rage.ConsoleCommands.AutoCompleters;

namespace AsiSupport
{
	public static class ConsoleCommands
	{
		[ConsoleCommand(Description = "Load a specific ASI Plugin")]
		public static void LoadAsiPlugin([ConsoleCommandParameter(Description = "The plugin's name")] string name)
		{
			Support.Instance.Loader.LoadPlugin(name);
		}

		[ConsoleCommand(Description = "Unload a specific ASI Plugin")]
		public static void UnloadAsiPlugin([ConsoleCommandParameter(Description = "The plugin's name")] string name)
		{
			Support.Instance.Loader.UnloadPlugin(name);
		}

		[ConsoleCommand(Description = "Load all existing ASI plugins")]
		public static void LoadAllAsiPlugins()
		{
			Support.Instance.Loader.LoadAllPlugins();
		}

		[ConsoleCommand(Description = "Unload all existing ASI plugins")]
		public static void UnloadAllAsiPlugins()
		{
			Support.Instance.Loader.UnloadAllPlugins();
		}
	}
}
