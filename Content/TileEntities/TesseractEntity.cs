using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class TesseractEntity : Battery, IFluidMachine{
		public override int MachineTile => ModContent.TileType<Tesseract>();

		//5 slots for items, 4 slots each for adding/removing liquids and gases
		public override int SlotsCount => 5 + 4 * 2;

		//Duplicate entries are needed for proper I/O usage
		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			//Liquid
			new FluidEntry(max: 500f, isInput: true, null),
			new FluidEntry(max: 500f, isInput: false, null),
			//Gas
			new FluidEntry(max: 500f, isInput: true, null),
			new FluidEntry(max: 500f, isInput: false, null)
		};

		public int FluidPlaceDelay{ get; set; }

		public override TerraFlux ImportRate => new TerraFlux(500f / 60f);

		public override TerraFlux ExportRate => new TerraFlux(500f / 60f);

		public override TerraFlux FluxCap => new TerraFlux(8000f);

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

		public void TryExportFluid(Point16 pumpPos){
			this.TryImportFluids(pumpPos, 3);

			FluidEntries[2].id = FluidEntries[3].id;
			FluidEntries[2].current = FluidEntries[3].current;
			FluidEntries[2].max = FluidEntries[3].max;

			this.TryExportFluids(pumpPos, 1);

			FluidEntries[0].id = FluidEntries[1].id;
			FluidEntries[0].current = FluidEntries[1].current;
			FluidEntries[0].max = FluidEntries[1].max;
		}

		public void TryImportFluid(Point16 pipePos){
			this.TryImportFluids(pipePos, 2);

			FluidEntries[3].id = FluidEntries[2].id;
			FluidEntries[3].current = FluidEntries[2].current;
			FluidEntries[3].max = FluidEntries[2].max;

			this.TryImportFluids(pipePos, 0);

			FluidEntries[1].id = FluidEntries[0].id;
			FluidEntries[1].current = FluidEntries[0].current;
			FluidEntries[1].max = FluidEntries[0].max;
		}

		public override bool UpdateReaction()
			=> false;

		internal override bool CanInputItem(int slot, Item item)
			=> true;

		internal override int[] GetInputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };

		internal override int[] GetOutputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };
	}
}
