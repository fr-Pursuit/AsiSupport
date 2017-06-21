#pragma once
#pragma optimize("", off)

#define WINVER 0x0601 //Windows 7
#define _WIN32_WINNT 0x0601

#include <stdio.h>
#include <windows.h>
#include <strsafe.h>
#include <iostream>
#include <string>
#include <list>
#include <vector>
#include <fstream>
#include <msclr\marshal_cppstd.h>

using namespace std;
using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Threading;

// DWORD key, WORD repeats, BYTE scanCode, BOOL isExtended, BOOL isWithAlt, BOOL wasDownBefore, BOOL isUpNow
typedef void(*KeyboardHandler)(DWORD, WORD, BYTE, BOOL, BOOL, BOOL, BOOL);

#include "Util.h"
#include "Log.h"
#include "KeyboardManager.h"
#include "TexturesManager.h"
#include "PickupLister.h"
#include "PluginThread.h"
#include "Plugin.h"
#include "IntegrityMap.h"
#include "ASIImage.h"
#include "AsiLoader.h"
#include "Vector3Natives.h"
