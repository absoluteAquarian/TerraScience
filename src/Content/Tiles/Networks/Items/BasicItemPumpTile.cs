﻿using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using TerraScience.Content.Items.Networks.Items;

namespace TerraScience.Content.Tiles.Networks.Items {
	public class BasicItemPumpTile : BasePumpTile<BasicItemPumpItem>, IItemPumpTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual int StackPerExtraction => 1;

		public virtual float GetItemSize(int x, int y) => 3.85f * 2;

		public override int GetMaxTimer(int x, int y) => 22;
	}
}
