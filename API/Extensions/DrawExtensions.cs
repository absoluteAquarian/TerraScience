using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerraScience.API.Extensions {
	public static class DrawExtensions {
		public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Color color, float rotation = 0f, Rectangle? sourceRectangle = null, Vector2? origin = null, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0f) {
			spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin ?? Vector2.Zero, spriteEffects, layerDepth);
		}
	}
}
