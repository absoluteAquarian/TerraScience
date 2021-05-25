using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.Content.UI;

namespace TerraScience.API.UI{
	public class UIMachineGauge : UIElement{
		public float fluidMax;
		public float fluidCur;

		public string fluidName;

		public Color fluidColor;

		public override void OnInitialize(){
			Width.Set(32, 0);
			Height.Set(210, 0);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch){
			//Draw the back first
			Color backColor = new Color(){ PackedValue = 0xff4c5452 };

			var texture = ModContent.GetTexture("TerraScience/Content/UI/fluidgauge back");
			var dims = GetInnerDimensions();

			spriteBatch.Draw(texture, dims.Position(), null, backColor);

			int visibleHeight = texture.Height - 12;
			float height = visibleHeight - visibleHeight * fluidCur / fluidMax;
			Rectangle source = new Rectangle(0, 6 + (int)height, texture.Width, texture.Height - (int)height - 6);

			if(fluidColor != Color.Transparent)
				spriteBatch.Draw(texture, dims.Position() + new Vector2(0, 6 + height), source, fluidColor);

			var textureFrame = ModContent.GetTexture("TerraScience/Content/UI/fluidgauge border");

			spriteBatch.Draw(textureFrame, dims.Position(), null, Color.White);

			//Why does this have to be in Draw and not Update?  The world will never know...
			if(ContainsPoint(Main.MouseScreen) && fluidName != null)
				Main.hoverItemName = $"{fluidName}: {MachineUI.UIDecimalFormat(fluidCur)} / {MachineUI.UIDecimalFormat(fluidMax)} L";
		}
	}
}
