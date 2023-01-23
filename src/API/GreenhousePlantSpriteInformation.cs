using Microsoft.Xna.Framework;
using SerousEnergyLib.API;

namespace TerraScience.API {
	public readonly struct GreenhousePlantSpriteInformation {
		public readonly int soil;
		public readonly int modifier;
		public readonly int plant;

		/// <summary>
		/// The plant sprite used when the growth progress is under 45%
		/// </summary>
		public readonly MachineSpriteEffectInformation immature;
		/// <summary>
		/// The plant sprite used when the growth progress is under 95%<br/>
		/// If this field is <see langword="null"/>, <see cref="immature"/> is used as a fallback
		/// </summary>
		public readonly MachineSpriteEffectInformation? mature;
		/// <summary>
		/// The plant sprite used when the growth progress is at or above 95%<br/>
		/// If this field is <see langword="null"/>, <see cref="immature"/> is used as a fallback
		/// </summary>
		public readonly MachineSpriteEffectInformation? blooming;
		/// <summary>
		/// The plant sprite used when the growth progress is at or above 95% as a glowmask<br/>
		/// If this field is <see langword="null"/>, then no glowmask is rendered
		/// </summary>
		public readonly MachineSpriteEffectInformation? glowmask;
		/// <summary>
		/// The color emitted by the machine when a plant is in its "blooming" stage<br/>
		/// If this field is <see langword="null"/>, then no light is emitted
		/// </summary>
		public readonly Color? light;

		/// <summary>
		/// Create a new structure of sprite information for a growing plant in a Greenhouse
		/// </summary>
		/// <param name="immatureSprite">The plant sprite used when the growth progress is under 45%</param>
		/// <param name="matureSprite">
		/// The plant sprite used when the growth progress is under 95%<br/>
		/// If this parameter is <see langword="null"/>, <paramref name="immatureSprite"/> is used as a fallback
		/// </param>
		/// <param name="bloomingSprite">
		/// The plant sprite used when the growth progress is at or above 95%<br/>
		/// If this parameter is <see langword="null"/>, <paramref name="immatureSprite"/> is used as a fallback
		/// </param>
		/// <param name="glowmaskSprite">
		/// The glowmask to render over the plant sprite used when the growth progress is at or above 95%<br/>
		/// If this parameter is <see langword="null"/>, then no glowmask is rendered
		/// </param>
		/// <param name="emitLight">
		/// The color emitted by the machine when a plant is in its "blooming" stage<br/>
		/// If this parameter is <see langword="null"/>, then no light is emitted
		/// </param>
		public GreenhousePlantSpriteInformation(int soil, int modifier, int plant, MachineSpriteEffectInformation immatureSprite, MachineSpriteEffectInformation? matureSprite = null, MachineSpriteEffectInformation? bloomingSprite = null, MachineSpriteEffectInformation? glowmaskSprite = null, Color? emitLight = null) {
			this.soil = soil;
			this.modifier = modifier;
			this.plant = plant;
			immature = immatureSprite;
			mature = matureSprite;
			blooming = bloomingSprite;
			glowmask = glowmaskSprite;
			light = emitLight;
		}
	}
}
