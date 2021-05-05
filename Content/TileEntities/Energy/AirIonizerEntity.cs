using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class AirIonizerEntity : PoweredMachineEntity{
		public override int MachineTile => ModContent.TileType<AirIonizer>();

		public override TerraFlux FluxCap => new TerraFlux(3000f);

		public static List<int> ResultTypes;

		public static List<double> ResultWeights;

		public static List<int> ResultStacks;

		public override int SlotsCount => 11;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();
			baseTag.Add(nameof(CurBatteryCharge), CurBatteryCharge);
			return baseTag;
		}

		public override void PreUpdateReaction(){
			//Force this tile to do nothing for the time being until i can think of a use for it
			bool f = true;
			if(f)
				return;

			//Battery lasts for 30s
			if((float)StoredFlux <= 0f)
				CurBatteryCharge -= BatteryMax / 30f / 60f;
			else if(CheckFluxRequirement(new TerraFlux(300f), use: false))
				CurBatteryCharge += BatteryMax / 30f / 60f;

			CurBatteryCharge = Utils.Clamp(CurBatteryCharge, 0f, BatteryMax);

			//Always try to collect air, unless one of the slots would be full if another result stack was added to it
			for(int i = 0; i < 10; i++){
				Item item = this.RetrieveItem(i);

				if(item.IsAir)
					continue;

				for(int j = 0; j < ResultTypes.Count; j++){
					if(ResultTypes[j] == item.type && item.stack + ResultStacks[j] >= item.maxStack){
						ReactionInProgress = false;
						return;
					}
				}
			}

			Item battery = this.RetrieveItem(SlotsCount - 1);

			if(!battery.IsAir && CurBatteryCharge <= 0){
				battery.stack--;

				if(battery.stack <= 0){
					battery.TurnToAir();
					CurBatteryCharge = 0f;
				}else
					CurBatteryCharge = BatteryMax;
			}

			//If we're here, then no slots were too full.  Check if there's a battery active
			ReactionInProgress = CurBatteryCharge > 0;
		}

		public override bool UpdateReaction(){
			//Small chance to get an item each tick
			if(Main.rand.NextFloat() < 0.08f / 60f)
				ReactionProgress = 100;
			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			bool f = true;
			if(f || CurBatteryCharge <= 0f || !CheckFluxRequirement(new TerraFlux(300f)))
				return;

			//Initialize the randomness
			TerraScience.wRand.Clear();
			for(int i = 0; i < ResultTypes.Count; i++){
				TerraScience.wRand.Add((ResultTypes[i], ResultStacks[i]), ResultWeights[i]);
			}

			//Do the randomness
			var result = TerraScience.wRand.Get();

			//Parse the result
			int type = result.Item1;
			int stack = result.Item2;

			//Zappy sound
			Main.PlaySound(SoundLoader.customSoundType, TileUtils.TileEntityCenter(this, TileUtils.Structures.AirIonizer), TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/Zap"));

			//Then try and either add to an existing stack or insert it into the first available slot
			//Check for existing stacks first, then empty slots
			for(int i = 0; i < 10; i++){
				Item item = this.RetrieveItem(i);

				if(item.IsAir)
					continue;

				if(item.type == type){
					item.stack += stack;

					if(item.stack > item.maxStack)
						item.stack = item.maxStack;

					return;
				}
			}
			for(int i = 0; i < 10; i++){
				Item item = this.RetrieveItem(i);

				if(item.IsAir){
					item.SetDefaults(type);
					item.stack = stack;
					return;
				}
			}
		}
	}
}
