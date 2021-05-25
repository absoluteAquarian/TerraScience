using TerraScience.Content.ID;

namespace TerraScience.Content.TileEntities{
	public interface IGasMachine{
		MachineGasID[] GasTypes{ get; set; }
		float[] StoredGasAmounts{ get; set; }
	}

	public interface ILiquidMachine{
		MachineLiquidID[] LiquidTypes{ get; set; }
		float[] StoredLiquidAmounts{ get; set; }
	}
}
