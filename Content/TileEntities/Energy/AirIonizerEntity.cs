using System.Collections.Generic;
using System.IO;
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

		public override TerraFlux FluxUsage => new TerraFlux(300f);

		public static Dictionary<int, (int requireStack, int resultType, int resultStack, float energyUsage, float convertTimeSeconds)> recipes;

		public override int SlotsCount => 3;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		private float currentConvertTimeMax;
		private float currentConvertTime;
		private Item convertItem;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			CurBatteryCharge = tag.GetFloat("battery");
			currentConvertTime = tag.GetFloat("convertTime");
			currentConvertTimeMax = tag.GetFloat("convertMax");
			if(tag.GetCompound("convert") is TagCompound item)
				convertItem = ItemIO.Load(item);
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();

			baseTag.Add("battery", CurBatteryCharge);
			baseTag.Add("convertTime", currentConvertTime);
			baseTag.Add("convertMax", currentConvertTimeMax);
			baseTag.Add("convert", ItemIO.Save(convertItem));

			return baseTag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			writer.Write(CurBatteryCharge);
			writer.Write(currentConvertTime);
			writer.Write(currentConvertTimeMax);
			ItemIO.Send(convertItem, writer, writeStack: true);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			CurBatteryCharge = reader.ReadSingle();
			currentConvertTime = reader.ReadSingle();
			currentConvertTimeMax = reader.ReadSingle();
			ItemIO.Receive(convertItem, reader, readStack: true);
		}

		public override void PreUpdateReaction(){
			//Battery lasts for 30s
			if((float)StoredFlux <= 0f)
				CurBatteryCharge -= BatteryMax / 30f / 60f;
			else if(CheckFluxRequirement(FluxUsage, use: false))
				CurBatteryCharge += BatteryMax / 30f / 60f;

			CurBatteryCharge = Utils.Clamp(CurBatteryCharge, 0f, BatteryMax);

			Item battery = this.RetrieveItem(1);

			if(!battery.IsAir && CurBatteryCharge <= 0){
				battery.stack--;

				if(battery.stack <= 0){
					battery.TurnToAir();
					CurBatteryCharge = 0f;
				}else
					CurBatteryCharge = BatteryMax;
			}

			Item input = this.RetrieveItem(0);

			if(convertItem.IsAir && !input.IsAir)
				SetNextConvert();

			Item output = this.RetrieveItem(2);

			ReactionInProgress = CurBatteryCharge > 0
				&& recipes.ContainsKey(convertItem.type)
				&& convertItem.stack >= recipes[convertItem.type].requireStack
				&& (output.IsAir || recipes[convertItem.type].resultType == output.type);
		}

		public override bool UpdateReaction(){
			//Small chance to get an item each tick
			if(currentConvertTimeMax == 0)
				return false;

			currentConvertTime += 1f / 60f;

			ReactionProgress = 100 * currentConvertTime / currentConvertTimeMax;
			return ReactionProgress >= 100;
		}

		public override void ReactionComplete(){
			Item input = this.RetrieveItem(0);
			Item output = this.RetrieveItem(2);

			ReactionProgress = 0;

			(int requireStack, int resultType, int resultStack, float _, float _) = recipes[convertItem.type];

			if(output.IsAir){
				output.SetDefaults(resultType);
				output.stack = resultStack;
			}else
				output.stack += resultStack;

			input.stack -= requireStack;

			if(input.stack <= 0)
				input.TurnToAir();

			SetNextConvert();
		}

		private void SetNextConvert(){
			Item input = this.RetrieveItem(0);

			if(recipes.ContainsKey(input.type) && recipes[input.type].requireStack <= input.stack && CheckFluxRequirement(new TerraFlux(recipes[input.type].energyUsage))){
				currentConvertTimeMax = recipes[input.type].convertTimeSeconds;
				currentConvertTime = 0;

				convertItem = input.Clone();

				this.PlayCustomSound(TileUtils.TileEntityCenter(this, MachineTile), "Zap");
			}else{
				currentConvertTimeMax = 0;
				convertItem.TurnToAir();
			}
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override int[] GetOutputSlots() => new int[]{ 2 };

		internal override bool CanInputItem(int slot, Item item) => slot == 0 && recipes.ContainsKey(item.type);
	}
}
