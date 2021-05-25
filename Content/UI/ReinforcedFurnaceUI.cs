using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class ReinforcedFurnaceUI : MachineUI{
		public bool HasFuel => GetSlot(0).StoredItem.stack > 0;

		public override string Header => "Reinforced Furnace";

		public override int TileType => ModContent.TileType<ReinforcedFurnace>();

		public override void PlayOpenSound()
			=> Main.PlaySound(SoundLoader.customSoundType, Style: TechMod.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/SFX chest open"));

		public override void PlayCloseSound()
			=> Main.PlaySound(SoundLoader.customSoundType, Style: TechMod.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/SFX chest close"));

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

			text[0].SetText($"Heat: {UIDecimalFormat(entity.Heat)}°C");
			text[1].SetText($"Speed Multiplier: {UIDecimalFormat(entity.ReactionSpeed)}x");
			text[2].SetText($"Progress: {(int)entity.ReactionProgress}%");
		}

		public static bool ValidItem(Item item){
			string name = Lang.GetItemNameValue(item.type);
			return item.IsAir || name.Contains("Wood") || name.Contains("wood");
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			UIItemSlot input = new UIItemSlot(){
				//Wood items burn
				ValidItemFunc = item => ValidItem(item),
				HAlign = 0.345f
			};
			input.Top.Set(152, 0);
			slots.Add(input);

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
