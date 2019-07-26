using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PursuitLib.IO;
using PursuitLib.IO.Serialization;

namespace AsiSupport.ASI
{
	public class IntegrityMap : XMLFile
	{
		[SaveData]
		private Dictionary<string, Entry> map = new Dictionary<string, Entry>();
		[IgnoreData]
		public bool Dirty { get; private set; } = false;

		public IntegrityMap(string path) : base(path, true) {}

		public bool HasConversionHash(string name)
		{
			return this.map.ContainsKey(name.ToLower());
		}

		public string GetAsiHash(string name)
		{
			string lowerName = name.ToLower();
			return this.HasConversionHash(lowerName) ? this.map[lowerName].AsiHash : null;
		}

		public string GetUnivHash(string name)
		{
			string lowerName = name.ToLower();
			return this.HasConversionHash(lowerName) ? this.map[lowerName].UnivHash : null;
		}

		public void UpdateConversionHash(string name, string asiHash, string univHash)
		{
			string lowerName = name.ToLower();
			Entry entry = new Entry(asiHash, univHash);
			if(this.HasConversionHash(lowerName))
				this.map[lowerName] = entry;
			else this.map.Add(lowerName, entry);

			this.Dirty = true;
		}

		public void RemoveConversionHash(string name)
		{
			string lowerName = name.ToLower();

			if(this.HasConversionHash(lowerName))
				this.map.Remove(lowerName);

			this.Dirty = true;
		}

		public void SaveMap()
		{
			this.Save();
			this.Dirty = false;
		}

		public void Cleanup(string workingDir)
		{
			List<string> toRemove = new List<string>();

			foreach(string key in this.map.Keys)
			{
				if(!File.Exists(Path.Combine(workingDir, key + ".uasi")))
					toRemove.Add(key);
			}

			foreach(string key in toRemove)
				this.map.Remove(key);

			if(toRemove.Count > 0)
				this.Save();
		}

		private class Entry
		{
			public string AsiHash { get; private set; }
			public string UnivHash { get; private set; }

			public Entry(string asiHash, string univHash)
			{
				this.AsiHash = asiHash;
				this.UnivHash = univHash;
			}
		}
	}
}
