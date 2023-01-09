using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public class MachineWorkbenchDisplay : UIElement, IColorable {
		private Asset<Texture2D> Texture;
		private Rectangle frame;

		public float Scale { get; set; } = 1f;
		public Color Color { get; set; } = Color.White;

		public MachineWorkbenchDisplay(string asset, Rectangle frame) {
			Texture = ModContent.Request<Texture2D>(asset);
			this.frame = frame;

			Width.Set(frame.Width, 0);
			Height.Set(frame.Height, 0);
		}

		public MachineWorkbenchDisplay(Asset<Texture2D> asset, Rectangle frame) {
			Texture = asset;
			this.frame = frame;

			Width.Set(frame.Width, 0);
			Height.Set(frame.Height, 0);
		}

		public void SetImage(Asset<Texture2D> texture, Rectangle frame) {
			Texture = texture;
			this.frame = frame;
			Width.Set(frame.Width, 0f);
			Height.Set(frame.Height, 0f);
		}

		public void SetFrame(Rectangle frame) {
			this.frame = frame;
		}

		public void SetFrame(int columnCount = 1, int rowCount = 1, int frameX = 0, int frameY = 0, int sizeOffsetX = 0, int sizeOffsetY = 0) {
			// Ensure that the asset is loaded before trying to use it to get a source frame
			if (!Texture.IsLoaded)
				Texture.Wait?.Invoke();

			SetFrame(Texture.Frame(columnCount, rowCount, frameX, frameY, sizeOffsetX, sizeOffsetY));
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			var dims = GetDimensions();
			Vector2 pos = dims.Position();
			pos.X = (int)pos.X;
			pos.Y = (int)pos.Y;

			spriteBatch.Draw(Texture.Value, dims.Position(), frame, Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
		}
	}
}
