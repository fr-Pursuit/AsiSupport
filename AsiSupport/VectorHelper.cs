using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PursuitLib;
using PursuitLib.IO;

namespace AsiSupport
{
	public static class VectorHelper
	{
		public static List<ulong> NativesList { get; private set; }

		public static void Init()
		{
			NativesList = new List<ulong>();

			ManifestFile manifest = new ManifestFile(Path.Combine(Support.Instance.DataDirectory, "Vector3Natives"));
			
			foreach(string entry in manifest.Entries)
			{
				try
				{
					NativesList.Add(Convert.ToUInt64(entry, 16));
				}
				catch(Exception)
				{
					Log.Warn("Invalid entry in Vector3Natives.manifest: " + entry);
				}
			}
		}

		[StructLayout(LayoutKind.Explicit, Size = 24)]
		public struct NativeVector3
		{
			[FieldOffset(0)]
			public float x;
			[FieldOffset(8)]
			public float y;
			[FieldOffset(16)]
			public float z;
		}
	}
}
