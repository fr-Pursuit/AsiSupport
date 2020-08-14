using PursuitLib.IO;

namespace AsiSupport
{
	public class Config : ConfigFile
	{
		public bool LoadAllPluginsOnStartup { get; set; } = true;

		public bool NotifyFiberCrash { get; set; } = true;

		public bool ConsiderVersionUnknown { get; set; } = false;

		public bool IgnoreUnknownNatives { get; set; } = true;

		public Config() : base(Support.Instance.ConfigFile, true) {}
	}
}