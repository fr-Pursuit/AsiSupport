#pragma once

public ref class PickupLister
{
	public:
	static List<Rage::PoolHandle>^ GetAllPickupHandles();

	private:
	static IntPtr PickupsPoolPointer = IntPtr::Zero;
	static IntPtr EntitiesScriptGuidsPoolPointer = IntPtr::Zero;
	static IntPtr AddEntityToScriptGuidsPool = IntPtr::Zero;

	static List<Rage::PoolHandle>^ getAllPickupsResult;

	static void GetAllPickupsRawFrameRenderWrapper(System::Object^ sender, Rage::GraphicsEventArgs^ e);

	static List<Rage::PoolHandle>^ GetAllPickupsInternal();
};

struct Pool
{
	public:
	long poolStartAddress;
	byte* byteArray;
	unsigned int size;
	unsigned int itemSize;

	bool IsValid(unsigned int index);

	long long GetAddress(unsigned int index);

	long Mask(unsigned int i);
};

struct EntitiesScriptGuidsPool
{
	public:
	Pool pool;

	bool IsFull();
};