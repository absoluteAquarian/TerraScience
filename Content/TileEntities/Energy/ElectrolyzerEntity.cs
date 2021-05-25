using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class ElectrolyzerEntity : PoweredMachineEntity, ILiquidMachine, IGasMachine{
		public const float MaxLiquid = 30f;
		public const float MaxGasPrimary = 100f;
		public const float MaxGasSecondary = 100f;

		public const int MaxPlaceDelay = 12;
		public int WaterPlaceDelay = 0;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override int MachineTile => ModContent.TileType<Electrolyzer>();

		public override TerraFlux FluxCap => new TerraFlux(5000f);

		public override TerraFlux FluxUsage => new TerraFlux(1000f);

		public override int SlotsCount => 5;

		public MachineLiquidID[] LiquidTypes{ get; set; }
		public float[] StoredLiquidAmounts{ get; set; }

		public MachineGasID[] GasTypes{ get; set; }
		public float[] StoredGasAmounts{ get; set; }

		public static Dictionary<MachineLiquidID, (MachineGasID, MachineGasID)> liquidToGas;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));

			this.LoadLiquids(tag, failsafeArrayLength: 1);
			this.LoadGases(tag, failsafeArrayLength: 2);
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();
			baseTag.Add(nameof(CurBatteryCharge), CurBatteryCharge);

			this.SaveLiquids(baseTag);
			this.SaveGases(baseTag);

			return baseTag;
		}

		public override bool UpdateReaction(){
			StoredLiquidAmounts[0] -= 0.3333f / 60f;

			StoredLiquidAmounts[0] = Utils.Clamp(StoredLiquidAmounts[0], 0f, MaxLiquid);

			if(StoredLiquidAmounts[0] == 0)
				LiquidTypes[0] = MachineLiquidID.None;

			//Moderate chance to produce something every tick
			if(Main.rand.NextFloat() < 0.1f / 60f)
				ReactionProgress = 100;

			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			//1000TF per operation
			if(CurBatteryCharge <= 0f || !CheckFluxRequirement(FluxUsage))
				return;

			(MachineGasID gas, MachineGasID gas2) = liquidToGas[LiquidTypes[0]];
			GasTypes[0] = gas;
			GasTypes[1] = gas2;

			StoredGasAmounts[0]++;
			StoredGasAmounts[1]++;

			this.PlaySound(SoundID.Item85, TileUtils.TileEntityCenter(this, MachineTile));
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

				if(battery.stack <= 0)
					battery.TurnToAir();

				CurBatteryCharge = BatteryMax;
			}

			Item gas1Out = this.RetrieveItem(2);
			Item gas2Out = this.RetrieveItem(4);

			bool canOutput1 = gas1Out.IsAir || GasTypes[0] == MachineGasID.None || (GasTypes[0] != MachineGasID.None && gas1Out.modItem is Capsule capsule && capsule.GasType == GasTypes[0]);
			bool canOutput2 = gas2Out.IsAir || GasTypes[1] == MachineGasID.None || (GasTypes[1] != MachineGasID.None && gas2Out.modItem is Capsule capsule2 && capsule2.GasType == GasTypes[1]);

			ReactionInProgress = CurBatteryCharge > 0
				&& StoredLiquidAmounts[0] > 0
				&& StoredGasAmounts[0] < MaxGasPrimary
				&& StoredGasAmounts[1] < MaxGasSecondary
				&& canOutput1
				&& canOutput2;
		}

		public override void PostReaction(){
			//Update the delay timer
			if(WaterPlaceDelay > 0)
				WaterPlaceDelay--;

			if(StoredLiquidAmounts[0] <= 0)
				LiquidTypes[0] = MachineLiquidID.None;

			(MachineGasID gas, MachineGasID gas2) = LiquidTypes[0] != MachineLiquidID.None ? liquidToGas[LiquidTypes[0]] : (MachineGasID.None, MachineGasID.None);

			//Check whether a capsule can be filled or not
			Item gas1Input = this.RetrieveItem(1);
			Item gas1Output = this.RetrieveItem(2);
			TileEntityUtils.UpdateOutputSlot(gas, gas1Input, gas1Output, ref StoredGasAmounts[0]);

			if(StoredGasAmounts[0] <= 0)
				GasTypes[0] = MachineGasID.None;

			Item gas2Input = this.RetrieveItem(3);
			Item gas2Output = this.RetrieveItem(4);
			TileEntityUtils.UpdateOutputSlot(gas2, gas2Input, gas2Output, ref StoredGasAmounts[1]);

			if(StoredGasAmounts[1] <= 0)
				GasTypes[1] = MachineGasID.None;
		}

		internal override int[] GetInputSlots() => new int[]{ 0, 1, 3 };

		internal override int[] GetOutputSlots() => new int[]{ 2, 4 };

		internal override bool CanInputItem(int slot, Item item)
			=> (slot == 0 && item.type == ModContent.ItemType<Battery9V>()) || ((slot == 1 || slot == 3) && item.modItem is Capsule capsule && capsule.GasType == MachineGasID.None);
	}
}
