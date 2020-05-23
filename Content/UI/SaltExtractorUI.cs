using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class SaltExtractorUI : MachineUI{
		public override string Header => "Salt Extractor";

		public override Tile[,] Structure => TileUtils.Structures.SaltExtractor;

		internal override void PanelSize(out int width, out int height){
			width = 300;
			height = 230;
		}

		internal override void InitializeText(List<UIText> text){
			UIText waterValues = new UIText("0L / 10L", 1.3f) {
				HAlign = 0.5f
			};
			waterValues.Top.Set(58, 0);
			text.Add(waterValues);

			UIText progress = new UIText("Processing: None", 1.3f) {
				HAlign = 0.5f
			};
			progress.Top.Set(87, 0);
			text.Add(progress);

			UIText reactionSpeed = new UIText("Speed Multiplier: 1x", 1.3f) {
				HAlign = 0.5f
			};
			reactionSpeed.Top.Set(116, 0);
			text.Add(reactionSpeed);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot itemSlot_Salt = new UIItemSlot {
				HAlign = 0.3333f,
				ValidItemFunc = item => item.IsAir
			};
			itemSlot_Salt.Top.Set(152, 0);
			slots.Add(itemSlot_Salt);

			UIItemSlot itemSlot_Water = new UIItemSlot{
				HAlign = 0.6667f,
				ValidItemFunc = item => item.IsAir
			};
			itemSlot_Water.Top.Set(152, 0);
			slots.Add(itemSlot_Water);
		}

		internal override void UpdateText(List<UIText> text){
			SaltExtractorEntity se = UIEntity as SaltExtractorEntity;

			text[0].SetText($"{UIDecimalFormat(se.StoredLiquid)}L / {Math.Round(SaltExtractorEntity.MaxLiquid)}L");
			text[1].SetText($"Processing: {Enum.GetName(typeof(SaltExtractorEntity.SE_LiquidType), se.LiquidType)}");
			text[2].SetText($"Speed Multiplier: {UIDecimalFormat(se.ReactionSpeed)}x");
		}
	}
}
