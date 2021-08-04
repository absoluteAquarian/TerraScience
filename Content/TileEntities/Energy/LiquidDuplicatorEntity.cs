using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class LiquidDuplicatorEntity : PoweredMachineEntity, ILiquidMachine{
		//Usage depends on what liquid is being duplicated
		public override TerraFlux FluxUsage => new TerraFlux(0f);

		public override TerraFlux FluxCap => new TerraFlux(2000f);

		public override int MachineTile => ModContent.TileType<LiquidDuplicator>();

		public override int SlotsCount => 1;

		public LiquidEntry[] LiquidEntries{ get; set; } = new LiquidEntry[]{
			new LiquidEntry(max: 10f, isInput: false, null)
		};
		public int LiquidPlaceDelay{ get; set; }

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			this.LoadLiquids(tag);
		}

		public override TagCompound ExtraSave(){
			var tag = base.ExtraSave();

			this.SaveLiquids(tag);

			return tag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			this.SendLiquids(writer);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			this.ReceiveLiquids(reader);
		}

		public override void PreUpdateReaction(){
			Item input = this.RetrieveItem(0);

			if((ParentState?.Active ?? false) && ParentState.GetSlot(0).ItemTypeChanged)
				ReactionProgress = 0;

			var entry = LiquidEntries[0];

			ReactionInProgress = !input.IsAir
				&& (entry.id == MachineLiquidID.None || entry.id == MiscUtils.GetIDFromItem(input.type))
				&& entry.current + 1 <= entry.max;
		}

		public override bool UpdateReaction(){
			Item input = this.RetrieveItem(0);

			float flux;
			float time;
			switch(MiscUtils.GetIDFromItem(input.type)){
				case MachineLiquidID.Water:
					flux = 100f;
					time = 5f;
					break;
				case MachineLiquidID.Saltwater:
					flux = 150f;
					time = 5f;
					break;
				case MachineLiquidID.Lava:
					flux = 300f;
					time = 10f;
					break;
				case MachineLiquidID.Honey:
					flux = 200f;
					time = 15f;
					break;
				default:
					throw new Exception("TerraScience internal error -- unknown liquid ID");
			}

			if(!CheckFluxRequirement(new TerraFlux(flux / 60f), use: true))
				return false;

			ReactionProgress += time / 60f;

			return ReactionProgress >= 100f;
		}

		public override void ReactionComplete(){
			ReactionProgress = 0f;

			var entry = LiquidEntries[0];

			MachineLiquidID id = MiscUtils.GetIDFromItem(this.RetrieveItem(0).type);

			if(entry.id == MachineLiquidID.None)
				entry.id = id;

			entry.current++;

			PlaySound(SoundID.Splash, TileUtils.TileEntityCenter(this, MachineTile));
		}

		internal override bool CanInputItem(int slot, Item item) => false;

		internal override int[] GetInputSlots() => Array.Empty<int>();

		internal override int[] GetOutputSlots() => Array.Empty<int>();

		public void TryExportLiquid(Point16 pumpPos)
			=> this.TryExportLiquids(pumpPos, 0);

		public void TryImportLiquid(Point16 pipePos){ }
	}
}
