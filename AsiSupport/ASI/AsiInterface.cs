using System;
using System.Runtime.InteropServices;
using System.Text;
using AsiSupport.Managers;
using PursuitLib;
using Rage;
using Rage.Native;

namespace AsiSupport.ASI
{
	public static unsafe class AsiInterface
	{
		[DllImport("UnvAsiIntrf.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int RegisterHandler(IntPtr argCreateTexture, IntPtr argDrawTexture, IntPtr argPresentCallbackRegister, IntPtr argPresentCallbackUnregister, IntPtr argKeyboardHandlerRegister, IntPtr argKeyboardHandlerUnregister, IntPtr argScriptWait, IntPtr argScriptRegister, IntPtr argScriptRegisterAdditionalThread, IntPtr argScriptUnregisterByModule, IntPtr argScriptUnregisterByMain, IntPtr argNativeInit, IntPtr argNativePush64, IntPtr argNativeCall, IntPtr argGetGlobalPtr, IntPtr argWorldGetAllVehicles, IntPtr argWorldGetAllPeds, IntPtr argWorldGetAllObjects, IntPtr argWorldGetAllPickups, IntPtr argGetScriptHandleBaseAddress, IntPtr argGetGameVersion);

		private delegate int CreateTextureDelegate(string texFileName);
		private delegate void DrawTextureDelegate(int id, int index, int level, int time, float sizeX, float sizeY, float centerX, float centerY, float posX, float posY, float rotation, float screenHeightScaleFactor, float r, float g, float b, float a);
		private delegate void PresentCallbackRegisterDelegate(IntPtr cb);
		private delegate void PresentCallbackUnregisterDelegate(IntPtr cb);
		private delegate void KeyboardHandlerRegisterDelegate(IntPtr handler);
		private delegate void KeyboardHandlerUnregisterDelegate(IntPtr handler);
		private delegate void ScriptWaitDelegate(uint time);
		private delegate void ScriptRegisterDelegate(IntPtr module, IntPtr scriptMain);
		private delegate void ScriptRegisterAdditionalThreadDelegate(IntPtr module, IntPtr threadMain);
		private delegate void ScriptUnregisterByModuleDelegate(IntPtr module);
		private delegate void ScriptUnregisterByMainDelegate(IntPtr scriptMain);
		private delegate void NativeInitDelegate(ulong hash);
		private delegate void NativePush64Delegate(ulong val);
		private delegate IntPtr NativeCallDelegate();
		private delegate IntPtr GetGlobalPtrDelegate(int globalId);
		private delegate int WorldGetAllVehiclesDelegate(IntPtr argPointer, int arrSize);
		private delegate int WorldGetAllPedsDelegate(IntPtr argPointer, int arrSize);
		private delegate int WorldGetAllObjectsDelegate(IntPtr argPointer, int arrSize);
		private delegate int WorldGetAllPickupsDelegate(IntPtr argPointer, int arrSize);
		private delegate IntPtr GetScriptHandleBaseAddressDelegate(uint handle);
		private delegate int GetGameVersionDelegate();

		//Always keeping a reference to all delegates to prevent them from being garbage collected
		private static CreateTextureDelegate createTexture = CreateTexture;
		private static DrawTextureDelegate drawTexture = DrawTexture;
		private static PresentCallbackRegisterDelegate presentCallbackRegister = PresentCallbackRegister;
		private static PresentCallbackUnregisterDelegate presentCallbackUnregister = PresentCallbackUnregister;
		private static KeyboardHandlerRegisterDelegate keyboardHandlerRegister = KeyboardHandlerRegister;
		private static KeyboardHandlerUnregisterDelegate keyboardHandlerUnregister = KeyboardHandlerUnregister;
		private static ScriptWaitDelegate scriptWait = ScriptWait;
		private static ScriptRegisterDelegate scriptRegister = ScriptRegister;
		private static ScriptRegisterAdditionalThreadDelegate scriptRegisterAdditionalThread = ScriptRegisterAdditionalThread;
		private static ScriptUnregisterByModuleDelegate scriptUnregisterByModule = ScriptUnregisterByModule;
		private static ScriptUnregisterByMainDelegate scriptUnregisterByMain = ScriptUnregisterByMain;
		private static NativeInitDelegate nativeInit = NativeInit;
		private static NativePush64Delegate nativePush64 = NativePush64;
		private static NativeCallDelegate nativeCall = NativeCall;
		private static GetGlobalPtrDelegate getGlobalPtr = GetGlobalPtr;
		private static WorldGetAllVehiclesDelegate worldGetAllVehicles = WorldGetAllVehicles;
		private static WorldGetAllPedsDelegate worldGetAllPeds = WorldGetAllPeds;
		private static WorldGetAllObjectsDelegate worldGetAllObjects = WorldGetAllObjects;
		private static WorldGetAllPickupsDelegate worldGetAllPickups = WorldGetAllPickups;
		private static GetScriptHandleBaseAddressDelegate getScriptHandleBaseAddress = GetScriptHandleBaseAddress;
		private static GetGameVersionDelegate getGameVersion = GetGameVersion;

		private static ulong nativeHash;
		private static ulong[] arguments;
		private static int argumentsIndex = 0;
		private static IntPtr returnedValue = IntPtr.Zero;

		public static void Initialize()
		{
			arguments = new ulong[25];
			returnedValue = Marshal.AllocHGlobal(sizeof(NativeRetVal));

			RegisterHandler(Marshal.GetFunctionPointerForDelegate(createTexture),
							Marshal.GetFunctionPointerForDelegate(drawTexture),
							Marshal.GetFunctionPointerForDelegate(presentCallbackRegister),
							Marshal.GetFunctionPointerForDelegate(presentCallbackUnregister),
							Marshal.GetFunctionPointerForDelegate(keyboardHandlerRegister),
							Marshal.GetFunctionPointerForDelegate(keyboardHandlerUnregister),
							Marshal.GetFunctionPointerForDelegate(scriptWait),
							Marshal.GetFunctionPointerForDelegate(scriptRegister),
							Marshal.GetFunctionPointerForDelegate(scriptRegisterAdditionalThread),
							Marshal.GetFunctionPointerForDelegate(scriptUnregisterByModule),
							Marshal.GetFunctionPointerForDelegate(scriptUnregisterByMain),
							Marshal.GetFunctionPointerForDelegate(nativeInit),
							Marshal.GetFunctionPointerForDelegate(nativePush64),
							Marshal.GetFunctionPointerForDelegate(nativeCall),
							Marshal.GetFunctionPointerForDelegate(getGlobalPtr),
							Marshal.GetFunctionPointerForDelegate(worldGetAllVehicles),
							Marshal.GetFunctionPointerForDelegate(worldGetAllPeds),
							Marshal.GetFunctionPointerForDelegate(worldGetAllObjects),
							Marshal.GetFunctionPointerForDelegate(worldGetAllPickups),
							Marshal.GetFunctionPointerForDelegate(getScriptHandleBaseAddress),
							Marshal.GetFunctionPointerForDelegate(getGameVersion));
		}

		public static void Dispose()
		{
			if(returnedValue != IntPtr.Zero)
				Marshal.FreeHGlobal(returnedValue);

			returnedValue = IntPtr.Zero;
		}

		public static void FillCrashReport(StringBuilder report)
		{
			report.Append("Pointer to RetVal: " + returnedValue.ToString("X") + '\n');
			report.Append("Last native called: " + nativeHash.ToString("X") + '\n');
		}

		/* textures */

		// Create texture
		//	texFileName	- texture file name, it's best to specify full texture path and use PNG textures
		//	returns	internal texture id
		//	Texture deletion is performed automatically when game reloads scripts
		//	Can be called only in the same thread as natives

		private static int CreateTexture(string texFileName)
		{
			return Support.Instance.TextureManager.CreateTexture(texFileName);
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

		private static void DrawTexture(int id, int index, int level, int time,
			float sizeX, float sizeY, float centerX, float centerY,
			float posX, float posY, float rotation, float screenHeightScaleFactor,
			float r, float g, float b, float a)
		{
			Support.Instance.TextureManager.DrawTexture(id, time, posX, posY, sizeX, sizeY * screenHeightScaleFactor, rotation, centerX, centerY);
		}

		// Register IDXGISwapChain::Present callback
		// must be called on dll attach
		private static void PresentCallbackRegister(IntPtr cb) //Not implemented
		{
			Log.Warn("Present callbacks are not supported. This call to PresentCallbackRegister will not do anything.");
		}

		// Unregister IDXGISwapChain::Present callback
		// must be called on dll detach
		private static void PresentCallbackUnregister(IntPtr cb) //Not implemented
		{
			Log.Warn("Present callbacks are not supported. This call to PresentCallbackUnregister will not do anything.");
		}

		/* keyboard */

		// Register keyboard handler
		// must be called on dll attach
		private static void KeyboardHandlerRegister(IntPtr handler)
		{
			Support.Instance.KeyboardManager.RegisterHandler(Marshal.GetDelegateForFunctionPointer<KeyboardManager.KeyboardHandler>(handler));
		}

		// Unregister keyboard handler
		// must be called on dll detach
		private static void KeyboardHandlerUnregister(IntPtr handler)
		{
			Support.Instance.KeyboardManager.UnregisterHandler(Marshal.GetDelegateForFunctionPointer<KeyboardManager.KeyboardHandler>(handler));
		}

		/* scripts */

		private static void ScriptWait(uint time)
		{
			GameFiber.Wait((int)time);
		}

		private static void ScriptRegister(IntPtr module, IntPtr scriptMain)
		{
			AsiPlugin plugin = Support.Instance.Loader.GetPlugin(module);

			if(plugin != null)
				plugin.ScriptThreads.Add(new AsiThread(plugin.Name + "-main", Marshal.GetDelegateForFunctionPointer<Action>(scriptMain)));
			else Log.Warn("Unable to register script: unknown plugin");
		}

		private static void ScriptRegisterAdditionalThread(IntPtr module, IntPtr threadMain)
		{
			AsiPlugin plugin = Support.Instance.Loader.GetPlugin(module);

			if(plugin != null)
				plugin.ScriptThreads.Add(new AsiThread(plugin.Name + "-additional" + (plugin.ScriptThreads.Count - 1), Marshal.GetDelegateForFunctionPointer<Action>(threadMain)));
			else Log.Warn("Unable to register additional script thread: unknown plugin");
		}

		private static void ScriptUnregisterByModule(IntPtr module) {} //Nothing is done here, all threads should be aborted at this point

		private static void ScriptUnregisterByMain(IntPtr scriptMain) {} // deprecated

		private static void NativeInit(ulong hash)
		{
			nativeHash = hash;
		}

		private static void NativePush64(ulong val)
		{
			arguments[argumentsIndex] = val;
			argumentsIndex++;
		}

		private static IntPtr NativeCall()
		{
			if(returnedValue == IntPtr.Zero)
				throw new Exception("AsiInterface is not initialized.");

			NativeArgument[] args = new NativeArgument[argumentsIndex];

			for(int i = 0; i < argumentsIndex; i++)
				args[i] = new NativeArgument(arguments[i]);

			argumentsIndex = 0;

			NativeRetVal retVal = (NativeRetVal)NativeFunction.Call(nativeHash, typeof(NativeRetVal), args);
			*(NativeRetVal*)returnedValue = retVal;

			return returnedValue;
		}

		// Returns pointer to global variable
		// make sure that you check game version before accessing globals because
		// ids may differ between patches
		private static IntPtr GetGlobalPtr(int globalId)
		{
			return Game.GetScriptGlobalVariableAddress(globalId);
		}

		/* world */

		// Get entities from internal pools
		// return value represents filled array elements count
		// can be called only in the same thread as natives
		private static int WorldGetAllVehicles(IntPtr argPointer, int arrSize)
		{
			uint* arrPointer = (uint*)argPointer;
			int count = 0;

			foreach(Vehicle veh in World.EnumerateVehicles())
			{
				if(count == arrSize)
					break;

				arrPointer[count] = veh.Handle.Value;
				count++;
			}

			return count;
		}

		private static int WorldGetAllPeds(IntPtr argPointer, int arrSize)
		{
			uint* arrPointer = (uint*)argPointer;
			int count = 0;

			foreach(Ped ped in World.EnumeratePeds())
			{
				if(count == arrSize)
					break;

				arrPointer[count] = ped.Handle.Value;
				count++;
			}

			return count;
		}

		private static int WorldGetAllObjects(IntPtr argPointer, int arrSize)
		{
			uint* arrPointer = (uint*)argPointer;
			int count = 0;

			foreach(Rage.Object obj in World.EnumerateObjects())
			{
				if(count == arrSize)
					break;

				arrPointer[count] = obj.Handle.Value;
				count++;
			}

			return count;
		}

		/*Thanks to alexguirre and SHVDN for this !*/
		private static int WorldGetAllPickups(IntPtr argPointer, int arrSize)
		{
			uint* arrPointer = (uint*)argPointer;
			int count = 0;

			foreach(uint pickup in PickupLister.GetAllPickups())
			{
				if(count == arrSize)
					break;

				arrPointer[count] = pickup;
				count++;
			}

			return count;
		}

		/* misc */

		// Returns base object pointer using it's script handle
		// make sure that you check game version before accessing object fields because
		// offsets may differ between patches
		private static IntPtr GetScriptHandleBaseAddress(uint handle)
		{
			return World.GetEntityByHandle<Entity>(handle).MemoryAddress;
		}

		private static int GetGameVersion()
		{
			return Support.Instance.GameVersion;
		}
	}
}