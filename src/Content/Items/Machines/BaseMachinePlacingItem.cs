using SerousEnergyLib.Items;
using SerousEnergyLib.Tiles;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Machines {
	public abstract class BaseMachinePlacingItem : BaseMachineItem {
		public override string Texture => base.Texture.Replace("Content", "Assets");
	}

	public abstract class BaseMachinePlacingItem<T> : BaseMachineItem<T> where T : ModTile, IMachineTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");
	}

	public abstract class BaseDatalessMachinePlacingItem<TItem, TTile> : DatalessMachineItem<TItem, TTile> where TItem : BaseMachineItem<TTile> where TTile : ModTile, IMachineTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");
	}
}
