using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PursuitLib.RPH;

namespace AsiSupport
{
	public static class EntryPoint
	{
		public static void Main()
		{
			RPHPlugin.Init();
			new Support().Run();
		}

		public static void OnUnload(bool isTerminating)
		{
			Support.Instance?.Unload(!isTerminating);
		}
	}
}
