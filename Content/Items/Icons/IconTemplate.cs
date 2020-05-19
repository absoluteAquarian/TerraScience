using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Icons{
	public class IconTemplate : ModItem{
		public override bool Autoload(ref string name) => false;

		public IconTemplate(){ }

		public readonly string MachineName;

		public IconTemplate(string machine){
			MachineName = string.Concat(machine.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault(MachineName);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
