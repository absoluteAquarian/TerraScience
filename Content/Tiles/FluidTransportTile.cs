using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.Items.Tools;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles{
	public class FluidTransportTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Fluids;

		public virtual float Capacity => 0.25f;  //0.25L

		//Export rate/s needs to be greater than pump rate/s in order to have a net gain in storage containers
		public virtual float ExportRate => 0.5f / 60f;  //0.5L/s -> 0.008333L/tick

		public override void SafeSetDefaults(){
			AddMapEntry(Color.DarkBlue);
			drop = ModContent.ItemType<FluidTransport>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			NetworkCollection.OnFluidPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnFluidPipeKill(new Point16(i, j));
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			DrawFluid(new Point16(i, j), ModContent.GetTexture("TerraScience/Content/Tiles/Effect_FluidTransportTile_fluid"), spriteBatch);

			return true;
		}

		internal static void DrawFluid(Point16 tilePos, Texture2D texture, SpriteBatch spriteBatch){
			Tile tile = Framing.GetTileSafely(tilePos);
			if(NetworkCollection.HasFluidPipeAt(tilePos, out FluidNetwork net)){
				float factor = net.StoredFluid / net.Capacity;

				float alpha = net.liquidType != MachineLiquidID.None
					? 1f
					: (net.gasType != MachineGasID.None
						? 0.65f * factor
						: 0);

				if(alpha == 0)
					return;

				Color color = net.gasType != MachineGasID.None
					? Capsule.GetBackColor(net.gasType)
					: net.liquidType != MachineLiquidID.None
						? Capsule.GetBackColor(net.liquidType)
						: throw new Exception();

				color = MiscUtils.MixLightColors(Lighting.GetColor(tilePos.X, tilePos.Y), color);

				var offset = MiscUtils.GetLightingDrawOffset();

				var rect = new Rectangle(tile.frameX, tile.frameY, 16, 16);

				if(net.liquidType != MachineLiquidID.None){
					//Adjust the frame to the proper subset
					int subsetHeight = texture.Frame(1, 4, 0, 0).Height;

					if(factor < 0.3f)
						rect.Y += subsetHeight * 3;
					else if(factor < 0.6f)
						rect.Y += subsetHeight * 2;
					else if(factor < 0.90f)
						rect.Y += subsetHeight;
				}

				spriteBatch.Draw(texture, tilePos.ToWorldCoordinates(0, 0) + offset - Main.screenPosition, rect, color * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
		}
	}
}
