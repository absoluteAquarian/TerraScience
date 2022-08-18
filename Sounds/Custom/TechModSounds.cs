using Terraria.Audio;

namespace TerraScience.Sounds.Custom {
	internal class TechModSounds {
		public static SoundStyle ZapSound;
		public static SoundStyle CampfireBurning;
		public static void Initialize() {
			ZapSound = new SoundStyle("TerraScience/Sounds/Custom/Zap.mp3") with {
				Volume = 0.23f
			};

			CampfireBurning = new SoundStyle("TerraScience/Sounds/Custom/CampfireBurning.mp3") with {
				Volume = 0.35f
			};
		}
	}
}
