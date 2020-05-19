using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class ScienceWorkbenchEntity : MachineEntity{
		public int curRecipeType;
		public int curRecipeStack;
		public List<Recipe> curRecipes = new List<Recipe>();

		public bool HasRecipe;

		public override bool RequiresUI => true;

		public override int MachineTile => ModContent.TileType<ScienceWorkbench>();

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

				if(!recipeItem.IsAir && !slot.IsAir)
					slot.stack -= recipeItem.stack;
			}

			HasRecipe = RecipeUtils.HasRecipe(ui, out curRecipeType, out curRecipeStack, curRecipes);

			Main.PlaySound(SoundID.Grab);
		}
	}
}
