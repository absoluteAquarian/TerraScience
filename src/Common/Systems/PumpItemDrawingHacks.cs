using SerousEnergyLib.API;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Networks;

namespace TerraScience.Common.Systems {
	internal class PreDrawHeldItem : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeldItem);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			var item = drawInfo.drawPlayer.HeldItem;

			if (item.ModItem is not BasePumpItem)
				return;

			// Fool the game into thinking that the junction item has an animation, when it really doesn't
			DrawAnimationHorizontal animation = Main.itemAnimations[item.type] as DrawAnimationHorizontal;
			animation.Frame = item.placeStyle;
		}
	}

	internal class PostDrawHeldItem : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			var item = drawInfo.drawPlayer.HeldItem;

			if (item.ModItem is not BasePumpItem)
				return;

			// Reset the frame back to 0
			DrawAnimationHorizontal animation = Main.itemAnimations[item.type] as DrawAnimationHorizontal;
			animation.Frame = 0;
		}
	}
}
