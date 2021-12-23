using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.API.UI{
	public class UIScienceWorkbenchDisplay : UIElement{
		private Texture2D texture;
		private Rectangle? frame;

		public float Scale{ get; set; }

		public Color DrawColor{ get; set; }

		public UIScienceWorkbenchDisplay(string texture, Rectangle? frame){
			this.texture = ModContent.GetTexture(texture);
			this.frame = frame;
		}

		public void SetImage(string texture, Rectangle? frame){
			this.texture = ModContent.GetTexture(texture);
			this.frame = frame;

			Width.Set(frame?.Width ?? this.texture.Width, 0);
			Height.Set(frame?.Height ?? this.texture.Height, 0);
		}

		public void SetFrame(Rectangle? frame){
			this.frame = frame;

			Width.Set(frame?.Width ?? texture.Width, 0);
			Height.Set(frame?.Height ?? texture.Height, 0);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch){
			var dims = GetDimensions();
			spriteBatch.Draw(texture, dims.Position(), frame, DrawColor, 0, dims.ToRectangle().Size() / 2f, Scale, SpriteEffects.None, 0);
		}
	}
}
