using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rage;

namespace AsiSupport
{
	/// <summary>
	/// Based on alexguirre's PickupObject class
	/// </summary>
	public static unsafe class PickupLister
	{
		private static List<uint> result;

		public static List<uint> GetAllPickups()
		{
			Game.RawFrameRender += GetAllPickupsOnRawFrameRender;

			while(result == null) // wait for the RawFrameRender to execute
				GameFiber.Yield();

			List<uint> toReturn = result;
			result = null;
			return toReturn;
		}

		/// <summary>
		/// AddEntityToScriptGuidsPool needs to be called from the RawFrameRender,
		/// otherwise it crashes, TLS issues I'm guessing
		/// so that's why we do this nasty workaround...
		/// </summary>
		private static void GetAllPickupsOnRawFrameRender(object sender, GraphicsEventArgs e)
		{
			Pool* pickupPool = PickupsPoolInstance;
			EntitiesScriptGuidsPool* entitiesPool = EntitiesScriptGuidsPoolInstance;

			List<uint> pickups = new List<uint>();

			for(uint i = 0; i < pickupPool->size; i++)
			{
				if(entitiesPool->IsFull())
				{
					break;
				}

				if(pickupPool->IsValid(i))
				{
					IntPtr address = pickupPool->GetAddress(i);

					if(address != IntPtr.Zero)
						pickups.Add(AddEntityToScriptGuidsPool(address));
				}
			}

			result = pickups;
			Game.RawFrameRender -= GetAllPickupsOnRawFrameRender;
		}

		/// <summary>
		/// Based off SHVDN's GenericPool struct: https://github.com/crosire/scripthookvdotnet/blob/dev_v3/source/core/NativeMemory.cpp#L56
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 0x28)]
		private unsafe struct Pool
		{
			[FieldOffset(0x00)]
			public long poolStartAddress;
			[FieldOffset(0x08)]
			public byte* byteArray;
			[FieldOffset(0x10)]
			public uint size;
			[FieldOffset(0x14)]
			public uint itemSize;

			public bool IsValid(uint index)
			{
				return index < size && Mask(index) != 0;
			}

			public IntPtr GetAddress(uint index)
			{
				return new IntPtr(Mask(index) & (poolStartAddress + index * itemSize));
			}

			internal long Mask(uint i)
			{
				long num1 = byteArray[i] & 0x80;
				return ~((num1 | -num1) >> 63);
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private unsafe struct EntitiesScriptGuidsPool
		{
			[FieldOffset(0x00)]
			internal Pool pool;

			public bool IsFull()
			{
				// SHDN checks if at leasts there are 256 free slots
				return pool.size - pool.itemSize <= 256;
			}
		}


		private static Pool* pickupsPool;
		private static Pool* PickupsPoolInstance
		{
			get
			{
				if(pickupsPool == null)
				{
					IntPtr address = Game.FindPattern("8B F0 48 8B 05 ?? ?? ?? ?? F3 0F 59 F6");
					address = address + *(int*)(address + 5) + 9;
					pickupsPool = *(Pool**)address;
				}

				return pickupsPool;
			}
		}

		private static EntitiesScriptGuidsPool* entitiesScriptGuidsPool;
		private static EntitiesScriptGuidsPool* EntitiesScriptGuidsPoolInstance
		{
			get
			{
				if(entitiesScriptGuidsPool == null)
				{
					IntPtr address = Game.FindPattern("4C 8B 0D ?? ?? ?? ?? 44 8B C1 49 8B 41 08");
					address = address + *(int*)(address + 3) + 7;
					entitiesScriptGuidsPool = *(EntitiesScriptGuidsPool**)address;
				}

				return entitiesScriptGuidsPool;
			}
		}

		private delegate uint AddEntityToScriptGuidsPoolDelegate(IntPtr entity);
		private static AddEntityToScriptGuidsPoolDelegate addEntityToScriptGuidsPool;
		private static AddEntityToScriptGuidsPoolDelegate AddEntityToScriptGuidsPool
		{
			get
			{
				if(addEntityToScriptGuidsPool == null)
				{
					IntPtr address = Game.FindPattern("48 F7 F9 49 8B 48 08 48 63 D0 C1 E0 08 0F B6 1C 11 03 D8");
					address = address - 0x68;
					addEntityToScriptGuidsPool = Marshal.GetDelegateForFunctionPointer<AddEntityToScriptGuidsPoolDelegate>(address);
				}

				return addEntityToScriptGuidsPool;
			}
		}
	}
}
