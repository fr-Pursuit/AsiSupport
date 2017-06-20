#pragma once

namespace AsiSupport
{
	public ref class PluginThread
	{
		public:
		property String^ Name;
		property Rage::GameFiber^ Fiber;

		PluginThread(String^ name, void(*entryPoint)());

		void Start();

		void RunFiber();

		private:
		Action^ start;
	};
}
