#include "Stdafx.h"

AsiSupport::IntegrityMap::IntegrityMap()
{
	this->integrityMap = gcnew Dictionary<String^, String^>();

	String^ metaPath = Path::Combine(AsiLoader::instance->WorkingDir, "rasi.meta");
	
	if(File::Exists(metaPath))
	{
		StreamReader^ reader = gcnew StreamReader(metaPath);

		int lineNumber = 0;
		String^ line = nullptr;

		while(!reader->EndOfStream)
		{
			try
			{
				line = reader->ReadLine();

				if(line->StartsWith("#") || String::IsNullOrWhiteSpace(line) || !line->Contains("="))
					continue;
				else
				{
					String^ name = line->Substring(0, line->IndexOf('='));
					String^ value = line->Substring(name->Length + 1);
					this->integrityMap->Add(name, value);
				}
			}
			catch(exception)
			{
				Log::Warn("Line " + to_string(lineNumber) + " of rasi.meta is invalid");
			}

			lineNumber++;
		}

		reader->Close();
	}
}

void AsiSupport::IntegrityMap::SetEntry(String^ name, String^ hash)
{
	if(this->integrityMap->ContainsKey(name))
		this->integrityMap[name] = hash;
	else this->integrityMap->Add(name, hash);
}

String^ AsiSupport::IntegrityMap::GetEntry(String^ name)
{
	if(this->integrityMap->ContainsKey(name))
		return this->integrityMap[name];
	else return nullptr;
}

void AsiSupport::IntegrityMap::Save()
{
	String^ metaPath = Path::Combine(AsiLoader::instance->WorkingDir, "rasi.meta");

	if(File::Exists(metaPath))
		File::Delete(metaPath);

	StreamWriter^ writer = gcnew StreamWriter(metaPath);

	for each(KeyValuePair<String^, String^>^ pair in this->integrityMap)
		writer->WriteLine(pair->Key + "=" + pair->Value);

	writer->Close();
}
