#include "stdafx.h"
#include "Vector3NativesList.h"
#define DLL_EXPORT __declspec( dllexport )

using namespace AsiSupport;
using namespace System::Runtime::InteropServices;
using namespace System::Threading;
using namespace Rage::Native;

static UINT64 nativeHash;
static UINT64 arguments[25];
static int argumentsIndex = 0;
static UINT64 returnedValue;

/* textures */

// Create texture
//	texFileName	- texture file name, it's best to specify full texture path and use PNG textures
//	returns	internal texture id
//	Texture deletion is performed automatically when game reloads scripts
//	Can be called only in the same thread as natives

DLL_EXPORT int createTexture(const char *texFileName)
{
	return TexturesManager::CreateTexture(gcnew String(texFileName));
}

// Draw texture
//	id		-	texture id recieved from createTexture()
//	index	-	each texture can have up to 64 different instances on screen at one time - does nothing
//	level	-	draw level, being used in global draw order, texture instance with least level draws first - does nothing
//	time	-	how much time (ms) texture instance will stay on screen, the amount of time should be enough
//				for it to stay on screen until the next corresponding drawTexture() call
//	sizeX,Y	-	size in screen space, should be in the range from 0.0 to 1.0, e.g setting this to 0.2 means that
//				texture instance will take 20% of the screen space
//	centerX,Y -	center position in texture space, e.g. 0.5 means real texture center
//	posX,Y	-	position in screen space, [0.0, 0.0] - top left corner, [1.0, 1.0] - bottom right,
//				texture instance is positioned according to it's center
//	rotation -	should be in the range from 0.0 to 1.0
//	screenHeightScaleFactor - screen aspect ratio, used for texture size correction, you can get it using natives
//	r,g,b,a	-	color, should be in the range from 0.0 to 1.0 - does nothing
//
//	Texture instance draw parameters are updated each time script performs corresponding call to drawTexture()
//	You should always check your textures layout for 16:9, 16:10 and 4:3 screen aspects, for ex. in 1280x720,
//	1440x900 and 1024x768 screen resolutions, use windowed mode for this
//	Can be called only in the same thread as natives

DLL_EXPORT void drawTexture(int id, int index, int level, int time,
	float sizeX, float sizeY, float centerX, float centerY,
	float posX, float posY, float rotation, float screenHeightScaleFactor,
	float r, float g, float b, float a)
{
	TexturesManager::DrawTexture(id, time, posX, posY, sizeX, sizeY * screenHeightScaleFactor, rotation, centerX, centerY);
}

// IDXGISwapChain::Present callback
// Called right before the actual Present method call, render test calls don't trigger callbacks
// When the game uses DX10 it actually uses DX11 with DX10 feature level
// Remember that you can't call natives inside
// void OnPresent(IDXGISwapChain *swapChain);
typedef void(*PresentCallback)(void *);

// Register IDXGISwapChain::Present callback
// must be called on dll attach
DLL_EXPORT void presentCallbackRegister(PresentCallback cb) //Not implemented
{
	Log::Info("Present callbacks are not supported.");
}

// Unregister IDXGISwapChain::Present callback
// must be called on dll detach
DLL_EXPORT void presentCallbackUnregister(PresentCallback cb) {} //Not implemented

/* keyboard */

// Register keyboard handler
// must be called on dll attach
DLL_EXPORT void keyboardHandlerRegister(KeyboardHandler handler)
{
	KeyboardManager::RegisterHandler(handler);
}

// Unregister keyboard handler
// must be called on dll detach
DLL_EXPORT void keyboardHandlerUnregister(KeyboardHandler handler)
{
	KeyboardManager::UnregisterHandler(handler);
}

/* scripts */

DLL_EXPORT void scriptWait(DWORD time)
{
	Rage::GameFiber::Wait(time);
}

DLL_EXPORT void scriptRegister(HMODULE module, void(*LP_SCRIPT_MAIN)())
{
	Plugin^ plugin = AsiLoader::instance->GetPlugin(module);

	if(plugin != nullptr)
		plugin->ScriptThreads->Add(gcnew PluginThread(Path::GetFileNameWithoutExtension(plugin->FileName) + "-main", LP_SCRIPT_MAIN));
	else Log::Warn("Unable to register script: invalid plugin");
}

DLL_EXPORT void scriptRegisterAdditionalThread(HMODULE module, void(*LP_SCRIPT_MAIN)())
{
	Plugin^ plugin = AsiLoader::instance->GetPlugin(module);

	if(plugin != nullptr)
		plugin->ScriptThreads->Add(gcnew PluginThread(Path::GetFileNameWithoutExtension(plugin->FileName) + "-additional", LP_SCRIPT_MAIN));
	else Log::Warn("Unable to register additional script thread: invalid plugin");
}

DLL_EXPORT void scriptUnregister(HMODULE module) {} //Not supported

DLL_EXPORT void scriptUnregister(void(*LP_SCRIPT_MAIN)()) {}// deprecated

DLL_EXPORT void nativeInit(UINT64 hash)
{
	nativeHash = hash;
}

DLL_EXPORT void nativePush64(UINT64 val)
{
	arguments[argumentsIndex] = val;
	argumentsIndex++;
}

DLL_EXPORT PUINT64 nativeCall()
{
	cli::array<NativeArgument^>^ args = gcnew cli::array<NativeArgument^>(argumentsIndex);

	for(int i = 0; i < argumentsIndex; i++)
		args[i] = gcnew NativeArgument(arguments[i]);

	argumentsIndex = 0;

	if(AsiSupport::Vector3Natives::Vector3NativesList->Contains(nativeHash))
	{
		Vector3 vec = *reinterpret_cast<Vector3 *>(&returnedValue);
		vec = Vector3();
		Rage::Vector3 rageVector = (Rage::Vector3)NativeFunction::Call(nativeHash, Rage::Vector3::typeid, args);
		vec.x = rageVector.X;
		vec.y = rageVector.Y;
		vec.z = rageVector.Z;
		return reinterpret_cast<PUINT64>(&vec);
	}
	else
		returnedValue = (UINT64)NativeFunction::Call(nativeHash, System::UInt64::typeid, args);

	return &returnedValue;
}

// Returns pointer to global variable
// make sure that you check game version before accessing globals because
// ids may differ between patches
DLL_EXPORT UINT64 *getGlobalPtr(int globalId)
{
	return (UINT64*)Rage::Game::GetScriptGlobalVariableAddress(globalId).ToPointer();
}

/* world */

// Get entities from internal pools
// return value represents filled array elements count
// can be called only in the same thread as natives
DLL_EXPORT int worldGetAllVehicles(int *arrPointer, int arrSize)
{
	int count = 0;

	for each(Rage::Vehicle^ veh in Rage::World::EnumerateVehicles())
	{
		if(count == arrSize)
			break;

		arrPointer[count] = (int)veh->Handle.Value;
		count++;
	}

	return count;
}

DLL_EXPORT int worldGetAllPeds(int *arrPointer, int arrSize)
{
	int count = 0;

	for each(Rage::Ped^ ped in Rage::World::EnumeratePeds())
	{
		if(count == arrSize)
			break;

		arrPointer[count] = (int)ped->Handle.Value;
		count++;
	}

	return count;
}

DLL_EXPORT int worldGetAllObjects(int *arrPointer, int arrSize)
{
	int count = 0;

	for each(Rage::Object^ obj in Rage::World::EnumerateObjects())
	{
		if(count == arrSize)
			break;

		arrPointer[count] = (int)obj->Handle.Value;
		count++;
	}

	return count;
}

/*Thanks to alexguirre and SHVDN for this !*/
DLL_EXPORT int worldGetAllPickups(int *arrPointer, int arrSize)
{
	int count = 0;

	List<Rage::PoolHandle>^ pickupList = PickupLister::GetAllPickupHandles();

	for each(Rage::PoolHandle pickup in pickupList)
	{
		if(count == arrSize)
			break;

		arrPointer[count] = (int)pickup.Value;
		count++;
	}

	return count;
}

/* misc */

// Returns base object pointer using it's script handle
// make sure that you check game version before accessing object fields because
// offsets may differ between patches
DLL_EXPORT BYTE *getScriptHandleBaseAddress(int handle)
{
	return reinterpret_cast<BYTE *>(Rage::World::GetEntityByHandle<Rage::Entity^>(Rage::PoolHandle((unsigned int)handle))->MemoryAddress.ToPointer());
}

enum eGameVersion : int
{
	VER_1_0_335_2_STEAM,
	VER_1_0_335_2_NOSTEAM,

	VER_1_0_350_1_STEAM,
	VER_1_0_350_2_NOSTEAM,

	VER_1_0_372_2_STEAM,
	VER_1_0_372_2_NOSTEAM,

	VER_1_0_393_2_STEAM,
	VER_1_0_393_2_NOSTEAM,

	VER_1_0_393_4_STEAM,
	VER_1_0_393_4_NOSTEAM,

	VER_1_0_463_1_STEAM,
	VER_1_0_463_1_NOSTEAM,

	VER_1_0_505_2_STEAM,
	VER_1_0_505_2_NOSTEAM,

	VER_1_0_573_1_STEAM,
	VER_1_0_573_1_NOSTEAM,

	VER_1_0_617_1_STEAM,
	VER_1_0_617_1_NOSTEAM,

	VER_SIZE,
	VER_UNK = -1
};

DLL_EXPORT eGameVersion getGameVersion()
{
	if(File::Exists(Path::Combine(AsiSupport::AsiLoader::instance->WorkingDir, "steam_api64.dll")))
		return VER_1_0_617_1_STEAM;
	else return VER_1_0_617_1_NOSTEAM;
}
