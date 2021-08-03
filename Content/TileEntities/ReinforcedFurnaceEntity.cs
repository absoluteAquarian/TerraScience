using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;
using TerraScience.Content.Items.Materials;
using System.Collections.Generic;
using System.IO;

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

		public override int SlotsCount => 2;

		public static List<int> woodTypes;

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

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write(Heat);
			writer.Write(targetHeat);
			writer.Write(Heating);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			Heat = reader.ReadSingle();
			targetHeat = reader.ReadSingle();
			Heating = reader.ReadBoolean();
		}

		public override void PreUpdateReaction(){
			if(Heating){
				targetHeat = HeatMax;

				Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

				burning = this.PlayCustomSound(center, "CampfireBurning");
			}else{
				targetHeat = HeatMin;
				burning?.Stop();
			}

			//Stop the sound if the game isn't in focus
			// TODO: this doesn't work; the sound keeps playing while the game isn't in focus...
			if(!Main.hasFocus)
				burning?.Stop();

			Heating = ReactionInProgress = !this.RetrieveItem(0).IsAir;
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
			MachineUILoader loader = TechMod.Instance.machineLoader;
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
			Item input = this.RetrieveItem(0);
			Item result = this.RetrieveItem(1);

			input.stack--;

			if(input.stack <= 0){
				input.TurnToAir();
				Heating = false;
			}

			if(result.IsAir){
				result.SetDefaults(ModContent.ItemType<Coal>());
				result.stack = 0;
			}
			
			if(Main.rand.NextFloat() < 0.35f)
				result.stack += Main.rand.Next(1, 4);

			if(result.stack > result.maxStack) {
				result.stack = result.maxStack;
				Heating = false;
			}

			Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

			this.PlayCustomSound(center, "Flame Arrow");
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override int[] GetOutputSlots() => new int[]{ 1 };

		internal override bool CanInputItem(int slot, Item item)
			=> slot == 0 && ReinforcedFurnaceUI.ValidItem(item);
	}
}
