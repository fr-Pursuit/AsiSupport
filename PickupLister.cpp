#include "Stdafx.h"


List<Rage::PoolHandle>^ PickupLister::GetAllPickupHandles()
{
	Rage::Game::RawFrameRender += gcnew System::EventHandler<Rage::GraphicsEventArgs ^>(&PickupLister::GetAllPickupsRawFrameRenderWrapper);
	
	while(getAllPickupsResult == nullptr) // wait for the RawFrameRender to execute
		Rage::GameFiber::Sleep(0);

	List<Rage::PoolHandle>^ result = getAllPickupsResult;
	getAllPickupsResult = nullptr;
	return result;
}

void PickupLister::GetAllPickupsRawFrameRenderWrapper(System::Object^ sender, Rage::GraphicsEventArgs^ e)
{
	getAllPickupsResult = GetAllPickupsInternal(); // AddEntityToScriptGuidsPool needs to be called from the RawFrameRender,
												   // otherwise it crashes, TLS issues I'm guessing
												   // so that's why we do this nasty workaround...

	Rage::Game::RawFrameRender -= gcnew System::EventHandler<Rage::GraphicsEventArgs ^>(&GetAllPickupsRawFrameRenderWrapper);
}

List<Rage::PoolHandle>^ PickupLister::GetAllPickupsInternal()
{
	if(PickupsPoolPointer == IntPtr::Zero)
	{
		int* address = (int*)Rage::Game::FindPattern("8B F0 48 8B 05 ?? ?? ?? ?? F3 0F 59 F6").ToPointer();
		address = address + *(int*)(address + 5) + 9;
		PickupsPoolPointer = IntPtr(*(long long*)address);
	}

	if(EntitiesScriptGuidsPoolPointer == IntPtr::Zero)
	{
		int* address = (int*)Rage::Game::FindPattern("4C 8B 0D ?? ?? ?? ?? 44 8B C1 49 8B 41 08").ToPointer();
		address = address + *(int*)(address + 3) + 7;
		EntitiesScriptGuidsPoolPointer = IntPtr(*(long long*)address);
	}

	Pool* pickupPool = (Pool*)PickupsPoolPointer.ToPointer();
	EntitiesScriptGuidsPool* entitiesPool = (EntitiesScriptGuidsPool*)EntitiesScriptGuidsPoolPointer.ToPointer();

	List<Rage::PoolHandle>^ pickups = gcnew List<Rage::PoolHandle>();

	for(unsigned int i = 0; i < pickupPool->size; i++)
	{
		if(entitiesPool->IsFull())
		{
			break;
		}

		if(pickupPool->IsValid(i))
		{
			long long address = pickupPool->GetAddress(i);

			if(address != 0)
			{
				if(AddEntityToScriptGuidsPool == IntPtr::Zero)
				{
					IntPtr functionAddress = Rage::Game::FindPattern("48 F7 F9 49 8B 48 08 48 63 D0 C1 E0 08 0F B6 1C 11 03 D8");
					AddEntityToScriptGuidsPool = functionAddress - 0x68;
				}

				Rage::PoolHandle(*function) (long long) = (Rage::PoolHandle(*)(long long))AddEntityToScriptGuidsPool.ToPointer();

				Rage::PoolHandle handle = function(address);

				pickups->Add(handle);
			}
		}
	}

	return pickups;
}

bool Pool::IsValid(unsigned int index)
{
	return index < size && Mask(index) != 0;
}

long long Pool::GetAddress(unsigned int index)
{
	return Mask(index) & (poolStartAddress + index * itemSize);
}

long Pool::Mask(unsigned int i)
{
	long num1 = byteArray[i] & 0x80;
	return ~((num1 | -num1) >> 63);
}

bool EntitiesScriptGuidsPool::IsFull()
{
	// SHDN checks if at leasts there are 256 free slots
	return pool.size - pool.itemSize <= 256;
}
