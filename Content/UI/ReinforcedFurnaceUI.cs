using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class ReinforcedFurnaceUI : MachineUI{
		public override Tile[,] Structure => TileUtils.Structures.ReinforcedFurncace;

		public bool HasFuel => GetSlot(0).StoredItem.stack > 0;

		public override string Header => "Reinforced Furnace";

		public override void PlayOpenSound()
			=> Main.PlaySound(SoundLoader.customSoundType, Style: TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/SFX chest open"));

		public override void PlayCloseSound()
			=> Main.PlaySound(SoundLoader.customSoundType, Style: TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/SFX chest close"));

		internal override void PanelSize(out int width, out int height){
			width = 420;
			height = 230;
		}

		internal override void InitializeText(List<UIText> text){
			UIText curHeat = new UIText("Heat: ", 1.3f){
				HAlign = 0.5f
			};
			curHeat.Top.Set(58, 0);
			text.Add(curHeat);

			UIText reactionSpeed = new UIText("Speed Multiplier: 1x", 1.3f) {
				HAlign = 0.5f
			};
			reactionSpeed.Top.Set(87, 0);
			text.Add(reactionSpeed);

			UIText reaction = new UIText("Progress: 0%", 1.3f){
				HAlign = 0.5f
			};
			reaction.Top.Set(116, 0);
			text.Add(reaction);
		}

		internal override void UpdateText(List<UIText> text){
			ReinforcedFurnaceEntity entity = UIEntity as ReinforcedFurnaceEntity;

			text[0].SetText($"Heat: {DecimalFormat(entity.Heat)}°C");
			text[1].SetText($"Speed Multiplier: {DecimalFormat(entity.ReactionSpeed)}x");
			text[2].SetText($"Progress: {(int)entity.ReactionProgress}%");
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot fuel = new UIItemSlot(){
				//Wood items burn
				ValidItemFunc = item => item.IsAir || Lang.GetItemNameValue(item.type).Contains("Wood"),
				HAlign = 0.345f
			};
			fuel.Top.Set(152, 0);
			slots.Add(fuel);

			UIItemSlot carbons = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir,
				HAlign = 0.655f
			};
			carbons.Top.Set(152, 0);
			slots.Add(carbons);
		}

		internal override void UpdateEntity(){
			ReinforcedFurnaceEntity furnace = UIEntity as ReinforcedFurnaceEntity;
			UIItemSlot fuel = GetSlot(0);

			furnace.Heating = !fuel.StoredItem.IsAir;

			if(!furnace.Heating)
				furnace.ReactionProgress = 0f;
		}
	}
}
