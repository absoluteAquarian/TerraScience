using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Systems;

namespace TerraScience.Content.UI{
	public class TesseractNetworkUI : UIState{
		private UIDragablePanel panel;

		private UIPanel[] visibleKnownNetworkPanels;
		private UIText[] visibleKnownNetworks;

		//The UI that this network UI is bound to
		internal TesseractUI boundUI;

		public override void OnInitialize(){
			//Make the panel
			panel = new UIDragablePanel();

			panel.Width.Set(400, 0);
			panel.Height.Set(320, 0);
			panel.HAlign = panel.VAlign = 0.5f;
			Append(panel);

			//Make the header text
			UIText header = new UIText("Terract Network", 1, true) {
				HAlign = 0.5f
			};
			header.Top.Set(10, 0);
			panel.Append(header);

			//Make the panel containing the known networks
			UIPanel knownPanel = new UIPanel();
			knownPanel.Width.Set(0, 0.65f);
			knownPanel.Height.Set(250, 0);
			knownPanel.Left.Set(20, 0);
			knownPanel.Top.Set(50, 0);
			panel.Append(knownPanel);

			UIText dummy = new UIText("Very Cool Network");

			float dummyHeight = dummy.GetDimensions().Height;
			int rows = (int)(knownPanel.GetInnerDimensions().Height / dummyHeight);

			visibleKnownNetworkPanels = new UIPanel[rows];
			visibleKnownNetworks = new UIText[rows];

			float top = 0;
			for(int i = 0; i < rows; i++){
				UIPanel networkPanel = visibleKnownNetworkPanels[i] = new UIPanel();
				networkPanel.SetPadding(4);
				networkPanel.Width.Set(0, 1f);
				networkPanel.Height.Set(dummyHeight - 8, 0);
				networkPanel.Left.Set(0, 0);
				networkPanel.Top.Set(top, 0);
				knownPanel.Append(networkPanel);

				var panelDims = networkPanel.GetDimensions();

				UIText networkText = visibleKnownNetworks[i] = new UIText("");
				networkPanel.Append(networkText);

				UIImageButton config = new UIImageButton(UICommon.ButtonModConfigTexture);
				config.Left.Set(panelDims.Width - config.Width.Pixels, 0);
				config.Top.Set(top + panelDims.Height / 2 - config.Height.Pixels, 0);
				config.OnClick += (evt, e) => {
					// TODO: pull up another menu for setting the configuration of the network
				};
				config.SetVisibility(whenActive: 1f, whenInactive: 0.65f);
				networkPanel.Append(config);

				top += dummyHeight;
			}

			//Set the other buttons
		}

		public override void Update(GameTime gameTime){
			base.Update(gameTime);

			Main.playerInventory = true;

			if(Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed){
				TechMod.Instance.machineLoader.tesseractNetworkInterface.SetState(null);
				Main.playerInventory = false;
			}
		}
	}
}
