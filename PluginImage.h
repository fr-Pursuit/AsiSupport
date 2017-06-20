#pragma once

typedef const IMAGE_NT_HEADERS64 NT64H;
typedef std::vector<char> bufferVec;

class PluginImage
{
	public:

	PluginImage();
	~PluginImage();

	bool			Load(const std::string & path);

	bool			IsCompatible();

	bool			PatchCompatibility();

	private:

	uint64_t		GetDirectoryAddress(int index);
	uint64_t		RVAToVA(uint32_t rva) const;

	bool			ParseImage();

	private:

	std::string		filePath;
	bufferVec		fileBuffer;
	NT64H *			ntHeader = nullptr;
};