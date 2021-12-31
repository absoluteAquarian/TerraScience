using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Systems{
	public class TesseractNetwork : ModWorld{
		public class Entry{
			public string name;
			public string password;

			//Using the network requires a password to be entered if it's private
			public bool entryIsPrivate;

			internal Item[] items = new Item[5].Populate(() => new Item());
			internal FluidEntry fluid = new FluidEntry(max: 500f, false, null);

			internal TerraFlux flux;

			private Entry(){ }

			public Entry(string name, bool entryIsPrivate, string password = null){
				this.name = name;
				this.entryIsPrivate = entryIsPrivate;
				this.password = entryIsPrivate ? password : null;
			}

			public static Entry Load(TagCompound tag){
				Entry entry = new Entry();

				if(tag.GetString("name") is string name)
					entry.name = name;
				else
					throw new ArgumentException("Tag entry \"name\" was invalid");

				entry.entryIsPrivate = tag.GetBool("private");

				bool release = TechMod.Release;
				if(tag.GetByteArray("password") is byte[] passwordBytes && passwordBytes.Length > 1){
					string scrambled = FromBytes(passwordBytes);
					entry.password = Unscramble(scrambled);

					if(!release){
						var encoding = Encoding.UTF8;
						//Need to do GetString(GetBytes(str)) to filter out bad Unicode chars
						TechMod.Instance.Logger.Debug($"Loading Tesseract Network \"{name}\".  Password? YES.  Scrambled: \"{encoding.GetString(encoding.GetBytes(scrambled))}\", Unscrambled: \"{entry.password}\"");
					}
				}else if(!release)
					TechMod.Instance.Logger.Debug($"Loading Tesseract Network \"{name}\".  Password? NO.");

				if(tag.GetList<TagCompound>("items") is List<TagCompound> tags && tags.Count == entry.items.Length){
					for(int i = 0; i < entry.items.Length; i++)
						entry.items[i] = ItemIO.Load(tags[i]);
				}

				entry.flux = new TerraFlux(tag.GetFloat("flux"));

				if(tag.GetCompound("fluid") is var fluid)
					entry.fluid.Load(fluid);

				return entry;
			}

			public override bool Equals(object obj)
				=> obj is Entry entry && name == entry.name && password == entry.password && entryIsPrivate == entry.entryIsPrivate;

			public override int GetHashCode() => name.GetHashCode();

			public TagCompound Save()
				=> new TagCompound(){
					["name"] = name,
					["private"] = entryIsPrivate,
					["password"] = password is null ? null : ToBytes(Scramble(password)),
					["items"] = items.Select(ItemIO.Save).ToList(),
					["flux"] = (float)flux,
					["fluid"] = fluid.Save()
				};

			public static bool operator ==(Entry left, Entry right)
				=> left.name == right.name && left.password == right.password && left.entryIsPrivate == right.entryIsPrivate;

			public static bool operator !=(Entry left, Entry right)
				=> left.name != right.name || left.password != right.password || left.entryIsPrivate != right.entryIsPrivate;

			private static string Scramble(string orig){
				ushort[] c=orig.Select(a=>(ushort)a).ToArray();int l=c.Length,i=0;ushort d=(ushort)((c[l-1]&3)<<14),s;
				for(;i<l;i++){s=d;d=(ushort)((c[i]&3)<<14);c[i]=(ushort)(((c[i]>>2)|s)^40659);}
				return new string(c.Select(a=>(char)a).ToArray());
			}

			private static string Unscramble(string orig){
				ushort[] c=orig.Select(a=>(ushort)a).ToArray();int l=c.Length;ushort d=(ushort)(((c[0]^40659)&49152)>>14),s;
				while(l-->0){s=d;d=(ushort)(((c[l]^40659)&49152)>>14);c[l]=(ushort)(((c[l]^40659)<<2)|s);}
				return new string(c.Select(a=>(char)a).ToArray());
			}

			private static byte[] ToBytes(string str){
				//Can't rely on Encoding.UTF8.GetBytes() here
				byte[] buffer = new byte[str.Length * 2];
				char[] letters = str.ToCharArray();

				for(int i = 0; i < letters.Length; i++){
					ushort u = (ushort)letters[i];
					buffer[i * 2] = (byte)((u & 0xff00) >> 8);
					buffer[i * 2 + 1] = (byte)(u & 0x00ff);
				}

				return buffer;
			}

			private static string FromBytes(byte[] bytes){
				char[] letters = new char[bytes.Length / 2];

				for(int i = 0; i < letters.Length; i++)
					letters[i] = (char)(((bytes[i * 2]) << 8) | bytes[i * 2 + 1]);

				return new string(letters);
			}
		}

		private static List<Entry> networks;

		public int NetworkCount => networks.Count;

		public override void Initialize(){
			networks = new List<Entry>();
		}

		internal static void Unload(){
			networks = null;
		}

		public override void Load(TagCompound tag){
			if(tag.GetList<TagCompound>("networks") is var nets)
				networks = nets.Select(Entry.Load).ToList();
		}

		public override TagCompound Save()
			=> new TagCompound(){
				["networks"] = networks.Select(e => e.Save()).ToList()
			};

		public static bool RegisterEntry(Entry entry){
			if(TryGetEntryIndex(entry.name, out _))
				return false;

			networks.Add(entry);
			return true;
		}

		public static bool RemoveEntry(Entry entry){
			if(!TryGetEntryIndex(entry.name, out int index))
				return false;

			networks.RemoveAt(index);
			entry.name = null;
			entry.password = null;
			entry.entryIsPrivate = true;
			return true;
		}

		public static bool TryGetEntry(string name, out Entry entry){
			for(int i = 0; i < networks.Count; i++){
				if(networks[i].name == name){
					entry = networks[i];
					return true;
				}
			}

			entry = default;
			return false;
		}

		private static bool TryGetEntryIndex(string name, out int index){
			for(int i = 0; i < networks.Count; i++){
				if(networks[i].name == name){
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static void UpdatePassword(string entryName, string newPassword){
			if(!TryGetEntryIndex(entryName, out int index))
				return;

			networks[index].password = newPassword;
		}

		public static void UpdateIsPrivate(string entryName, bool newIsPrivate){
			if(!TryGetEntryIndex(entryName, out int index))
				return;

			networks[index].entryIsPrivate = newIsPrivate;
		}

		internal static void UpdateNetworkUIEntries(UIText[] text, ref int page, out int pageTotal, in string usedNetwork){
			if(networks.Count == 0){
				page = 1;
				pageTotal = 1;

				for(int i = 0; i < text.Length; i++)
					text[i].SetText("");

				return;
			}

			while(page * text.Length >= networks.Count + 1)
				page--;

			if(page < 1)
				page = 1;

			pageTotal = networks.Count / text.Length + 1;

			int start = (page - 1) * text.Length;
			int count = Math.Min(networks.Count - start, text.Length);

			for(int i = 0; i < count; i++){
				var name = networks[start + i].name;
				TryGetEntry(name, out var entry);
				text[i].SetText(name + "\nPrivate: " + (entry.entryIsPrivate ? "yes" : "no"));

				text[i].TextColor = name == usedNetwork ? Color.Yellow : Color.White;
			}

			for(int i = count; i < text.Length; i++)
				text[i].SetText("");
		}
	}
}
