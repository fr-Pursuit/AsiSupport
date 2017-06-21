#pragma once

namespace AsiSupport
{
	public ref class IntegrityMap
	{
		public:
		IntegrityMap();

		void SetEntry(String^ name, String^ hash);

		String^ GetEntry(String^ name);

		void Save();

		private:
		Dictionary<String^, String^>^ integrityMap;
	};
}