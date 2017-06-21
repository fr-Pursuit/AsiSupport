#include "stdafx.h"

AsiSupport::AsiLoader::AsiLoader(String^ workingDir)
{
	instance = this;

	this->WorkingDir = workingDir;
	this->Loading = false;
	this->LoadedPlugins = gcnew List<Plugin^>();
	this->IntegrityMap = gcnew AsiSupport::IntegrityMap();
}

void AsiSupport::AsiLoader::Initialize()
{
	Log::Info("Loading ASI plugins");

	for each(String^ file in Directory::EnumerateFiles(this->WorkingDir, "*.rasi"))
	{
		LoadPlugin(file);
		Rage::GameFiber::Sleep(0);
	}

	for each(String^ file in Directory::EnumerateFiles(this->WorkingDir, "*.asi"))
	{
		if(!File::Exists(Path::GetFileNameWithoutExtension(file) + ".rasi"))
		{
			LoadPlugin(file);
			Rage::GameFiber::Sleep(0);
		}
	}

	this->IntegrityMap->Save();
	Log::Info("Finished loading ASI plugins");
}

bool AsiSupport::AsiLoader::IsLoaded(String^ name)
{
	for each(Plugin^ plugin in this->LoadedPlugins)
	{
		if(Path::GetFileNameWithoutExtension(plugin->FileName)->Equals(name))
			return true;
	}

	return false;
}

void AsiSupport::AsiLoader::LoadPlugin(String^ path)
{
	this->Loading = true;
	String^ managedName = Path::GetFileNameWithoutExtension(path);
	const string name = Util::ToUnmanaged(managedName);

	if(this->IsLoaded(managedName))
		Log::Info("Plugin \"" + name + "\" already loaded.");
	else if(name == "OpenIV")
		Log::Info("Skipping \"OpenIV\" as it is a stand-alone plugin.");
	else
	{
		string pluginPath = Util::ToUnmanaged(path);

		Log::Info("Loading \"" + name + '"');

		//In case ScriptHookV is here and has loaded the ASI already
		Rage::Game::TerminateAllScriptsWithName(managedName->ToLower() + ".asi");

		if(path->EndsWith(".rasi") && File::Exists(path->Replace(".rasi", ".asi")))
		{
			String^ hash = Util::GetFileChecksum(path->Replace(".rasi", ".asi"));
			String^ oldHash = this->IntegrityMap->GetEntry(managedName);

			if(oldHash == nullptr || hash != oldHash)
			{
				Log::Info("Updating \"" + name + '"');
				File::Delete(path);

				path = path->Replace(".rasi", ".asi");
				pluginPath = Util::ToUnmanaged(path);
			}
		}

		if(path->EndsWith(".asi"))
		{
			Log::Info("Regular ASI detected. Checking compatibility...");

			ASIImage pluginImage;

			if(!pluginImage.Load(pluginPath))
			{
				Log::Error("Unable to load \"" + name + "\" image.");
				this->Loading = false;
				return;
			}

			Rage::GameFiber::Sleep(0);

			if(!pluginImage.IsCompatible())
			{
				Log::Info("Detected ScriptHookV ASI. Creating Rage ASI version...");

				if(pluginImage.CreateRASI())
				{
					this->IntegrityMap->SetEntry(gcnew String(name.c_str()), Util::GetFileChecksum(path));
					path = path->Replace(".asi", ".rasi");
					pluginPath = Util::ToUnmanaged(path);
					Log::Info("Successfully created RASI version of \"" + name + '"');
				}
				else
				{
					Log::Error("Failed to create Rage ASI version of plugin \"" + name + '"');
					this->Loading = false;
					return;
				}
			}

			Rage::GameFiber::Sleep(0);
		}

		Plugin^ currentPlugin = gcnew Plugin(pluginPath);
		this->LoadedPlugins->Add(currentPlugin);
		HMODULE module = LoadLibraryA(pluginPath.c_str());

		if(module)
		{
			Log::Info("Plugin \"" + name + "\" loaded successfully.");

			for each(PluginThread^ thread in currentPlugin->ScriptThreads)
				thread->Start();
		}
		else
		{
			this->LoadedPlugins->Remove(currentPlugin);
			Log::Error("Unable to load \"" + name + "\" (error code " + to_string(GetLastError()) + "), try rebooting your game.");
		}
	}

	this->Loading = false;
}

AsiSupport::Plugin^ AsiSupport::AsiLoader::GetPlugin(String ^ name)
{
	for each(Plugin^ plugin in this->LoadedPlugins)
	{
		if(Path::GetFileNameWithoutExtension(plugin->FileName)->Equals(name))
			return plugin;
	}

	return nullptr;
}

AsiSupport::Plugin^ AsiSupport::AsiLoader::GetPlugin(HMODULE pluginModule)
{
	String^ pluginPath;
	char pluginPathArray[MAX_PATH];
	GetModuleFileNameA(pluginModule, pluginPathArray, MAX_PATH);
	pluginPath = gcnew String(pluginPathArray);

	for each(Plugin^ plugin in this->LoadedPlugins)
	{
		if(pluginPath->Equals(plugin->FileName))
			return plugin;
	}

	return nullptr;
}

void AsiSupport::AsiLoader::UnloadPlugin(Plugin^ plugin)
{
	string name = Util::ToUnmanaged(Path::GetFileNameWithoutExtension(plugin->FileName));

	Log::Info("Unloading \"" + name +'"');

	for each(PluginThread^ thread in plugin->ScriptThreads)
		thread->Fiber->Abort();

	string fileName = Util::ToUnmanaged(plugin->FileName);

	this->LoadedPlugins->Remove(plugin);

	Log::Info("You will have to reboot your game for the plugin to work again.");

	// \/ this is causing a crash...
	/*if(FreeLibrary(GetModuleHandleA(fileName.c_str())))
		Log::Info("Plugin \"" + name + "\" unloaded successfully.");
	else
		Log::Info("Unable to unload plugin \"" + name + "\" (error code " + to_string(GetLastError()) + ")");*/
}

void AsiSupport::AsiLoader::UnloadAllPlugins()
{
	List<Plugin^>^ plugins = gcnew List<Plugin^>(this->LoadedPlugins);

	for each(Plugin^ plugin in plugins)
		this->UnloadPlugin(plugin);
}
