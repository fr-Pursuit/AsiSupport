using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PursuitLib.IO;

namespace AsiSupport
{
	public class Config : ConfigFile
	{
		public bool LoadAllPluginsOnStartup { get; set; } = true;

		public Config() : base(Support.Instance.ConfigFile, true) {}
	}
}