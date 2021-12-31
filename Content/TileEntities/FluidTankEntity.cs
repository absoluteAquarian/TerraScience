using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class FluidTankEntity : MachineEntity, IFluidMachine{
		public override int MachineTile => ModContent.TileType<FluidTank>();

		public override int SlotsCount => 0;

		//Duplicate entries are needed for proper I/O usage
		const float DefaultMax = 2000f;
		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			//Left side
			new FluidEntry(max: DefaultMax, isInput: true, null),
			new FluidEntry(max: DefaultMax, isInput: false, null),
			//Right side
			new FluidEntry(max: DefaultMax, isInput: true, null),
			new FluidEntry(max: DefaultMax, isInput: false, null)
		};

		public int FluidPlaceDelay{ get; set; }

		public override TagCompound ExtraSave(){
			TagCompound tag = new TagCompound();

			this.SaveFluids(tag);

			return tag;
		}

		public override void ExtraLoad(TagCompound tag){
			this.LoadFluids(tag);
		}

		public override void ExtraNetSend(BinaryWriter writer){
			this.SendFluids(writer);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			this.ReceiveFluids(reader);
		}

		public override void ReactionComplete(){ }

		public void TryImportFluid(Point16 pipePos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pipePos.X + 0.5f > x + 0.01f){
				this.TryImportFluids(pipePos, 2);

				FluidEntries[3].id = FluidEntries[2].id;
				FluidEntries[3].current = FluidEntries[2].current;
				FluidEntries[3].max = FluidEntries[2].max;
			}else if(pipePos.X + 0.5f < x - 0.01f){
				this.TryImportFluids(pipePos, 0);

				FluidEntries[1].id = FluidEntries[0].id;
				FluidEntries[1].current = FluidEntries[0].current;
				FluidEntries[1].max = FluidEntries[0].max;
			}
		}

		public void TryExportFluid(Point16 pumpPos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pumpPos.X + 0.5f > x + 0.01f){
				this.TryExportFluids(pumpPos, 3);

				FluidEntries[2].id = FluidEntries[3].id;
				FluidEntries[2].current = FluidEntries[3].current;
				FluidEntries[2].max = FluidEntries[3].max;
			}else if(pumpPos.X + 0.5f < x - 0.01f){
				this.TryExportFluids(pumpPos, 1);

				FluidEntries[0].id = FluidEntries[1].id;
				FluidEntries[0].current = FluidEntries[1].current;
				FluidEntries[0].max = FluidEntries[1].max;
			}
		}

		public override void PreUpdateReaction(){
			if(FluidPlaceDelay > 0)
				FluidPlaceDelay--;
		}

		public override bool UpdateReaction() => false;

		internal override bool CanInputItem(int slot, Item item) => false;

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];
	}
}
