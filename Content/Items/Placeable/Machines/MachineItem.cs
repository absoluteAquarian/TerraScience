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

		public override bool CloneNewInstances => true;

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

		public sealed override TagCompound Save()
			=> entityData;

		public sealed override void Load(TagCompound tag){
			entityData = tag;
		}

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips){
			int index = tooltips.FindIndex(tl => tl.text == "<>");
			if(index >= 0){
				var type = this.GetType();
				if(entityData != null && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DatalessMachineItem<>))){
					tooltips[index].text = "[c/dddd00:This machine contains entity data.]";

					if(TileUtils.tileToEntity[TileType] is PoweredMachineEntity pme){
						// root -> "extra" -> "flux"
						tooltips.Insert(++index, new TooltipLine(mod, "PowerDescription", $"[c/dddd00:{entityData.GetCompound("extra").GetFloat("flux")} / {(float)pme.FluxCap} TF]"));
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
			item.melee = false;
			item.magic = false;
			item.ranged = false;
			item.summon = false;
			item.thrown = false;
			item.useTime = 10;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.createTile = TileType;
			item.maxStack = 1;
			item.consumable = true;
			item.autoReuse = true;
			item.useTurn = true;
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

	public sealed class DatalessMachineItem<T> : MachineItem where T : MachineItem{
		//Need a parametered ctor here to prevent tModLoader from trying to autoload this item, even with Autoload returning false
#pragma warning disable IDE0060
		public DatalessMachineItem(bool b = false){ }
#pragma warning restore IDE0060

		public override string Texture => ModContent.GetInstance<T>().Texture;

		public override string ItemName => ModContent.GetInstance<T>().ItemName;
		public override string ItemTooltip => ModContent.GetInstance<T>().ItemTooltip;

		public override int TileType => ModContent.GetInstance<T>().TileType;

		// TODO: work on science workbench ui
		internal override ScienceWorkbenchItemRegistry GetRegistry() => ModContent.GetInstance<T>().GetRegistry();

		public sealed override bool Autoload(ref string name) => false;

		public sealed override void SetDefaults(){
			item.CloneDefaults(ModContent.ItemType<T>());
			item.maxStack = 99;
		}

		public override void AddRecipes(){
			try{
				DatalessMachineInfo.recipes[ModContent.ItemType<T>()](mod);
			}catch(KeyNotFoundException){
				throw new Exception($"Machine \"{typeof(T).Name}\" does not have a recipe");
			}
		}
	}
}
