using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public abstract class MachineItem : ModItem{
		public TagCompound entityData;

		public override bool CloneNewInstances => true;

		public abstract int TileType{ get; }

		public abstract string ItemName{ get; }
		public abstract string ItemTooltip{ get; }

		public sealed override void SetStaticDefaults(){
			string name = ItemName;
			string tooltip = ItemTooltip;
			if(name != null)
				DisplayName.SetDefault(name);
			if(tooltip != null)
				Tooltip.SetDefault(tooltip + "\n<>");
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

		public static void Register<TItem>((int type, int stack)[] items) where TItem : MachineItem{
			recipes.Add(ModContent.ItemType<TItem>(), mod => RecipeUtils.ScienceWorkbenchRecipe<TItem>(mod, items));
		}

		public static void Register<TItem>((short type, int stack)[] items) where TItem : MachineItem{
			recipes.Add(ModContent.ItemType<TItem>(), mod => RecipeUtils.ScienceWorkbenchRecipe<TItem>(mod, items));
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
