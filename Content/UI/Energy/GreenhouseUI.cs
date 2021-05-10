using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Utilities;

namespace TerraScience.Content.UI.Energy{
	public class GreenhouseUI : PoweredMachineUI{
		public override string Header => "Greenhouse";

		public override int TileType => ModContent.TileType<Greenhouse>();

		internal override void PanelSize(out int width, out int height){
			width = 450;
			height = 280;
		}

		internal override void InitializeText(List<UIText> text){
			UIText power = new UIText("Power: 0 / 0TF"){
				HAlign = 0.5f
			};
			power.Top.Set(48, 0);
			text.Add(power);

			UIText progress = new UIText("Progress: 0%", 1.3f){
				HAlign = 0.5f
			};
			progress.Top.Set(78, 0);
			text.Add(progress);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			const int topOrig = 120;
			const int leftOrig = 50;
			UIItemSlot input = new UIItemSlot();
			input.Top.Set(topOrig, 0f);
			input.Left.Set(leftOrig, 0f);
			UIItemSlot block = new UIItemSlot();
			block.Top.Set(topOrig + Main.inventoryBack9Texture.Height + 20, 0f);
			block.Left.Set(leftOrig - Main.inventoryBack9Texture.Width / 2 - 10, 0f);
			UIItemSlot modifier = new UIItemSlot();
			modifier.Top.Set(topOrig + Main.inventoryBack9Texture.Height + 20, 0f);
			modifier.Left.Set(leftOrig + Main.inventoryBack9Texture.Width / 2 + 10, 0f);

			input.ValidItemFunc = item => item.stack <= 1
				&& (item.IsAir || item.type == ItemID.Acorn || item.type == ItemID.DaybloomSeeds || item.type == ItemID.BlinkrootSeeds || item.type == ItemID.WaterleafSeeds || item.type == ItemID.DeathweedSeeds || item.type == ItemID.MoonglowSeeds || item.type == ItemID.ShiverthornSeeds || item.type == ItemID.FireblossomSeeds || item.type == ItemID.MushroomGrassSeeds || item.type == ItemID.Cactus)
				&& VerifySlots(item, block.StoredItem, modifier.StoredItem);
			block.ValidItemFunc = item => item.stack <= 1
				&& ((input.StoredItem.IsAir && item.IsAir) || item.type == ItemID.DirtBlock || item.type == ItemID.MudBlock || item.type == ItemID.SandBlock || item.type == ItemID.SnowBlock || item.type == ItemID.AshBlock)
				&& VerifySlots(input.StoredItem, item, modifier.StoredItem);
			modifier.ValidItemFunc = item => item.stack <= 1
				&& ((input.StoredItem.IsAir && item.IsAir) || item.type == ItemID.GrassSeeds || item.type == ItemID.CorruptSeeds || item.type == ItemID.CrimsonSeeds || item.type == ItemID.HallowedSeeds || item.type == ItemID.JungleGrassSeeds || item.type == ModContent.ItemType<Vial_Saltwater>())
				&& VerifySlots(input.StoredItem, block.StoredItem, item);

			slots.Add(input);
			slots.Add(block);
			slots.Add(modifier);

			int top = topOrig + 60;
			int left = leftOrig + Main.inventoryBack9Texture.Width / 2 + 10 + 120;

			for(int c = 0; c < 3; c++){
				//Can't place items, only remove them
				UIItemSlot slot = new UIItemSlot(){
					ValidItemFunc = item => item.IsAir
				};
				slot.Left.Set(left, 0);
				slot.Top.Set(top, 0);

				left += Main.inventoryBack9Texture.Width + 15;

				slots.Add(slot);
			}
		}

		private bool VerifySlots(Item input, Item block, Item modifier){
			Item existingInput = UIEntity.RetrieveItem(0);
			Item existingBlock = UIEntity.RetrieveItem(1);
			Item existingModifier = UIEntity.RetrieveItem(2);
			
			if(existingInput.IsAir && existingBlock.IsAir && existingModifier.IsAir)
				return true;

			bool CheckBlock(){
				switch(block.type){
					case ItemID.DirtBlock:
						return modifier.IsAir || modifier.type == ItemID.GrassSeeds || modifier.type == ItemID.CorruptSeeds || modifier.type == ItemID.CrimsonSeeds || modifier.type == ItemID.HallowedSeeds || modifier.type == ModContent.ItemType<Vial_Saltwater>();
					case ItemID.SnowBlock:
						return modifier.IsAir;
					case ItemID.SandBlock:
						return modifier.IsAir;
					case ItemID.MudBlock:
						return modifier.IsAir || modifier.type == ItemID.JungleGrassSeeds;
					case ItemID.AshBlock:
						return false;
					default:
						if(input.IsAir && block.IsAir && modifier.IsAir)
							return true;
						break;
				}

				return false;
			}

			bool check;
			switch(input.type){
				case ItemID.Acorn:
					check = CheckBlock();
					if(check)
						return check;
					break;
				case ItemID.DaybloomSeeds:
					return block.type == ItemID.DirtBlock && modifier.type == ItemID.GrassSeeds;
				case ItemID.BlinkrootSeeds:
					return block.type == ItemID.DirtBlock && modifier.IsAir;
				case ItemID.WaterleafSeeds:
					return block.type == ItemID.SandBlock && modifier.IsAir;
				case ItemID.ShiverthornSeeds:
					return block.type == ItemID.SnowBlock && modifier.IsAir;
				case ItemID.MoonglowSeeds:
					return block.type == ItemID.MudBlock && modifier.type == ItemID.JungleGrassSeeds;
				case ItemID.DeathweedSeeds:
					return block.type == ItemID.DirtBlock && (modifier.type == ItemID.CorruptSeeds || modifier.type == ItemID.CrimsonSeeds);
				case ItemID.FireblossomSeeds:
					return block.type == ItemID.AshBlock && modifier.IsAir;

				case ItemID.MushroomGrassSeeds:
					return block.type == ItemID.MudBlock && modifier.IsAir;

				case ItemID.Cactus:
					return block.type == ItemID.SandBlock && modifier.IsAir;
			}

			//Input was air.  Check the modifier
			check = CheckBlock();
			if(check)
				return check;

			//Input and block were air.  Just return true since the modifier will be the only item there
			if((block.type == ItemID.DirtBlock || block.type == ItemID.MudBlock) && !modifier.IsAir)
				return true;

			return false;
		}

		internal override void UpdateText(List<UIText> text){
			text[0].SetText(GetFluxString());
			text[1].SetText($"Progress: {UIDecimalFormat(UIEntity.ReactionProgress)}%");
		}
	}
}
