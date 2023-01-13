using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.API;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Networks;

namespace TerraScience.Content.Items.Networks {
	/// <summary>
	/// A base implementation for an item that can place a <see cref="BasePumpTile"/> tile
	/// </summary>
	public abstract class BasePumpItem : BaseNetworkEntryPlacingItem {
		// Code was copied from SerousEnergyLib/src/Items/NetworkJunctionItem.cs
		public override void SetStaticDefaults() {
			// Frame gets overwritten in custom PlayerDrawLayers
			Main.RegisterItemAnimation(Type, new DrawAnimationHorizontal(1000, 4) {
				NotActuallyAnimating = true
			});
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.useTime = 15;
			Item.createTile = -1;
		}

		public override bool AltFunctionUse(Player player) => true;

		private bool switchingMode;

		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				switchingMode = Item.createTile != -1;

				// Cycle to the next style and prevent placement
				if (!switchingMode)
					Item.placeStyle = ++Item.placeStyle % 4;

				Item.createTile = -1;
				Item.useStyle = ItemUseStyleID.HoldUp;
			} else {
				switchingMode = Item.createTile == -1;

				// Allow placement of the tile
				Item.createTile = NetworkTile;
				Item.useStyle = ItemUseStyleID.Swing;
			}

			return true;
		}

		public override bool ConsumeItem(Player player) {
			// Right click should not consume the item, only rotate it
			return player.altFunctionUse != 2 && !switchingMode;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			// Fool the game into thinking that the junction item has an animation, when it really doesn't
			DrawAnimationHorizontal animation = Main.itemAnimations[Type] as DrawAnimationHorizontal;
			animation.Frame = 0;

			var texture = TextureAssets.Item[Type].Value;
			frame = texture.Frame(4, 1, Item.placeStyle, 0);
			spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			// Fool the game into thinking that the junction item has an animation, when it really doesn't
			DrawAnimationHorizontal animation = Main.itemAnimations[Type] as DrawAnimationHorizontal;
			animation.Frame = 0;

			var texture = TextureAssets.Item[Type].Value;
			Rectangle frame = texture.Frame(4, 1, Item.placeStyle, 0);
			spriteBatch.Draw(texture, Item.Center - Main.screenPosition, frame, lightColor, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0);
			return false;
		}
	}

	/// <inheritdoc cref="BasePumpItem"/>
	public abstract class BasePumpItem<T> : BasePumpItem where T : BasePumpTile {
		public sealed override int NetworkTile => ModContent.TileType<T>();
	}
}
