using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using Terraria.Audio;
using TerraScience.Utilities;
using Microsoft.Xna.Framework;

namespace TerraScience.Content.UI{
	public class ReinforcedFurnaceUI : MachineUI{
		public bool HasFuel => GetSlot(0).StoredItem.stack > 0;

		public override string Header => "Reinforced Furnace";

		public override int TileType => ModContent.TileType<ReinforcedFurnace>();

		public override void PlayOpenSound()
			=> TechMod.Instance.PlayCustomSound(-Vector2.One, "SFX chest open");

		public override void PlayCloseSound()
			=> TechMod.Instance.PlayCustomSound(-Vector2.One, "SFX chest close");

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

		public static bool ValidItem(Item item)
			=> ReinforcedFurnaceEntity.woodTypes.Contains(item.type);

		internal override void InitializeSlots(List<UIItemSlotWrapper> slots){
			UIItemSlotWrapper input = new UIItemSlotWrapper(){
				//Wood items burn
				ValidItemFunc = item => ValidItem(item),
				HAlign = 0.345f
			};
			input.Top.Set(152, 0);
			slots.Add(input);

			UIItemSlotWrapper carbons = new UIItemSlotWrapper(){
				ValidItemFunc = item => item.IsAir,
				HAlign = 0.655f
			};
			carbons.Top.Set(152, 0);
			slots.Add(carbons);
		}
	}
}
