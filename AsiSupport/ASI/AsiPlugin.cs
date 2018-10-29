using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace AsiSupport.ASI
{
	public class AsiPlugin
	{
		private const ushort ImageDosSignature = 0x5A4D;
		private const ushort ImagePeSignature = 0x4550;
		private const ushort OptionalHeaderSignature = 0x020b;
		private static readonly char[] UniversalDll = "UnvAsiIntrf.dll".ToCharArray();

		[DllImport("Kernel32.dll")]
		private static extern IntPtr GetModuleHandleA(string lpModuleName);

		public static bool Exists(string name)
		{
			return File.Exists(Path.Combine(Support.Instance.Loader.WorkingDir, name + ".uasi")) || File.Exists(name + ".asi");
		}

		public string Name { get; private set; }
		public AsiType Type { get; private set; }
		public string UASIPath => Path.Combine(Support.Instance.Loader.WorkingDir, this.Name + ".uasi");
		public List<AsiThread> ScriptThreads { get; private set; }
		public IntPtr Module => GetModuleHandleA(this.UASIPath);

		public AsiPlugin(string name)
		{
			this.Name = name;
			this.UpdateType();
			this.ScriptThreads = new List<AsiThread>();
		}

		public void UpdateType()
		{
			if(File.Exists(this.UASIPath))
			{
				if(File.Exists(this.Name + ".asi"))
					this.Type = AsiType.UniversalConverted;
				else this.Type = AsiType.Universal;
			}
			else if(File.Exists(this.Name + ".asi"))
				this.Type = AsiType.NonUniversal;
		}

		public void ConvertAsi()
		{
			using(FileStream fStream = File.OpenRead(this.Name + ".asi"))
			using(BinaryReader reader = new BinaryReader(fStream))
			{
				/*Finding the reference to ScriptHookV*/
				uint shvPos;

				if(reader.ReadUInt16() == ImageDosSignature)
				{
					fStream.Position = 60; //Jump to PE Header address
					fStream.Position = reader.ReadUInt32(); //Jump to PE Header

					if(reader.ReadUInt16() == ImagePeSignature)
					{
						fStream.Position += 22; //Jump to Optional x64 Header
						uint relocationPos = (uint)fStream.Position + 240;

						if(reader.ReadUInt16() == OptionalHeaderSignature)
						{
							fStream.Position += 118; //Jump to Import Table's virtual address
							this.JumpToRVA(fStream, reader, relocationPos, reader.ReadUInt32()); //Jump to Import Table

							uint pos = (uint)fStream.Position + 12;
							char[] dllName = new char[15];

							while(true)
							{
								fStream.Position = pos;
								this.JumpToRVA(fStream, reader, relocationPos, reader.ReadUInt32()); //Jump to the dll's Name

								for(int i = 0; i < dllName.Length; i++)
									dllName[i] = (char)reader.ReadByte();

								if(new string(dllName) == "ScriptHookV.dll")
								{
									shvPos = (uint)(fStream.Position - dllName.Length);
									break;
								}
								else pos += 20;
							}
						}
						else throw new InvalidOperationException("ASI file not valid: invalid optional header signature");
					}
					else throw new InvalidOperationException("ASI file not valid: invalid PE signature");
				}
				else throw new InvalidOperationException("ASI file not valid: invalid DOS signature");

				/*Copying the file while replacing the reference*/
				fStream.Position = 0;

				if(File.Exists(this.UASIPath))
					File.Delete(this.UASIPath);

				using(FileStream univ = File.Create(this.UASIPath))
				{
					byte[] buffer = new byte[4096];
					int read = 0;

					while((read = fStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if(shvPos >= univ.Position && shvPos < univ.Position + read)
						{
							for(int i = 0; i < UniversalDll.Length; i++)
								buffer[shvPos - univ.Position + i] = (byte)UniversalDll[i];
						}

						univ.Write(buffer, 0, read);
					}
				}
			}
		}

		private void JumpToRVA(FileStream fStream, BinaryReader reader, uint relocationPos, uint targetVA)
		{
			fStream.Position = relocationPos; //Jump to section header

			while(true)
			{
				fStream.Position += 8; //Jump to section's virtual size / virtual address
				uint size = reader.ReadUInt32();
				uint va = reader.ReadUInt32();

				if(targetVA >= va && targetVA < va + size)
				{
					fStream.Position += 4; //Jump to raw data address
					fStream.Position = reader.ReadUInt32() + (targetVA - va); //Jump to target
					return;
				}
				else fStream.Position += 24; //Jump to next section
			}
		}
	}
}
