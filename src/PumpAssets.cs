using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
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
	}

	internal class ModAssetsSystem : ModSystem {
		public override void PostSetupContent() {
			foreach (var pump in ModContent.GetContent<BasePumpTile>())
				ModAssets.pumpBar[pump.Type] = ModContent.Request<Texture2D>(pump.BarTexture);
		}
	}
}
