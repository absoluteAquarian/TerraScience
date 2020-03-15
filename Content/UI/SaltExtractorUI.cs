using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI {
	public class SaltExtractorUILoader {
		public UserInterface saltExtractorInterface;

		public SaltExtractorUI saltExtractorUI;

		private GameTime lastUpdateUIGameTime;

		/// <summary>
		/// Called on Mod.Load
		/// </summary>
		internal void Load() {
			if (!Main.dedServ) {
				saltExtractorInterface = new UserInterface();
				saltExtractorUI = new SaltExtractorUI();

				// Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
				saltExtractorUI.Activate();
			}
		}

		/// <summary>
		/// Called on Mod.UpdateUI
		/// </summary>
		internal void UpdateUI(GameTime gameTime) {
			lastUpdateUIGameTime = gameTime;

			if (saltExtractorInterface?.CurrentState != null)
				saltExtractorInterface.Update(gameTime);
		}

		/// <summary>
		/// Called on Mod.ModifyInterfaceLayers
		/// </summary>
		internal void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"TerraScience: SaltExtractorInterface",
					delegate {
						if (lastUpdateUIGameTime != null && saltExtractorInterface?.CurrentState != null)
							saltExtractorInterface.Draw(Main.spriteBatch, lastUpdateUIGameTime);

						return true;
					}, InterfaceScaleType.UI));
			}
		}

		/// <summary>
		/// Called on Mod.Unload
		/// </summary>
		internal void Unload() => saltExtractorUI = null;

		public void ShowUI(UIState state, SaltExtractorEntity entity) {
			Main.PlaySound(SoundID.MenuOpen);
			saltExtractorInterface.SetState(state);
			saltExtractorUI.SaltExtractor = entity;
		}

		public void HideUI() {
			Main.PlaySound(SoundID.MenuClose);
			saltExtractorInterface.SetState(null);
		}
	}

	public class SaltExtractorUI : UIState {
		/// <summary>
		/// The Salt Extractor tile entity. Set when UI is shown.
		/// </summary>
		public SaltExtractorEntity SaltExtractor { get; internal set; } = null;

		private UIText waterValues;

		private UIText progress;

		private UIText reactionSpeed;

		internal UIItemSlot itemSlot;

		public override void OnInitialize() {
			UIDragablePanel panel = new UIDragablePanel();
			panel.Width.Set(300, 0);
			panel.Height.Set(230, 0);
			panel.HAlign = panel.VAlign = 0.5f;
			Append(panel);

			UIText header = new UIText("Salt Extractor", 1, true) {
				HAlign = 0.5f
			};

			header.Top.Set(10, 0);
			panel.Append(header);

			waterValues = new UIText("Water: 0L / 10L", 1.3f) {
				HAlign = 0.5f
			};

			waterValues.Top.Set(58, 0);
			panel.Append(waterValues);

			progress = new UIText("Progress: 0%", 1.3f) {
				HAlign = 0.5f
			};

			progress.Top.Set(87, 0);
			panel.Append(progress);

			reactionSpeed = new UIText("Reaction Speed: 1x", 1.3f) {
				HAlign = 0.5f
			};

			reactionSpeed.Top.Set(116, 0);
			panel.Append(reactionSpeed);

			itemSlot = new UIItemSlot {
				HAlign = 0.5f,
				ValidItemFunc = item => item.IsAir //|| !item.IsAir && item.type == ModContent.GetInstance<TerraScience>().ItemType("SodiumChloride") //this following commented code makes the player able to place salt in the slot
			};

			itemSlot.Top.Set(152, 0);
			panel.Append(itemSlot);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			Main.playerInventory = true;

			if (SaltExtractor != null) {
				// Get the salt extractor's ceter position
				Vector2 middle = TileUtils.TileEntityCenter(SaltExtractor, TileUtils.Structures.SaltExtractor);

				// check if the inventory key was pressed or if the player is 5 blocks away from the tile. if so close UI
				if (Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed || Vector2.Distance(Main.LocalPlayer.Center, middle) > 5 * 16) {
					ModContent.GetInstance<TerraScience>().saltExtracterLoader.HideUI();
					Main.playerInventory = false;
				}

				waterValues.SetText($"Water: {string.Format("{0:G29}", decimal.Parse($"{SaltExtractor.StoredWater:N2}"))}L / {Math.Round(SaltExtractorEntity.MaxWater)}L");
				progress.SetText($"Progress: {Math.Round(SaltExtractor.ReactionProgress)}%");
				reactionSpeed.SetText($"Speed Multiplier: {string.Format("{0:G29}", decimal.Parse($"{SaltExtractor.ReactionSpeed:N2}"))}x");
			}
		}
	}
}