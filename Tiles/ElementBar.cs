using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Items.Elements;

namespace TerraScience.Tiles{
	public class ElementBar : ModTile{
		//We don't want this item to be autoloaded, since it's just a template for the other Element bar tiles
		public override bool Autoload(ref string name, ref string texture) => false;

		public ElementBar(){ }

		public override void SetDefaults(){
			//Copied from ExampleBar in ExampleMod
			Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.MetalBar")); // localized text for "Metal Bar"
		}

		public override bool Drop(int i, int j){
			TerraScience.SpawnElementItem(i * 16, j * 16, 16, 16, Name);
			return true;
		}
	}
}
