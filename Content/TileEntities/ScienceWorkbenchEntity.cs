using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;
using Terraria.Audio;

namespace TerraScience.Content.TileEntities{
	public class ScienceWorkbenchEntity : MachineEntity{
		public int curRecipeType;
		public int curRecipeStack;
		public List<Recipe> curRecipes = new List<Recipe>();

		public bool HasRecipe;

		public override bool RequiresUI => true;

		public override int MachineTile => ModContent.TileType<ScienceWorkbench>();

		public override int SlotsCount => 10;

		public override void PreUpdateReaction(){
			//Force UpdateReaction() to execute, since this method will only run if the UI is visible
			ReactionInProgress = true;
		}

		public override bool UpdateReaction(){
			ScienceWorkbenchUI ui = ParentState as ScienceWorkbenchUI;
			return ui.UpdateSuccess;
		}

		public override void ReactionComplete(){
			ScienceWorkbenchUI ui = ParentState as ScienceWorkbenchUI;
			Item resultSlot = ui.GetSlot(ui.SlotsLength - 1).StoredItem;
			
			resultSlot.stack += curRecipeStack;

			Recipe recipe = curRecipes[ui.CurrentRecipe];

			for(int i = 0; i < ui.SlotsLength - 1; i++){
				Item slot = ui.GetSlot(i).StoredItem;
				Item recipeItem = recipe.requiredItem[i];

				if(!recipeItem.IsAir && !slot.IsAir){
					slot.stack -= recipeItem.stack;

					if(slot.stack <= 0)
						slot.TurnToAir();
				}
			}

			HasRecipe = RecipeUtils.HasRecipe(ui, out curRecipeType, out curRecipeStack, curRecipes);

			SoundEngine.PlaySound(SoundID.Grab);
		}

		internal override int[] GetInputSlots() => System.Array.Empty<int>();

		internal override int[] GetOutputSlots() => new int[]{ SlotsCount - 1 };

		internal override bool CanInputItem(int slot, Item item) => false;
	}
}
