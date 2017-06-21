#include "Stdafx.h"

string Util::ToUnmanaged(String^ message)
{
	return msclr::interop::marshal_as<std::string>(message);
}

String^ Util::GetFileChecksum(String^ path)
{
	if(File::Exists(path))
	{
		System::Security::Cryptography::SHA1^ sha1 = System::Security::Cryptography::SHA1::Create();
		Stream^ stream = File::OpenRead(path);

		String^ hash = BitConverter::ToString(sha1->ComputeHash(stream))->Replace("-", "")->ToLower();

		stream->Close();

		return hash;
	}
	else
		throw gcnew FileNotFoundException(path);
}