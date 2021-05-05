using TerraScience.Content.TileEntities.Energy.Generators;

namespace TerraScience.Content.UI.Energy.Generators{
	public abstract class GeneratorUI : PoweredMachineUI{
		public string GetGenerationString() => $"Power: {UIDecimalFormat((float)(UIEntity as GeneratorEntity).GetPowerGeneration(60))} TF/s";
	}
}
