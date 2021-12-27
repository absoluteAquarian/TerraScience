using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
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

		private string boundNet;
		internal string BoundNetwork{
			get => boundNet;
			set{
				if(boundNet != value)
					boundNet = value != null && TesseractNetwork.TryGetEntry(value, out _) ? value : null;
			}
		}

		public override TagCompound ExtraSave()
			=> new TagCompound(){
				["boundNetwork"] = boundNet
			};

		public override void ExtraLoad(TagCompound tag){
			boundNet = tag.GetString("boundNetwork");
		}

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write(boundNet);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			BoundNetwork = reader.ReadString();
		}

		public override void ReactionComplete(){ }

		public void TryExportFluid(Point16 pumpPos){
			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry))
				return;
			
			this.TryExportFluids(pumpPos, 3);

			entry.fluids[1].id = FluidEntries[2].id = FluidEntries[3].id;
			entry.fluids[1].current = FluidEntries[2].current = FluidEntries[3].current;
			entry.fluids[1].max = FluidEntries[2].max = FluidEntries[3].max;

			this.TryExportFluids(pumpPos, 1);

			entry.fluids[0].id = FluidEntries[0].id = FluidEntries[1].id;
			entry.fluids[0].current = FluidEntries[0].current = FluidEntries[1].current;
			entry.fluids[0].max = FluidEntries[0].max = FluidEntries[1].max;
		}

		public void TryImportFluid(Point16 pipePos){
			if(!TesseractNetwork.TryGetEntry(boundNet, out var entry))
				return;

			this.TryImportFluids(pipePos, 2);

			entry.fluids[1].id = FluidEntries[3].id = FluidEntries[2].id;
			entry.fluids[1].current = FluidEntries[3].current = FluidEntries[2].current;
			entry.fluids[1].max = FluidEntries[3].max = FluidEntries[2].max;

			this.TryImportFluids(pipePos, 0);

			entry.fluids[0].id = FluidEntries[1].id = FluidEntries[0].id;
			entry.fluids[0].current = FluidEntries[1].current = FluidEntries[0].current;
			entry.fluids[0].max = FluidEntries[1].max = FluidEntries[0].max;
		}

		public override bool UpdateReaction()
			=> false;

		internal override bool CanInputItem(int slot, Item item)
			=> TesseractNetwork.TryGetEntry(boundNet, out _);

		internal override int[] GetInputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };

		internal override int[] GetOutputSlots()
			=> new int[]{ 0, 1, 2, 3, 4 };
	}
}
