#pragma once

namespace AsiSupport
{
	public ref class AsiLoader
	{
		public:
		static AsiLoader^ instance;

		property String^ WorkingDir;
		property bool Loading;
		property List<Plugin^>^ LoadedPlugins;
		property AsiSupport::IntegrityMap^ IntegrityMap;

		AsiLoader(String^ workingDir);

		void Initialize();

		bool IsLoaded(String^ path);

		void LoadPlugin(String^ path);

		Plugin^ GetPlugin(String^ name);

		Plugin^ GetPlugin(HMODULE pluginModule);

		void UnloadPlugin(Plugin^ plugin);

		void UnloadAllPlugins();
	};
}