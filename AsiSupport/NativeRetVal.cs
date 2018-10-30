using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AsiSupport
{
	/// <summary>
	/// Generic structure large enough to contain everything returned by native functions
	/// (usually 8 bytes, but sometimes 24 bytes when returning Vectors)
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 24)]
	public struct NativeRetVal {}
}
