using SerousCommonLib.API;
using SerousEnergyLib.API.Energy.Default;
using SerousEnergyLib.Tiles;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Networks {
	/// <summary>
	/// A base implementation for an item that can place a <see cref="BaseNetworkTile"/> tile
	/// </summary>
	public abstract class BaseNetworkEntryPlacingItem : ModItem {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		/// <summary>
		/// The tile ID that this item will place
		/// </summary>
		public abstract int NetworkTile { get; }

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.createTile = NetworkTile;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (TileLoader.GetTile(NetworkTile) is not BaseNetworkTile tile)
				return;  // Don't do anything

			if (tile is IItemTransportTile itemTransport) {
				TooltipHelper.FindAndModify(tooltips,
					"<ITEM_SPEED>",
					$"{itemTransport.TransportSpeed:0.###} block/s");
			}

			if (tile is IFluidTransportTile fluidTransport) {
				TooltipHelper.FindAndModify(tooltips,
					"<FLUID_CAPACITY>",
					$"{(double)fluidTransport.MaxCapacity} L");

				TooltipHelper.FindAndModify(tooltips,
					"<FLUID_RATES>",
					$"{(double)fluidTransport.ExportRate:0.###} L/gt ({(double)fluidTransport.ExportRate * 60:0.###} L/s)");
			}

			if (tile is IPowerTransportTile powerTransport) {
				var id = ModContent.GetInstance<TerraFluxTypeID>();

				TooltipHelper.FindAndModify(tooltips,
					"<POWER_CAPACITY>",
					$"{(double)powerTransport.MaxCapacity} {id.ShortName}");

				TooltipHelper.FindAndModify(tooltips,
					"<POWER_RATES>",
					$"{(double)powerTransport.TransferRate:0.###} {id.ShortName}/gt ({(double)powerTransport.TransferRate * 60:0.###} {id.ShortName}/s)");
			}

			if (tile is IItemPumpTile itemPump) {
				TooltipHelper.FindAndModify(tooltips,
					"<ITEM_PUMP_STACK>",
					$"{itemPump.StackPerExtraction}");
			}

			if (tile is IFluidPumpTile fluidPump) {
				TooltipHelper.FindAndModify(tooltips,
					"<FLUID_PUMP_EXPORT>",
					$"{fluidPump.MaxCapacity:0.###} L");
			}
		}
	}

	/// <inheritdoc cref="BaseNetworkEntryPlacingItem"/>
	public abstract class BaseNetworkEntryPlacingItem<T> : BaseNetworkEntryPlacingItem where T : BaseNetworkTile {
		public sealed override int NetworkTile => ModContent.TileType<T>();
	}
}
