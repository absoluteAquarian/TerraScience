using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using Terraria.Audio;

namespace TerraScience.API.Edits.Detours{
	public static partial class Vanilla{
#if MERGEDTESTING
		private static SoundEffectInstance SoundEngine_PlaySound_LegacySoundStyle_int_int(On.Terraria.Audio.SoundEngine.orig_PlaySound_LegacySoundStyle_int_int orig, LegacySoundStyle type, int x, int y){
			TechMod.Instance.soundTracker.CheckSound(type);

			return orig(type, x, y);
		}

		private static SlotId SoundEngine_PlayTrackedSound_SoundStyle_Vector2(On.Terraria.Audio.SoundEngine.orig_PlayTrackedSound_SoundStyle_Vector2 orig, SoundStyle style, Vector2 position){
			TechMod.Instance.soundTracker.CheckSound(style);

			var slot = orig(style, position);

			TechMod.Instance.soundTracker.SetActive(style, slot);
			return slot;
		}

		private static SlotId SoundEngine_PlayTrackedSound_SoundStyle(On.Terraria.Audio.SoundEngine.orig_PlayTrackedSound_SoundStyle orig, SoundStyle style){
			TechMod.Instance.soundTracker.CheckSound(style);

			var slot = orig(style);

			TechMod.Instance.soundTracker.SetActive(style, slot);
			return slot;
		}
#endif
	}
}
