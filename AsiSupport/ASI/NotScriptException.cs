using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsiSupport.ASI
{
	public class NotScriptException : Exception
	{
		public NotScriptException() : base("The specified ASI file is not an ASI script") {}
	}
}
