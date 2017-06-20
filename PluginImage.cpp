#include "stdafx.h"

using namespace std;

PluginImage::PluginImage() {}

PluginImage::~PluginImage() {}

bool PluginImage::Load(const string & path)
{
	filePath = path;

	ifstream inputFile(path, std::ios::binary);

	if(inputFile.fail())
	{
		return false;
	}

	vector<char> bufferTemp((std::istreambuf_iterator<char>(inputFile)), (std::istreambuf_iterator<char>()));
	fileBuffer.swap(bufferTemp);

	bool parseSuccess = ParseImage();
	if(!parseSuccess)
	{
		return false;
	}

	return true;
}

bool PluginImage::ParseImage()
{
	// Get DOS header
	const IMAGE_DOS_HEADER * dosHeader = reinterpret_cast<const IMAGE_DOS_HEADER*>(fileBuffer.data());

	// Not a valid PE
	if(dosHeader->e_magic != IMAGE_DOS_SIGNATURE)
	{
		return false;
	}

	// Get NT header
	ntHeader = reinterpret_cast<const IMAGE_NT_HEADERS64*>(reinterpret_cast<const uint8_t*>(dosHeader) + dosHeader->e_lfanew);

	return true;
}

uint64_t PluginImage::GetDirectoryAddress(int index)
{

	const IMAGE_DATA_DIRECTORY * dataDirectory = ntHeader->OptionalHeader.DataDirectory;

	return RVAToVA(dataDirectory[index].VirtualAddress);
}

uint64_t PluginImage::RVAToVA(uint32_t rva) const
{
	const IMAGE_SECTION_HEADER * sectionHeader = reinterpret_cast<const IMAGE_SECTION_HEADER*>(ntHeader + 1);
	for(int i = 0; i < ntHeader->FileHeader.NumberOfSections; ++i, ++sectionHeader)
	{

		if(rva >= sectionHeader->VirtualAddress && rva <= sectionHeader->VirtualAddress + sectionHeader->Misc.VirtualSize)
		{

			return reinterpret_cast<uint64_t>(fileBuffer.data()) + (rva - sectionHeader->VirtualAddress + sectionHeader->PointerToRawData);
		}
	}

	return 0;
}

bool PluginImage::IsCompatible()
{
	auto * importTable = reinterpret_cast<PIMAGE_IMPORT_DESCRIPTOR>(GetDirectoryAddress(IMAGE_DIRECTORY_ENTRY_IMPORT));
	for(; importTable->Name; ++importTable)
	{
		char * dllName = reinterpret_cast<char*>(RVAToVA(importTable->Name));

		if(strcmp(dllName, "ScriptHookV.dll") == 0)
			return false;
		else if(strcmp(dllName, "AsiSupport.dll") == 0)
			return true;
	}

	return true;
}

bool PluginImage::PatchCompatibility()
{
	Log::Info("Patching compatibility...");
	// Find ScriptHooKV import descriptor
	auto * importTable = reinterpret_cast<PIMAGE_IMPORT_DESCRIPTOR>(GetDirectoryAddress(IMAGE_DIRECTORY_ENTRY_IMPORT));
	for(; importTable->Name; ++importTable)
	{
		char * dllName = reinterpret_cast<char*>(RVAToVA(importTable->Name));

		if(strcmp(dllName, "ScriptHookV.dll") == 0)
		{
			// Found it, patch that shit
			ZeroMemory(dllName, strlen(dllName));
			strcpy(dllName, "AsiSupport.dll");

			// Overwrite original file with changes
			ofstream file(filePath, std::ios::binary | std::ios::out);
			file.write(reinterpret_cast<char*>(fileBuffer.data()), fileBuffer.size());
			file.close();

			return true;
		}
	}

	return false;
}