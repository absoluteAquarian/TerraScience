using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerraScience.Common {
	public static class IdentifierIO {
		public static TagCompound SaveItemID(int itemType) {
			if (itemType <= ItemID.None)
				return null;

			TagCompound tag = new();

			if (itemType < ItemID.Count) {
				tag["mod"] = "Terraria";
				tag["id"] = (short)itemType;
			} else if (ItemLoader.GetItem(itemType) is ModItem modItem) {
				tag["mod"] = modItem.Mod.Name;
				tag["name"] = modItem.Name;
			} else
				return null;

			return tag;
		}

		public static int LoadItemID(TagCompound tag) {
			string mod = tag.GetString("mod");

			if (string.IsNullOrWhiteSpace(mod))
				return -1;

			if (mod == "Terraria")
				return tag.TryGet("id", out short id) ? id : -1;
			
			string name = tag.GetString("name");

			if (string.IsNullOrWhiteSpace(name))
				return -1;

			if (ModLoader.TryGetMod(mod, out Mod source) && source.TryFind(name, out ModItem modItem))
				return modItem.Type;

			return -1;
		}
	}
}
