#include "stdafx.h"
#include "Log.h"

using namespace Rage;

void Log::Info(string message)
{
	DisplayLine("[INFO] " + message);
}

void Log::Warn(string message)
{
	DisplayLine("[WARN] " + message);
}

void Log::Error(string message)
{
	DisplayLine("[ERROR] " + message);
}

void Log::DisplayLine(string message)
{
	Game::LogTrivial(gcnew System::String(message.c_str()));
}