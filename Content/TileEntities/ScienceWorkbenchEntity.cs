using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.TileEntities{
	public class ScienceWorkbenchEntity : MachineEntity{
		public override bool RequiresUI => true;

		public override int MachineTile => ModContent.TileType<ScienceWorkbench>();

		public override int SlotsCount => 1;

		public override bool UpdateReaction() => false;

		public override void ReactionComplete(){ }

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];

		internal override bool CanInputItem(int slot, Item item) => false;
	}
}
