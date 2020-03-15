using TerraScience.API.Classes;
using TerraScience.API.Classes.ModLiquid;
using TerraScience.Systems;
using TerraScience.Systems.TemperatureSystem;

namespace TerraScience {
	public class TerraScienceLiquidLoader : ModLiquidLoader {
		public ModLiquid mercuryLiquid = null;

		public override void LoadLiquids(ModLiquidFactory liquidFactory) {
			mercuryLiquid = liquidFactory.Create("Mercury", "Mercury", new DefaultTemperature(8, 5, 4, 6));
		}
	}
}