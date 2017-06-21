#pragma once

namespace Util
{
	string ToUnmanaged(String^ message);

	String^ GetFileChecksum(String^ path);
}