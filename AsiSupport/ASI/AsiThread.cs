using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PursuitLib;
using Rage;

namespace AsiSupport.ASI
{
	public class AsiThread
	{
		public string Name { get; private set; }
		public GameFiber Fiber { get; private set; }
		private Action start;

		public AsiThread(string name, Action start)
		{
			this.Name = name;
			this.start = start;

			if(Support.Instance.Loader.Loading) //Don't start fibers during loading, they will be started after
				this.Fiber = null;
			else this.Fiber = GameFiber.StartNew(RunFiber, this.Name);
		}

		public void Start()
		{
			if(this.start != null)
			{
				if(this.Fiber == null)
					this.Fiber = GameFiber.StartNew(RunFiber, this.Name);
				else Log.Warn("AsiThread \"" + this.Name + "\" already started.");
			}
			else Log.Error("Unable to start AsiThread \"" + this.Name + "\": no function specified.");
		}

		private void RunFiber()
		{
			Log.Info("Starting new GameFiber: \"" + this.Name + '"');

			try
			{
				this.start.Invoke();
			}
			catch(Exception e)
			{
				if(!(e is ThreadAbortException)) //Thrown when unloading the plugin manually
					Log.Error("GameFiber \"" + this.Name + "\" has crashed: " + e);
			}
		}
	}
}
