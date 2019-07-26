using PursuitLib.IO;

namespace AsiSupport
{
	public class Config : ConfigFile
	{
		public bool LoadAllPluginsOnStartup { get; set; } = true;

		public bool ConsiderVersionUnknown { get; set; } = false;

		public bool IgnoreUnknownNatives { get; set; } = false;

		public bool EnableSHVDNSupport { get; set; } = true; //TODO: false by default?

		public Config() : base(Support.Instance.ConfigFile, true) {}
	}
}