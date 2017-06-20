#include "Stdafx.h"

using namespace System::Runtime::InteropServices;
using namespace System::Threading;

AsiSupport::PluginThread::PluginThread(String^ name, void(*entryPoint)())
{
	this->Name = name;
	this->start = (Action^)Marshal::GetDelegateForFunctionPointer(IntPtr(entryPoint), Action::typeid);
	
	if(AsiLoader::instance->Loading) //Don't start fibers during loading, they will be started after
		this->Fiber = nullptr;
	else this->Fiber = Rage::GameFiber::StartNew(gcnew ThreadStart(this, &PluginThread::RunFiber), this->Name);
}

void AsiSupport::PluginThread::Start()
{
	if(this->start != nullptr && this->Fiber == nullptr)
		this->Fiber = Rage::GameFiber::StartNew(gcnew ThreadStart(this, &PluginThread::RunFiber), this->Name);
	else Log::Warn("Unable to start PluginThread " + Log::ToUnmanaged(this->Name));
}

void AsiSupport::PluginThread::RunFiber()
{
	Log::Info("Starting new GameFiber " + Log::ToUnmanaged(this->Name));
	this->start->Invoke();
}
