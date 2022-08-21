using System.Collections.Generic;
using System.Text;
using Terraria.GameContent;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;

namespace TerraScience.Content.UI.Energy{
	public class AutoExtractinatorUI : PoweredMachineUI{
		public override string Header => "Auto-Extractinator";

		public override int TileType => ModContent.TileType<AutoExtractinator>();

		public ClickableButton getCoins;

		internal override void PanelSize(out int width, out int height){
			width = 580;
			height = 400;
		}

		internal override void InitializeText(List<UIText> text){
			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(48, 0);
			text.Add(power);

			UIText reactionSpeed = new UIText("Speed Multiplier: 1x", 1.3f) {
				HAlign = 0.5f
			};
			reactionSpeed.Top.Set(78, 0);
			text.Add(reactionSpeed);

			UIText coinCounts = new UIText($"No coins", 1, false);
			coinCounts.Top.Set(340, 0);
			coinCounts.Left.Set(40, 0);
			text.Add(coinCounts);
		}

		internal override void InitializeSlots(List<UIItemSlotWrapper> slots){
			//Copied from ScienceWorkbenchUI lul
			int top = 150;
			int origLeft = 30 + 200, left;
			const int buffer = 8;

			UIItemSlotWrapper input = new UIItemSlotWrapper(){
				ValidItemFunc = item => item.IsAir || ItemID.Sets.ExtractinatorMode[item.type] >= 0,
				VAlign = 0.5f
			};
			input.Left.Set(80, 0);
			slots.Add(input);

			//20 slots, 5 per row
			for(int r = 0; r < 4; r++){
				left = origLeft;
				for(int c = 0; c < 5; c++){
					//Can't place items, only remove them
					UIItemSlotWrapper slot = new UIItemSlotWrapper(){
						ValidItemFunc = item => item.IsAir
					};
					slot.Left.Set(left, 0);
					slot.Top.Set(top, 0);

					left += TextureAssets.InventoryBack9.Value.Width + buffer;

					slots.Add(slot);
				}
				top += TextureAssets.InventoryBack9.Value.Height + buffer;
			}
		}

		internal override void InitializeOther(UIPanel panel){
			getCoins = new ClickableButton("Collect Coins");
			getCoins.Top.Set(300, 0);
			getCoins.Left.Set(35, 0);
			panel.Append(getCoins);
		}

		internal override void UpdateEntity(){
			AutoExtractinatorEntity entity = UIEntity as AutoExtractinatorEntity;

			if(getCoins.LeftClick)
				entity.SpawnCoinsOnLocalPlayer();
		}

		internal override void UpdateText(List<UIText> text){
			AutoExtractinatorEntity entity = UIEntity as AutoExtractinatorEntity;

			text[0].SetText(GetFluxString());
			text[1].SetText($"Speed Multiplier: {UIDecimalFormat(entity.ReactionSpeed)}x");
			if(entity.storedCoins == 0)
				text[2].SetText("No coins");
			else{
				var coins = Utils.CoinsSplit(entity.storedCoins);

				StringBuilder sb = new StringBuilder(100);
				if(coins[3] > 0)
					sb.Append($"[i/s{coins[3]}:{ItemID.PlatinumCoin}]");
				if(coins[2] > 0)
					sb.Append($"[i/s{coins[2]}:{ItemID.GoldCoin}]");
				if(coins[1] > 0)
					sb.Append($"[i/s{coins[1]}:{ItemID.SilverCoin}]");
				if(coins[0] > 0)
					sb.Append($"[i/s{coins[0]}:{ItemID.CopperCoin}]");

				text[2].SetText(sb.ToString());
			}
		}
	}
}
