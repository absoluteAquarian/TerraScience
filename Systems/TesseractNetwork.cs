using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraScience.Systems{
	public class TesseractNetwork : ModWorld{
		public class Entry{
			public string name;
			public string password;

			//Using the network requires a password to be entered if it's private
			public bool entryIsPrivate;

			private Entry(){ }

			public Entry(string name, bool entryIsPrivate, string password = null){
				this.name = name;
				this.entryIsPrivate = entryIsPrivate;
				this.password = password;
			}

			public static Entry Load(TagCompound tag){
				Entry entry = new Entry();

				if(tag.GetString("name") is string name)
					entry.name = name;
				else
					throw new ArgumentException("Tag entry \"name\" was invalid");

				entry.entryIsPrivate = tag.GetBool("private");
				entry.password = Unscramble(tag.GetString("password"));

				return entry;
			}

			public override bool Equals(object obj)
				=> obj is Entry entry && name == entry.name && password == entry.password && entryIsPrivate == entry.entryIsPrivate;

			public override int GetHashCode() => name.GetHashCode();

			public TagCompound Save()
				=> new TagCompound(){
					["name"] = name,
					["private"] = entryIsPrivate,
					["password"] = Scramble(password)
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
		}

		private static List<Entry> networks;

		public int NetworkCount => networks.Count;

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

		internal static void UpdateNetworkUIEntries(UIText[] text, ref int page, in string usedNetwork){
			if(page < 0)
				page = 0;
			while(page * text.Length >= networks.Count)
				page--;

			int start = page * text.Length;
			int count = Math.Min(networks.Count - start, text.Length);

			for(int i = 0; i < count; i++){
				var name = networks[start + i].name;
				text[i].SetText(name);

				text[i].TextColor = name == usedNetwork ? Color.Yellow : Color.White;
			}

			for(int i = count; i < text.Length; i++)
				text[i].SetText("");
		}
	}
}
