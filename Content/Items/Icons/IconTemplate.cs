using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Icons{
	public class IconTemplate : ModItem{
		public override bool Autoload(ref string name) => false;

		public IconTemplate(){ }

		private readonly string machineName;

		public IconTemplate(string machine){
			machineName = string.Concat(machine.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(machineName);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
