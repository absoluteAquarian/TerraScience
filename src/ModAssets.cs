using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Networks;

namespace TerraScience {
	/// <summary>
	/// A helper class containing cached <see cref="Asset{T}"/> objects
	/// </summary>
	public static class ModAssets {
		private class Loadable : ILoadable {
			public void Load(Mod mod) { }

			public void Unload() {
				pumpBar.Clear();
			}
		}

		internal static readonly Dictionary<int, Asset<Texture2D>> pumpBar = new();
		public static IReadOnlyDictionary<int, Asset<Texture2D>> PumpBar => pumpBar;

		public static Asset<Texture2D> FluidFill { get; internal set; }

		public static Asset<Texture2D> FluidPumpFill { get; internal set; }
	}

	internal class ModAssetsSystem : ModSystem {
		public override void PostSetupContent() {
			if (Main.dedServ)
				return;

			foreach (var pump in ModContent.GetContent<BasePumpTile>())
				ModAssets.pumpBar[pump.Type] = ModContent.Request<Texture2D>(pump.BarTexture);

			ModAssets.FluidFill = ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidTransportTile_fluid");
			ModAssets.FluidPumpFill = ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidPumpTile_fluid");
		}
	}
}
