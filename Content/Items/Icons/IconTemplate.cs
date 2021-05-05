using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Items.Tools;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Icons{
	public class IconTemplate : ModItem{
		public override bool Autoload(ref string name) => false;

		public IconTemplate(){ }

		public readonly string MachineName;

		public static Dictionary<string, Action<ScienceRecipe, IconTemplate>> allRecipes;

		public IconTemplate(string machine, int[] materials, int[] stacks){
			if(materials.Length != stacks.Length)
				throw new Exception("Material array length did not match material stacks array length");

			MachineName = string.Concat(machine.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

			Action<ScienceRecipe, IconTemplate> r = new Action<ScienceRecipe, IconTemplate>((recipe, item) => {
				recipe.AddIngredient(ModContent.ItemType<Hammer>());

				for(int i = 0; i < materials.Length; i++)
					recipe.AddIngredient(materials[i], stacks[i]);

				recipe.SetResult(item, 1);
				recipe.AddRecipe();
			});
			allRecipes.Add(MachineName, r);
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(MachineName);
		}

		public override void AddRecipes(){
			allRecipes[MachineName](new ScienceRecipe(mod), this);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
