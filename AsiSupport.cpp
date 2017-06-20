#include "stdafx.h"

#include "AsiSupport.h"

using namespace Rage::Native;
using namespace System::Reflection;

/*Entry point for the RPH plugin*/
void AsiSupport::EntryPoint::Main()
{
	Vector3Natives::Initialize();
	KeyboardManager::Initialize();
	TexturesManager::Initialize();
	loader = gcnew AsiLoader(Directory::GetParent(Path::GetFullPath("child"))->FullName);
	loader->Initialize();
	Rage::GameFiber::Hibernate();
}

void AsiSupport::EntryPoint::ListAsiPlugins()
{
	Log::DisplayLine("___Loaded ASI plugins list___");

	for(int i = 0; i < loader->LoadedPlugins->Count; i++)
		Log::DisplayLine(Log::ToUnmanaged(Path::GetFileNameWithoutExtension(loader->LoadedPlugins[i]->FileName)));
}

void AsiSupport::EntryPoint::LoadAsiPlugin(String^ name)
{
	loader->LoadPlugin(Path::Combine(loader->WorkingDir, name) + ".asi");
}

void AsiSupport::EntryPoint::UnloadAsiPlugin(String ^ name)
{
	Plugin^ plugin = loader->GetPlugin(name);

	if(plugin != nullptr)
		loader->UnloadPlugin(plugin);
}

void AsiSupport::EntryPoint::AsiSupportVersion()
{
	Log::Info("AsiSupport for RagePluginHook 0.1.0 BETA - by Pursuit");
}

/*Disposing the RPH plugin*/
void AsiSupport::EntryPoint::OnUnload(bool isTerminating)
{
	Log::Warn("AsiSupport is getting unloaded. You will have to reboot your game for ASI plugins to work again");
	
	if(loader != nullptr)
		loader->UnloadAllPlugins();
	
	KeyboardManager::Dispose();
}