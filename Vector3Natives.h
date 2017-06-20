#pragma once

namespace AsiSupport
{
	#pragma pack(push, 1)
	typedef struct
	{
		float x;
		DWORD _paddingx;
		float y;
		DWORD _paddingy;
		float z;
		DWORD _paddingz;
	} Vector3;
	#pragma pack(pop)

	public ref class Vector3Natives
	{
		public:
		static property List<System::UInt64>^ Vector3NativesList;

		static void Initialize()
		{
			Vector3NativesList = gcnew List<System::UInt64>();
			Vector3NativesList->Add(0x0C92BA89F1AF26F8);
			Vector3NativesList->Add(0xE465D4AB7CA6AE72);
			Vector3NativesList->Add(0x3FEF770D40960D5A);
			Vector3NativesList->Add(0x0A794A5A57F8DF91);
			Vector3NativesList->Add(0x2274BC1C4885E333);
			Vector3NativesList->Add(0x1899F328B0E12848);
			Vector3NativesList->Add(0xAFBD61CC738D9EB9);
			Vector3NativesList->Add(0x213B91045D09B983);
			Vector3NativesList->Add(0x9A8D700A51CB7B0D);
			Vector3NativesList->Add(0x4805D2B1D8CF94A9);
			Vector3NativesList->Add(0x44A8FCB8ED227738);
			Vector3NativesList->Add(0xCD5003B097200F36);
			Vector3NativesList->Add(0xBE22B26DD764C040);
			Vector3NativesList->Add(0x4B805E6046EE9E47);
			Vector3NativesList->Add(0x3C06B8786DD94CD1);
			Vector3NativesList->Add(0xE0AF41401ADF87E3);
			Vector3NativesList->Add(0x17C07FC640E86B4E);
			Vector3NativesList->Add(0x92523B76657A517D);
			Vector3NativesList->Add(0xD242728AA6F0FBA2);
			Vector3NativesList->Add(0xF0F2103EFAF8CBA7);
			Vector3NativesList->Add(0x2058206FBE79A8AD);
			Vector3NativesList->Add(0x4EC6CFBC7B2E9536);
			Vector3NativesList->Add(0xCBDB9B923CACC92D);
			Vector3NativesList->Add(0x163E252DE035A133);
			Vector3NativesList->Add(0x6E16BC2503FF1FF0);
			Vector3NativesList->Add(0x225B8B35C88029B3);
			Vector3NativesList->Add(0x594A1028FC2A3E85);
			Vector3NativesList->Add(0x1F400FEF721170DA);
			Vector3NativesList->Add(0x21C235BC64831E5A);
			Vector3NativesList->Add(0x9E3B3E6D66F6E22F);
			Vector3NativesList->Add(0xBAC038F7459AE5AE);
			Vector3NativesList->Add(0x7D304C1C955E3E12);
			Vector3NativesList->Add(0x14D6F5678D8F1B37);
			Vector3NativesList->Add(0x837765A25378F0BB);
			Vector3NativesList->Add(0xA200EB1EE790F448);
			Vector3NativesList->Add(0x5B4E4C817FCC2DFB);
			Vector3NativesList->Add(0x26903D9CD1175F2C);
			Vector3NativesList->Add(0xFA7C7F0AADF25D09);
			Vector3NativesList->Add(0x586AFE3FF72D996E);
			Vector3NativesList->Add(0x223CA69A8C4417FD);
			Vector3NativesList->Add(0x5BFF36D6ED83E0AE);
			Vector3NativesList->Add(0x35736EE65BD00C11);
			Vector3NativesList->Add(0xA4664972A9B8F8BA);
			Vector3NativesList->Add(0x46CD3CB66E0825CC);
			Vector3NativesList->Add(0x8D2064E5B64A628A);
			Vector3NativesList->Add(0x21BB0FBD3E217C2D);
			Vector3NativesList->Add(0xEA61CA8E80F09E4D);
			Vector3NativesList->Add(0x8214A4B5A7A33612);
		}
	};
}