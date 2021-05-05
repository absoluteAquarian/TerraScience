using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerraScience.API.Extensions {
	public static class DrawExtensions {
		public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Color color, float rotation = 0f, Rectangle? sourceRectangle = null, Vector2? origin = null, SpriteEffects? spriteEffects = null, float layerDepth = 0f) {
			//'spriteEffects' must be made a Nullable in order to be an optional parameter.  Weird.
			spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin ?? Vector2.Zero, spriteEffects ?? SpriteEffects.None, layerDepth);
		}
	}
}
