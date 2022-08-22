using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public abstract class MachineItem : ModItem{
		public TagCompound entityData;

        protected override bool CloneNewInstances => true;

		public abstract int TileType{ get; }

		internal ModTile MachineTile => ModContent.GetModTile(TileType);

		internal MachineEntity Machine => TileUtils.tileToEntity[TileType];

		/// <summary>
		/// Gets the flux usage as a string
		/// </summary>
		/// <param name="perGameTick">Whether the TF/t and TF/s are displayed (<see langword="true"/>) or the per-operation TF is displayed (<see langword="false"/>)</param>
		protected string GetMachineFluxUsageString(bool perGameTick){
			//Ensure that flags/etc. are set properly on the theoretical machine
			if(!(Machine is PoweredMachineEntity pme))
				return null;

			float flux = (float)pme.FluxUsage;

			return perGameTick ? $"{flux :0.###} TF/t ({flux * 60 :0.###} TF/s)" : $"{flux :0.###} TF/operation";
		}

		public abstract string ItemName{ get; }
		public abstract string ItemTooltip{ get; }

		public ScienceWorkbenchItemRegistry ItemRegistry{ get; private set; }

		internal abstract ScienceWorkbenchItemRegistry GetRegistry();

		public sealed override void SetStaticDefaults(){
			string name = ItemName;
			string tooltip = ItemTooltip;
			if(name != null)
				DisplayName.SetDefault(name);
			if(tooltip != null)
				Tooltip.SetDefault(tooltip + "\n<>");

			ItemRegistry = GetRegistry();
		}

		public sealed override void SaveData(TagCompound tag)
			=> tag = entityData;

		public sealed override void LoadData(TagCompound tag){
			entityData = tag;
		}

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips){
			int index = tooltips.FindIndex(tl => tl.Text == "<>");
			if(index >= 0){
				var type = this.GetType();
				if(entityData != null && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DatalessMachineItem<>))){
					tooltips[index].Text = "[c/dddd00:This machine contains entity data.]";

					if(TileUtils.tileToEntity[TileType] is PoweredMachineEntity pme){
						// root -> "extra" -> "flux"
						tooltips.Insert(++index, new TooltipLine(TechMod.Instance, "PowerDescription", $"[c/dddd00:{entityData.GetCompound("extra").GetFloat("flux")} / {(float)pme.FluxCap} TF]"));
					}
				}else
					tooltips.RemoveAt(index);
			}
		}
	}

	public abstract class MachineItem<T> : MachineItem where T : Machine{
		public sealed override int TileType => ModContent.TileType<T>();

		public virtual void SafeSetDefaults(){ }

		public sealed override void SetDefaults(){
			SafeSetDefaults();
			/*Item.melee = false;
			Item.magic = false;
			Item.ranged = false;
			Item.summon = false;
			Item.thrown = false;*/
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.createTile = TileType;
			Item.maxStack = 1;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}
	}

	public static class DatalessMachineInfo{
		public static Dictionary<int, Action<Mod>> recipes;

		public static Dictionary<int, RecipeIngredientSet> recipeIngredients;

		public static void Register<TItem>(RecipeIngredientSet ingredients) where TItem : MachineItem{
			recipes.Add(ModContent.ItemType<TItem>(), mod => RecipeUtils.ScienceWorkbenchRecipe<TItem>(mod, ingredients));
			recipeIngredients.Add(ModContent.ItemType<TItem>(), ingredients);
		}
	}

    [Autoload(false)]
    public sealed class DatalessMachineItem<T> : MachineItem where T : MachineItem{
		//Need a parametered ctor here to prevent tModLoader from trying to autoload this Item, even with Autoload returning false
#pragma warning disable IDE0060
		public DatalessMachineItem(bool b = false){ }
#pragma warning restore IDE0060

		public override string Texture => ModContent.GetInstance<T>().Texture;

		public override string ItemName => ModContent.GetInstance<T>().ItemName;
		public override string ItemTooltip => ModContent.GetInstance<T>().ItemTooltip;

		public override int TileType => ModContent.GetInstance<T>().TileType;

		// TODO: work on science workbench ui
		internal override ScienceWorkbenchItemRegistry GetRegistry() => ModContent.GetInstance<T>().GetRegistry();


		public sealed override void SetDefaults(){
			Item.CloneDefaults(ModContent.ItemType<T>());
			Item.maxStack = 99;
		}

		public override void AddRecipes(){
			try{
				DatalessMachineInfo.recipes[ModContent.ItemType<T>()](TechMod.Instance);
			}catch(KeyNotFoundException){
				throw new Exception($"Machine \"{typeof(T).Name}\" does not have a recipe");
			}
		}
	}
}
