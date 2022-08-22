using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;
using TerraScience.World;

namespace TerraScience.Content.Tiles{
	public class ItemTransportTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Items;

		public virtual float TransferProgressPerTick => 2 / 60f;  //Moves through 2 tiles per second

		public override void SafeSetDefaults(){
			AddMapEntry(Color.Gray);
			ItemDrop = ModContent.ItemType<ItemTransport>();

			Main.tileMerge[TileID.Containers][Type] = true;
			Main.tileMerge[TileID.Containers2][Type] = true;
			Main.tileMerge[Type][TileID.Containers] = true;
			Main.tileMerge[Type][TileID.Containers2] = true;
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			NetworkCollection.OnItemPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnItemPipeKill(new Point16(i, j));
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			if(NetworkCollection.HasItemPipeAt(new Point16(i, j), out ItemNetwork net)){
				Rectangle rect = new Rectangle(i * 16, j * 16, 16, 16);

				//Draw all item path items that are in this tile
				foreach(var path in net.paths){
					Vector2 topLeft = path.worldCenter - new Vector2(3.85f, 3.85f);
					if(rect.X <= topLeft.X && rect.X + rect.Width >= topLeft.X && rect.Y <= topLeft.Y && rect.Y + rect.Height >= topLeft.Y){
						//Item should be drawn behind this pipe.  Draw it
						TerraScienceWorld.DrawItemInPipe(path, spriteBatch);
					}
				}
			}

			return true;
		}
	}
}
