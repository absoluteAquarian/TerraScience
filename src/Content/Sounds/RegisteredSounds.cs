using SerousEnergyLib.API.Sounds;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerraScience.Content.Sounds {
	public class RegisteredSounds : ModSystem {
		public override void Load() {
			Styles.ReinforcedFurnace.Burning = new SoundStyle("TerraScience/Assets/Sounds/FX/CampfireBurning") with {
				Volume = 0.08f,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				IsLooped = true
			};
			IDs.ReinforcedFurnace.Burning = MachineSounds.RegisterSound(Styles.ReinforcedFurnace.Burning);

			Styles.ReinforcedFurnace.Output = new SoundStyle("TerraScience/Assets/Sounds/FX/Flame Arrow") with {
				Volume = 0.10f,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
			};
			IDs.ReinforcedFurnace.Output = MachineSounds.RegisterSound(Styles.ReinforcedFurnace.Output);

			Styles.FurnaceGenerator.Running = new SoundStyle("TerraScience/Assets/Sounds/FX/hum") with {
				Volume = 0.06f,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				IsLooped = true
			};
			IDs.FurnaceGenerator.Running = MachineSounds.RegisterSound(Styles.FurnaceGenerator.Running);
		}

		public static class IDs {
			public static class ReinforcedFurnace {
				public static int Burning { get; internal set; }

				public static int Output { get; internal set; }
			}

			public static class FurnaceGenerator {
				public static int Running { get; internal set; }
			}
		}

		public static class Styles {
			public static class ReinforcedFurnace {
				public static SoundStyle Burning { get; internal set; }

				public static SoundStyle Output { get; internal set; }
			}

			public static class FurnaceGenerator {
				public static SoundStyle Running { get; internal set; }
			}
		}
	}
}
