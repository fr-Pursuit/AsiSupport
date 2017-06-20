#pragma once

namespace AsiSupport
{
	public class KeyboardManager
	{
		public:

		static void Initialize();

		static void RegisterHandler(KeyboardHandler handler);

		static void UnregisterHandler(KeyboardHandler handler);

		static LRESULT APIENTRY WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

		static void Dispose();

		private:
		static WNDPROC oWndProc;
		static list<KeyboardHandler> Handlers;
	};
}