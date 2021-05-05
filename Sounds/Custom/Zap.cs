using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Sounds.Custom{
	public class Zap : ModSound{
		public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type){
			//A bit quieter than usual
			float volumeFactor = 0.23f;

			if(soundInstance is null){
				soundInstance = sound.CreateInstance();
				soundInstance.Volume = volume * volumeFactor;
				soundInstance.Pan = pan;
				Main.PlaySoundInstance(soundInstance);
				return soundInstance;
			}

			soundInstance.Volume = volume * volumeFactor;
			soundInstance.Pan = pan;
			return soundInstance;
		}
	}
}
