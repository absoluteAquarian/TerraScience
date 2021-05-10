using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class ElectrolyzerEntity : PoweredMachineEntity{
		public float StoredLiquid;
		public const float MaxLiquid = 30f;

		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override int MachineTile => ModContent.TileType<Electrolyzer>();

		public override TerraFlux FluxCap => new TerraFlux(5000f);

		public override TerraFlux FluxUsage => new TerraFlux(1000f);

		public bool ForceNoReaction = false;

		public override int SlotsCount => 3;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			StoredLiquid = tag.GetFloat(nameof(StoredLiquid));
			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();
			baseTag.Add(nameof(StoredLiquid), StoredLiquid);
			baseTag.Add(nameof(CurBatteryCharge), CurBatteryCharge);
			return baseTag;
		}

		public override bool UpdateReaction(){
			StoredLiquid -= 0.3333f / 60f;

			StoredLiquid = Utils.Clamp(StoredLiquid, 0f, MaxLiquid);

			//Moderate chance to produce something every tick
			if(Main.rand.NextFloat() < 0.1f / 60f)
				ReactionProgress = 100;

			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			//1000TF per operation
			//Do nothing for now
			bool f = true;
			if(f || CurBatteryCharge <= 0f || !CheckFluxRequirement(FluxUsage))
				return;

			Item hydros = this.RetrieveItem(1);
			Item oxys = this.RetrieveItem(2);

			Main.PlaySound(SoundID.Item85, TileUtils.TileEntityCenter(this, MachineTile));

			if(hydros.IsAir){
			//	hydros.SetDefaults(ElementUtils.ElementType(Element.Hydrogen));
				hydros.stack = 0;
			}
			if(oxys.IsAir){
			//	oxys.SetDefaults(ElementUtils.ElementType(Element.Oxygen));
				oxys.stack = 0;
			}

			hydros.stack += 2;
			oxys.stack += 1;
		}

		public override void PreUpdateReaction(){
			//Battery lasts for 30s
			if((float)StoredFlux <= 0f)
				CurBatteryCharge -= BatteryMax / 30f / 60f;
			else if(CheckFluxRequirement(FluxUsage, use: false))
				CurBatteryCharge += BatteryMax / 30f / 60f;

			CurBatteryCharge = Utils.Clamp(CurBatteryCharge, 0f, BatteryMax);

			Item battery = this.RetrieveItem(0);

			if(!battery.IsAir && CurBatteryCharge <= 0){
				battery.stack--;

				if(battery.stack <= 0){
					battery.TurnToAir();
					CurBatteryCharge = 0f;
				}else
					CurBatteryCharge = BatteryMax;
			}

			ReactionInProgress = !ForceNoReaction && CurBatteryCharge > 0 && StoredLiquid > 0;
		}

		public override void PostReaction(){
			Item hydros = this.RetrieveItem(1);
			Item oxys = this.RetrieveItem(2);

			ForceNoReaction = hydros.stack >= 100 || oxys.stack >= 100;

			//Update the delay timer
			if(WaterPlaceDelay > 0)
				WaterPlaceDelay--;
		}
	}
}
