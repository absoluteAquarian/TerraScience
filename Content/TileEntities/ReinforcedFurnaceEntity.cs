using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;
using Terraria.ID;

namespace TerraScience.Content.TileEntities{
	public class ReinforcedFurnaceEntity : MachineEntity{
		// TODO: make HeatMin fluctuate based on the current season and/or biome
		public static readonly float HeatMax = 1000f;
		public static readonly float HeatMin = 20f;
		public static readonly float BaseReactionIncrease = 4.5f;

		public override int MachineTile => ModContent.TileType<ReinforcedFurnace>();

		public float Heat;
		private float targetHeat;

		public bool Heating{ get; set; }
		public bool AtMaxHeat => Heat >= HeatMax;

		//Variables used for emulating proper heat physics
		private readonly float heat_K = 0.0915f;
		private readonly float cool_K = 0.2132f;
		private readonly float epsilon = 0.005f;

		//Used for sound stuff
		private SoundEffectInstance burning;

		public override TagCompound ExtraSave(){
			//Save() is called when the world is exited, so stop the sound if it's playing
			burning?.Stop();

			return new TagCompound(){
				["heat"] = Heat,
				["furn_heat"] = Heating
			};
		}

		public override void ExtraLoad(TagCompound tag){
			Heat = tag.GetFloat("heat");
			Heating = tag.GetBool("furn_heat");
		}

		public override void PreUpdateReaction(){
			if(Heating){
				targetHeat = HeatMax;

				Vector2 center = TileUtils.TileEntityCenter(this, TileUtils.Structures.ReinforcedFurncace);

				burning = Main.PlaySound(SoundLoader.customSoundType, (int)center.X, (int)center.Y, TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/CampfireBurning"));
			}else{
				targetHeat = HeatMin;
				burning?.Stop();
			}

			ReactionInProgress = true;
		}

		public override bool UpdateReaction(){
			/*    dT/dt = k(M - T), k > 0
			 *    
			 *    where dT = change in Heat
			 *          dt = change in time
			 *          k = heat_K (M > T) or cool_K (M < T)
			 *          M = targetHeat
			 *          T = Heat
			 */
			MachineUILoader loader = TerraScience.Instance.machineLoader;
			float dt = (float)loader.lastUpdateUIGameTime.ElapsedGameTime.TotalSeconds;

			if(!Heating && Heat - targetHeat < epsilon){
				//Stop the furnace if we've cooled down to room temperature
				Heat = targetHeat;
				ReactionSpeed = 1f;
				ReactionProgress = 0f;
				return false;
			}

			ReactionSpeed *= Heating ? 1f + 0.06f / 60f : 1f - 0.235f / 60f;
			ReactionSpeed = Utils.Clamp(ReactionSpeed, 1f, 3f);

			if(Heating && AtMaxHeat){
				//If we're at max heat, then make ReactionProgress increase linearly
				ReactionProgress += ReactionSpeed * BaseReactionIncrease * dt;
				Heat = HeatMax;
			}else{
				//Actual physics
				float k = targetHeat >= Heat ? heat_K : cool_K;
				float dT = k * (targetHeat - Heat) * dt;

				if(Heating)
					ReactionProgress += ReactionSpeed * BaseReactionIncrease * Heat / HeatMax * dt;

				float cool_factor = 9.25f;
				float heat_factor = 6.35f;
				if(!Heating && dT / dt > -cool_factor){
					//Linear decrease to make it not super slow
					Heat -= cool_factor * dt;
				}else if(Heating && dT / dt < heat_factor){
					//Linear increase to make it not super slow
					Heat += heat_factor * dt;
				}else{
					//Regular heat change
					Heat += dT;
				}
			}

			return true;
		}

		public override void ReactionComplete(){
			//If the parent isn't loaded, save the changes in our own slots
			//Otherwise, modify the parent MachineUI directly
			ReinforcedFurnaceUI ui = ParentState as ReinforcedFurnaceUI;
			if(!(ui?.Active ?? false)){
				Item fuel = GetItem(0);
				Item result = GetItem(1);
				Do_Reaction(fuel, result);

			//	Main.NewText($"Edited entity slots: \"{Lang.GetItemNameValue(fuel.type)}\" ({fuel.stack}), \"{Lang.GetItemNameValue(result.type)}\" ({result.stack})");
			}else{
				UIItemSlot fuel = ui.GetSlot(0);
				UIItemSlot resultSlot = ui.GetSlot(1);

				Do_Reaction(fuel.StoredItem, resultSlot.StoredItem);

			//	Main.NewText($"Edited UI slots: \"{Lang.GetItemNameValue(fuel.StoredItem.type)}\" ({fuel.StoredItem.stack}), \"{Lang.GetItemNameValue(resultSlot.StoredItem.type)}\" ({resultSlot.StoredItem.stack})");
			}

			Vector2 center = TileUtils.TileEntityCenter(this, TileUtils.Structures.ReinforcedFurncace);

			Main.PlaySound(SoundLoader.customSoundType, center, TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/Flame Arrow"));
		}

		private void Do_Reaction(Item fuel, Item result){
			fuel.stack--;

			if(fuel.stack <= 0){
				fuel.TurnToAir();
				Heating = false;
			}

			if(result.IsAir){
				result.SetDefaults(ElementUtils.ElementType(Element.Carbon));
				result.stack = 0;
			}

			result.stack += Main.rand.Next(7, 12);

			if(result.stack > result.maxStack) {
				result.stack = result.maxStack;
				Heating = false;
			}
		}
	}
}
