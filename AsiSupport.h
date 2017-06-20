#pragma once

namespace AsiSupport
{
	using namespace Rage::Attributes;

	public ref class EntryPoint
	{
		public:
		static void Main();

		[ConsoleCommand]
		static void ListAsiPlugins();

		[ConsoleCommand]
		static void LoadAsiPlugin(String^ name);

		[ConsoleCommand]
		static void UnloadAsiPlugin(String^ name);

		[ConsoleCommand]
		static void AsiSupportVersion();

		static void OnUnload(bool isTerminating);

		private:
		static AsiLoader^ loader;
	};
}
