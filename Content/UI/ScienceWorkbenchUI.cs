using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.API.UI;
using TerraScience.Content.Items;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class ScienceWorkbenchUI : MachineUI{
		public ClickableButton craft;
		public ClickableButton nextRecipe;
		public ClickableButton prevRecipe;

		public int CurrentRecipe;
		public bool PromptForRecipes;
		public bool UpdateSuccess;

		public override string Header => "Science Workbench";

		public override Tile[,] Structure => TileUtils.Structures.ScienceWorkbench;

		internal override void PanelSize(out int width, out int height){
			width = 400;
			height = 400;
		}

		public override void PreUpdateEntity(){
			PromptForRecipes = false;

			nextRecipe.Update(new GameTime());
			prevRecipe.Update(new GameTime());
			craft.Update(new GameTime());
		}

		public override void UpdateMisc(){
			ScienceWorkbenchEntity entity = UIEntity as ScienceWorkbenchEntity;

			if(nextRecipe.LeftClick && GetSlot(SlotsLength - 1).StoredItem.IsAir){
				CurrentRecipe++;

				PromptForRecipes = true;

				if(CurrentRecipe > entity.curRecipes.Count)
					CurrentRecipe = entity.curRecipes.Count - 1;

				Main.PlaySound(SoundID.MenuTick);
			}else if(prevRecipe.LeftClick && GetSlot(SlotsLength - 1).StoredItem.IsAir){
				CurrentRecipe--;

				PromptForRecipes = true;

				if(CurrentRecipe < 0)
					CurrentRecipe = 0;

				Main.PlaySound(SoundID.MenuTick);
			}
		}

		internal override void UpdateText(List<UIText> text){
			ScienceWorkbenchEntity entity = UIEntity as ScienceWorkbenchEntity;

			if(entity.curRecipeType > 0)
				text[0].SetText($"Crafting: {Lang.GetItemNameValue(entity.curRecipeType)}");
			else{
				text[0].SetText("No valid recipe!");

				craft.BackgroundColor = new Color(30, 40, 100) * 0.7f;
			}
		}

		internal override void UpdateEntity(){
			UpdateSuccess = false;
			ScienceWorkbenchEntity entity = UIEntity as ScienceWorkbenchEntity;

			Item resultSlot = GetSlot(SlotsLength - 1).StoredItem;

			//Check if any of the ingredients have been added/removed
			for(int i = 0; i < SlotsLength - 1; i++){
				if(GetSlot(i).ItemChanged){
					PromptForRecipes = true;
					break;
				}
			}

			if(PromptForRecipes){
				entity.HasRecipe = RecipeUtils.HasRecipe(this, out entity.curRecipeType, out entity.curRecipeStack, entity.curRecipes);
				PromptForRecipes = false;
			}

			if(entity.HasRecipe && craft.LeftClick){
				//The recipe found an item.  Copy the defaults if it's air.
				if(entity.curRecipeType > 0 && resultSlot.type <= ItemID.None){
					resultSlot.SetDefaults(entity.curRecipeType);
					resultSlot.stack = 0;
				}

				//We have room to add another item
				if(resultSlot.stack + entity.curRecipeStack < resultSlot.maxStack){
					entity.ReactionProgress = 100;
					UpdateSuccess = true;
				}else
					UpdateSuccess = false;
			}else
				UpdateSuccess = false;
		}

		internal override void InitializeText(List<UIText> text) {
			UIText crafting = new UIText("No valid recipe!", 1, false){
				HAlign = 0.5f
			};
			crafting.Top.Set(58, 0);
			text.Add(crafting);
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			int top = 100;
			int origLeft = 30, left;

			//10 ingredient slots, 5 per row
			for(int r = 0; r < 2; r++){
				left = origLeft;
				for(int c = 0; c < 5; c++){
					UIItemSlot ingredient = new UIItemSlot(){
						ValidItemFunc = item => true
					};
					ingredient.Left.Set(left, 0);
					ingredient.Top.Set(top, 0);

					left += Main.inventoryBack9Texture.Width + 15;

					slots.Add(ingredient);
				}
				top += Main.inventoryBack9Texture.Height + 15;
			}

			top = 300;
			left = 400 - (Main.inventoryBack9Texture.Width + 15) - 30;

			//1 result slot
			for(int c = 0; c < 1; c++){
				UIItemSlot ingredient = new UIItemSlot(){
					ValidItemFunc = item => item.IsAir
				};
				ingredient.Left.Set(left, 0);
				ingredient.Top.Set(top, 0);

				left += Main.inventoryBack9Texture.Width + 15;

				slots.Add(ingredient);
			}
		}

		internal override void InitializeOther(UIPanel panel){
			craft = new ClickableButton(Language.GetTextValue("LegacyMisc.72")){
				HAlign = 0.2f
			};
			craft.Top.Set(300, 0);
			panel.Append(craft);

			nextRecipe = new ClickableButton(">"){
				HAlign = 0.7f
			};
			nextRecipe.Top.Set(300, 0);
			panel.Append(nextRecipe);

			prevRecipe = new ClickableButton("<"){
				HAlign = 0.55f
			};
			prevRecipe.Top.Set(300, 0);
			panel.Append(prevRecipe);
		}

		public override void PreClose(){
			PromptForRecipes = true;
			UpdateSuccess = false;
		}

		public override void DoSavedItemsCheck(){
			PromptForRecipes = true;
		}
	}
}
