using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace TerraScience{
	public class TechModConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static TechModConfig Instance => ModContent.GetInstance<TechModConfig>();

		[Label("Animate pump tiles")]
		[DefaultValue(true)]
		public bool AnimatePumps{ get; set; }
	}
}
