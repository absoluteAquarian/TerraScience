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
		private readonly List<Item> slots = new List<Item>();

		public int curRecipeType;
		public int curRecipeStack;
		public List<Recipe> curRecipes = new List<Recipe>();

		public bool HasRecipe;

		public override bool RequiresUI() => true;

		public override int GetTileType() => ModContent.TileType<ScienceWorkbench>();

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				["types"] = slots.Select(i => i.type).ToList(),
				["stacks"] = slots.Select(i => i.stack).ToList()
			};

		public override void ExtraLoad(TagCompound tag){
			if(!(tag.GetList<int>("types") is List<int> types) || !(tag.GetList<int>("stacks") is List<int> stacks) || types.Count != stacks.Count)
				return;

			for(int i = 0; i < types.Count; i++){
				Item item = new Item();
				item.SetDefaults(types[i]);
				item.stack = stacks[i];

				slots.Add(item);
			}
		}

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

		public void SaveSlots(){
			slots.Clear();

			for(int i = 0; i < ParentState.SlotsLength; i++){
				int type = ParentState.GetSlot(i).StoredItem.type;
				int stack = ParentState.GetSlot(i).StoredItem.stack;

				Item item = new Item();
				item.SetDefaults(type);
				item.stack = stack;

				slots.Add(item);
			}
		}

		public void LoadSlots(){
			ParentState.LoadToSlots(slots);
			(ParentState as ScienceWorkbenchUI).PromptForRecipes = true;
		}
	}
}
