using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Common;
using TerraScience.Content.Items.Networks.Fluids;

namespace TerraScience.Content.Tiles.Networks.Fluids {
	public class BasicFluidTransportTile : BaseNetworkEntryTile<BasicFluidTransportItem>, IFluidTransportTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual double MaxCapacity => 0.25;  // 0.25 Liters
		public virtual double ExportRate => 0.5 / 60d;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			NetworkDrawing.DrawFluid(spriteBatch,
				ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidTransportTile_fluid"),
				new Point16(i, j),
				columnsPerSet: 7,
				rowsPerSet: 4);

			return true;
		}

		
	}
}
