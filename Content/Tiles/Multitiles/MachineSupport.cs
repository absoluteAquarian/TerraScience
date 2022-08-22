using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Tiles.Multitiles{
	public class MachineSupport : ModTile{
		public override void SetStaticDefaults(){
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			AddMapEntry(new Color(30, 30, 30));

			DustType = 82;

			ItemDrop = ModContent.ItemType<Items.Placeable.MachineSupportItem>();
			HitSound = SoundID.Tink;

			MineResist = 2;
		}
	}
}
