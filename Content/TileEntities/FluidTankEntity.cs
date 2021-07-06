using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class FluidTankEntity : MachineEntity, ILiquidMachine, IGasMachine{
		public override int MachineTile => ModContent.TileType<FluidTank>();

		public override int SlotsCount => 0;

		//Duplicate entries are needed for proper I/O usage
		const float DefaultMax = 2000f;
		public LiquidEntry[] LiquidEntries{ get; set; } = new LiquidEntry[]{
			new LiquidEntry(max: DefaultMax, isInput: true, null),
			new LiquidEntry(max: DefaultMax, isInput: false, null)
		};

		public GasEntry[] GasEntries{ get; set; } = new GasEntry[]{
			new GasEntry(max: DefaultMax, isInput: true, null),
			new GasEntry(max: DefaultMax, isInput: false, null)
		};
		public int LiquidPlaceDelay{ get; set; }
		public int GasPlaceDelay{ get; set; }

		public override TagCompound ExtraSave(){
			TagCompound tag = new TagCompound();

			this.SaveLiquids(tag);
			this.SaveGases(tag);

			return tag;
		}

		public override void ExtraLoad(TagCompound tag){
			this.LoadLiquids(tag);
			this.LoadGases(tag);
		}

		public override void ReactionComplete(){ }

		public void TryExportGas(Point16 pumpPos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pumpPos.X + 0.5f > x + 0.01f){
				this.TryImportGases(pumpPos, 1);

				GasEntries[0].id = GasEntries[1].id;
				GasEntries[0].current = GasEntries[1].current;
				GasEntries[0].max = GasEntries[1].max;
			}
		}

		public void TryExportLiquid(Point16 pumpPos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pumpPos.X + 0.5f < x - 0.01f){
				this.TryExportLiquids(pumpPos, 1);

				LiquidEntries[0].id = LiquidEntries[1].id;
				LiquidEntries[0].current = LiquidEntries[1].current;
				LiquidEntries[0].max = LiquidEntries[1].max;
			}
		}

		public void TryImportGas(Point16 pipePos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pipePos.X + 0.5f > x + 0.01f){
				this.TryImportGases(pipePos, 0);

				GasEntries[1].id = GasEntries[0].id;
				GasEntries[1].current = GasEntries[0].current;
				GasEntries[1].max = GasEntries[0].max;
			}
		}

		public void TryImportLiquid(Point16 pipePos){
			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			float x = Position.X + width / 2f;

			if(pipePos.X + 0.5f < x - 0.01f){
				this.TryImportLiquids(pipePos, 0);

				LiquidEntries[1].id = LiquidEntries[0].id;
				LiquidEntries[1].current = LiquidEntries[0].current;
				LiquidEntries[1].max = LiquidEntries[0].max;
			}
		}

		public override void PreUpdateReaction(){
			if(LiquidPlaceDelay > 0)
				LiquidPlaceDelay--;

			if(GasPlaceDelay > 0)
				GasPlaceDelay--;
		}

		public override bool UpdateReaction() => false;

		internal override bool CanInputItem(int slot, Item item) => false;

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];
	}
}
