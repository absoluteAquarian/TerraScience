#if MERGEDTESTING
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerraScience.Sounds{
	public sealed class SoundTracker{
		private Dictionary<string, string> pathsToNames;
		private Dictionary<string, MERGEDTESTING> sounds;
		private Dictionary<string, SlotId> soundSlots;

		public void RegisterSounds(){
			const string baseSoundPath = "TerraScience/Sounds";

			pathsToNames = new Dictionary<string, string>();
			sounds = new Dictionary<string, MERGEDTESTING>();
			soundSlots = new Dictionary<string, SlotId>();

			AddSound("Campfire Burning", $"{baseSoundPath}/Custom/Campfire Burning", volume: 0.35f);
			AddSound("Zap", $"{baseSoundPath}/Custom/Zap", volume: 0.35f);
		}

		public MERGEDTESTING GetSound(string path)
			=> sounds[pathsToNames[path]];

		private void AddSound(string name, string path, float volume){
			pathsToNames.Add(path, name);
			sounds.Add(name, new MERGEDTESTING(path, volume: volume));
			soundSlots.Add(name, SlotId.Invalid);
		}

		public void CheckSound(SoundStyle type){
			//Only process our modded sounds
			if(type is not MERGEDTESTING modSound)
				return;

			if(IsSoundPlaying(modSound, "Campfire Burning", out SlotId id) || IsSoundPlaying(modSound, "Zap", out id)){
				var active = SoundEngine.GetActiveSound(id);

				//Only allow one sound to play at a time to prevent sound overload
				active.Stop();
			}
		}

		private bool IsSoundPlaying(MERGEDTESTING sound, string name, out SlotId id){
			id = SlotId.Invalid;
			return sound.SoundPath == sounds[name].SoundPath && (id = soundSlots[name]) != SlotId.Invalid;
		}

		public void SetActive(SoundStyle type, SlotId id){
			//Only process our modded sounds
			if(type is not MERGEDTESTING modSound)
				return;

			SetActive(modSound, "Campfire Burning", id);
			SetActive(modSound, "Zap", id);
		}

		private void SetActive(MERGEDTESTING sound, string name, SlotId id){
			if(sound.SoundPath == sounds[name].SoundPath)
				soundSlots[name] = id;
		}

		public void Unload(){
			pathsToNames = null;
			sounds = null;
			soundSlots = null;
		}
	}
}
#endif