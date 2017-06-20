#include "Stdafx.h"

WNDPROC AsiSupport::KeyboardManager::oWndProc;
list<KeyboardHandler> AsiSupport::KeyboardManager::Handlers;

void AsiSupport::KeyboardManager::Initialize()
{
	HWND windowHandle = NULL;
	while(windowHandle == NULL)
	{

		windowHandle = FindWindow(TEXT("grcWindow"), NULL);
		Sleep(100);
	}

	oWndProc = (WNDPROC)SetWindowLongPtr(windowHandle, GWLP_WNDPROC, (LONG_PTR)KeyboardManager::WndProc);
	if(oWndProc == NULL)
		Log::Error("Failed to attach input hook");
	else
		Log::Info("Input hook attached");
}

void AsiSupport::KeyboardManager::RegisterHandler(KeyboardHandler handler)
{
	size_t size = Handlers.size();
	auto it = Handlers.begin();

	for(int i = 0; i < size; i++)
		it++;

	Handlers.insert(it, handler);
}

void AsiSupport::KeyboardManager::UnregisterHandler(KeyboardHandler handler)
{
	Handlers.remove(handler);
}

LRESULT APIENTRY AsiSupport::KeyboardManager::WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	if(uMsg == WM_KEYDOWN || uMsg == WM_KEYUP || uMsg == WM_SYSKEYDOWN || uMsg == WM_SYSKEYUP)
	{
		for each(KeyboardHandler handler in Handlers)
			handler((DWORD)wParam, lParam & 0xFFFF, (lParam >> 16) & 0xFF, (lParam >> 24) & 1, (uMsg == WM_SYSKEYDOWN || uMsg == WM_SYSKEYUP), (lParam >> 30) & 1, (uMsg == WM_SYSKEYUP || uMsg == WM_KEYUP));
	}

	return CallWindowProc(oWndProc, hwnd, uMsg, wParam, lParam);
	return NULL;
}

void AsiSupport::KeyboardManager::Dispose()
{
	auto a = FindWindow(nullptr, nullptr);
	HWND windowHandle = FindWindow(TEXT("grcWindow"), NULL);
	SetWindowLongPtr(windowHandle, GWLP_WNDPROC, (LONG_PTR)oWndProc);
	Log::Info("Removed input hook");
}

