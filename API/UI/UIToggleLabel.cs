using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace TerraScience.API.UI{
	public class UIToggleLabel : UIToggleImage{
		public UIToggleLabel(string name, bool defaultState = false) : base(ModContent.Request<Texture2D>("Terraria/UI/Settings_Toggle"), 13, 13, new Point(17, 1), new Point(1, 1)){
			Append(new UIText(name){
				Top = { Pixels = 5 },
				Left = { Pixels = 20 }
			});

			SetState(defaultState);
		}
	}
}
