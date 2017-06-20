#pragma once

namespace AsiSupport
{
	public ref class Plugin
	{
		public:
		property String^ FileName;
		property List<PluginThread^>^ ScriptThreads;

		Plugin(string fileName);

		HMODULE GetModule();
	};
}
