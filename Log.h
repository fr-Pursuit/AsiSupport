#pragma once

using namespace std;

namespace Log
{
	void Info(string message);

	void Warn(string message);

	void Error(string message);

	void DisplayLine(string message);

	string ToUnmanaged(String^ message);
};
