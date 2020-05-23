using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class ElectrolyzerEntity : MachineEntity{
		public float StoredLiquid;
		public const float MaxLiquid = 30f;

		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override int MachineTile => ModContent.TileType<Electrolyzer>();

		public bool ForceNoReaction = false;

		public override int SlotsCount => 3;

		public override void ExtraLoad(TagCompound tag){
			StoredLiquid = tag.GetFloat(nameof(StoredLiquid));
			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));
		}

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				[nameof(StoredLiquid)] = StoredLiquid,
				[nameof(CurBatteryCharge)] = CurBatteryCharge
			};

		public override bool UpdateReaction(){
			//Battery lasts for 30s
			CurBatteryCharge -= BatteryMax / 30f / 60f;

			StoredLiquid -= 0.3333f / 60f;

			//Moderate chance to produce something every tick
			if(Main.rand.NextFloat() < 0.375f / 60f)
				ReactionProgress = 100;

			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			Item hydros = this.RetrieveItem(1);
			Item oxys = this.RetrieveItem(2);

			Main.PlaySound(SoundID.Item85, TileUtils.TileEntityCenter(this, TileUtils.tileToStructure[MachineTile]));

			if(hydros.IsAir){
				hydros.SetDefaults(ElementUtils.ElementType(Element.Hydrogen));
				hydros.stack = 0;
			}
			if(oxys.IsAir){
				oxys.SetDefaults(ElementUtils.ElementType(Element.Oxygen));
				oxys.stack = 0;
			}

			hydros.stack += 4;
			oxys.stack += 2;
		}

		public override void PreUpdateReaction(){
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
