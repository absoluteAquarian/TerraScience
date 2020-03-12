using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraScience.Content.UI {
	public class SaltExtractorLoader {
		internal UserInterface userInterface;

		internal SaltExtractorUI saltExtractorUI;

		private GameTime lastUpdateUIGameTime;

		/// <summary>
		/// Called on Mod.Load
		/// </summary>
		internal void Load() {
			if (!Main.dedServ) {
				userInterface = new UserInterface();
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
			if (userInterface?.CurrentState != null) {
				userInterface.Update(gameTime);
			}
		}

		/// <summary>
		/// Called on Mod.ModifyInterfaceLayers
		/// </summary>
		internal void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"TerraScience: SaltExtractorInterface",
					delegate
					{
						if (lastUpdateUIGameTime != null && userInterface?.CurrentState != null) {
							userInterface.Draw(Main.spriteBatch, lastUpdateUIGameTime);
						}

						return true;
					}, InterfaceScaleType.UI));
			}
		}

		/// <summary>
		/// Called on Mod.Unload
		/// </summary>
		internal void Unload() {
			saltExtractorUI?.Unload();
			saltExtractorUI = null;
		}

		public void ShowUI(UIState state) {
			userInterface.SetState(state);
		}

		public void HideUI() {
			userInterface.SetState(null);
		}
	}

	public class SaltExtractorUI : UIState {
		public override void OnInitialize() {
			UIPanel panel = new UIPanel();
			panel.Width.Set(300, 0);
			panel.Height.Set(300, 0);
			Append(panel);
		}

		internal void Unload() {
			// Anything that needs to be unloaded before getting set to null, for example, static fields.
		}

		public override void Update(GameTime gameTime) {
			//if key pressed, close the UI using
			// ModContent.GetInstance<TerraScience>().saltExtracterLoader.HideUI();
		}
	}
}
