#include "stdafx.h"

AsiSupport::Plugin::Plugin(string fileName)
{
	this->FileName = gcnew String(fileName.c_str());
	this->ScriptThreads = gcnew List<PluginThread^>();
}

HMODULE AsiSupport::Plugin::GetModule()
{
	return GetModuleHandleA(Util::ToUnmanaged(this->FileName).c_str());
}
