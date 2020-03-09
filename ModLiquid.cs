using System.Text.RegularExpressions;
using Terraria;

namespace TerraScience
{
	public class ModLiquid
	{
		public virtual string DisplayName { get; set; } = string.Empty;

		public ModLiquid() {
			DisplayName = ToString();
		}

		public virtual void InLiquid(Player player) { }

		public virtual void OnLiquidCollide() { }

		public override string ToString() {
			return Regex.Replace(GetType().ToString(), "([A-Z])", " $1", RegexOptions.Compiled).Trim();
		}
	}

	public class TestLiquid : ModLiquid {
		public override void OnLiquidCollide() {
			base.OnLiquidCollide();
		}
	}
}