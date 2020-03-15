using System.Text.RegularExpressions;
using Terraria;
using TerraScience.Systems;

namespace TerraScience.API.Classes
{
	public abstract class ModLiquid
	{
		public string InternalName { get; private set; } = string.Empty;
 		public virtual string DisplayName { get; set; } = string.Empty;

		public abstract float? DefaultTemperature(Season currentSeason);

		public ModLiquid() {
			InternalName = GetType().ToString();
			DisplayName = ToString();
		}

		public float CurrentTemperature() => TemperatureSystem.CalculateLiquidTemp(this);

		public virtual void InLiquid(Player player) { }

		public virtual void OnLiquidCollide() { }

		public override string ToString() => Regex.Replace(GetType().ToString(), "([A-Z])", " $1", RegexOptions.Compiled).Trim();
	}

	public class TestLiquid : ModLiquid {
		public override string DisplayName => "Testiest of Test Liquids";

		public override void OnLiquidCollide() {
			base.OnLiquidCollide();
		}

		public override float? DefaultTemperature(Season currentSeason) {
			if (currentSeason == Season.Spring)
				return 3f;
			else if (currentSeason == Season.Summer)
				return 4f;
			else if (currentSeason == Season.Autumn)
				return 2f;
			else if (currentSeason == Season.Winter)
				return 1f;
			else
				return null;
		}
	}
}