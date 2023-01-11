using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.API.Sounds;
using SerousEnergyLib.Systems;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerraScience.Common.Systems;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Sounds;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class ReinforcedFurnaceEntity : BaseInventoryEntity, IItemOutputGeneratorMachine, ISoundEmittingMachine, IReducedNetcodeMachine, IMachineUIAutoloading<ReinforcedFurnaceEntity, ReinforcedFurnaceUI> {
		public override int MachineTile => ModContent.TileType<ReinforcedFurnace>();

		// BaseInventoryEntity usage requires overriding this, even though IMachineUIAutoloading<,> does this already
		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<ReinforcedFurnaceEntity>();

		public override int DefaultInventoryCapacity => 6;

		public override int[] GetExportSlots() => new int[] { 1, 2, 3, 4, 5 };

		public override int[] GetInputSlots() => new int[] { 0 };

		public override int[] GetInputSlotsForRecipes() => GetInputSlots();

		#region Conversion Rate Fields/Properties
		/// <summary>
		/// The current temperature of this furnace, measured in Celsius
		/// </summary>
		public double CurrentTemperature { get; private set; }

		/// <summary>
		/// Whether this entity is able to start converting its input item
		/// </summary>
		public bool ConversionAvailable {
			get {
				GetHeatTargets(out _, out _, out double requiredHeat);

				return requiredHeat >= 0 && CurrentTemperature >= requiredHeat;
			}
		}

		/// <summary>
		/// The logarithmic factor used when this machine is heating up
		/// </summary>
		public const double Heat_K = 0.0425;
		/// <summary>
		/// The logarithmic factor used when this machine is cooling down
		/// </summary>
		public const double Cool_K = 0.2631;
		public const double Epsilon = 0.005;

		public CraftingProgress Progress { get; private set; } = new CraftingProgress();

		/// <summary>
		/// Whether this entity should increase its temperature
		/// </summary>
		public bool Heating {
			get {
				GetHeatTargets(out _, out _, out double requiredHeat);

				return requiredHeat > -1;
			}
		}

		#endregion

		// Used to track when the input item's type has changed
		private int oldItem;
		private MachineRecipe activeRecipe;

		public override void Update() {
			// Do not remove.
			base.Update();

			// Warm up or cool down the furnace depending on whether it's active
			GetHeatTargets(out double minHeat, out double maxHeat, out double requiredHeat);

			Item input = Inventory[0];

			bool active = IsActive(out _);

			if (active) {
				// Warm up the furnace
				DoHeatPhysics(
					increaseHeat: true,
					k: Heat_K,
					target: maxHeat,
					deltaTime: 1 / 60d,
					linearDeltaThresholdSlope: 6.35);

				if (CurrentTemperature > maxHeat - Epsilon)
					CurrentTemperature = maxHeat;

				if (TileLoader.GetTile(Main.tile[Position.X, Position.Y].TileType) is ReinforcedFurnace furnace) {
					furnace.GetMachineDimensions(out uint width, out uint height);
					Vector2 burningSoundPos = Position.ToWorldCoordinates(width * 8, height * 8);

					// Allow singleplayer to mute the sound when the game window isn't active
					float volumeAdjustment = Main.dedServ || Main.instance.IsActive ? 1f : 0f;

					ISoundEmittingMachine.EmitSound(
						emitter: this,
						RegisteredSounds.Styles.ReinforcedFurnace.Burning.WithVolumeScale(volumeAdjustment),
						NetcodeSoundMode.SendPosition | NetcodeSoundMode.SendVolume,
						ref burning,
						ref servPlayingSound,
						burningSoundPos);
				}
			} else {
				// Cool down the furnace
				DoHeatPhysics(
					increaseHeat: false,
					k: Cool_K,
					target: minHeat,
					deltaTime: 1 / 60d,
					linearDeltaThresholdSlope: 9.25);

				if (CurrentTemperature < minHeat + Epsilon)
					CurrentTemperature = minHeat;

				ISoundEmittingMachine.StopSound(
					emitter: this,
					RegisteredSounds.IDs.ReinforcedFurnace.Burning,
					ref burning,
					ref servPlayingSound);
			}

			// Adjust the conversion rate
			ref var speed = ref Progress.SpeedFactor;

			if (active && ConversionAvailable) {
				// Increase the conversion speed
				speed *= 1 + 0.06f / 60f;
			} else {
				// Slow down the conversion speed
				speed *= 1 - 0.235f / 60f;
			}

			if (speed.Multiplicative < 1)
				speed = new StatModifier(speed.Additive, 1, speed.Flat, speed.Base);
			else if (speed.Multiplicative > 3)
				speed = new StatModifier(speed.Additive, 3, speed.Flat, speed.Base);

			// Attempt to convert items
			if (input.type != oldItem || input.IsAir || activeRecipe is null) {
				// Reset the progress
				Progress.Progress = 0;
				activeRecipe = null;
			} else if (CurrentTemperature >= requiredHeat) {
				Ticks time = TechMod.Sets.ReinforcedFurnace.ConversionDuration[input.type];

				if (Progress.Step(1f / time.ticks)) {
					// Conversion was completed
					input.stack--;

					if (input.stack <= 0)
						input.TurnToAir();

					Netcode.SyncMachineInventorySlot(this, 0);

					IItemOutputGeneratorMachine.AddRecipeOutputsToExportInventory(this, activeRecipe);
				}
			}

			Netcode.SendReducedData(this);

			oldItem = input.type;
		}

		private void DoHeatPhysics(bool increaseHeat, double k, double target, double deltaTime, double linearDeltaThresholdSlope) {
			double slope = k * (target - CurrentTemperature);
			double deltaTemperature = slope * deltaTime;

			if (!increaseHeat && slope > -linearDeltaThresholdSlope) {
				// Linear decrease so that it isn't super slow
				CurrentTemperature -= linearDeltaThresholdSlope * deltaTime;
			} else if (increaseHeat && slope < linearDeltaThresholdSlope) {
				// Linear increase so that it isn't super slow
				CurrentTemperature += linearDeltaThresholdSlope * deltaTime;
			} else {
				// Normal physics
				CurrentTemperature += deltaTemperature;
			}
		}

		/// <summary>
		/// Retrieves information about the heat targets used by this machine
		/// </summary>
		/// <param name="minheat">The lower heat threshold for when this machine is not active</param>
		/// <param name="maxHeat">THe upper heat threshold for when this machine is active</param>
		/// <param name="ingredientBurnHeat">The heat required to start converting the input ingredient to its output, or <c>-1</c> if the machine is inactive or the input item is invalid</param>
		public void GetHeatTargets(out double minheat, out double maxHeat, out double ingredientBurnHeat) {
			// TODO: biome/depth detection for minimum heat
			minheat = 20;

			maxHeat = 1000;

			ingredientBurnHeat = IsActive(out double required) ? required : -1;
		}

		/// <summary>
		/// Whether this entity is currently processing items
		/// </summary>
		/// <param name="requiredHeat">The required heat to start converting this machine's input item if it's valid, <c>-1</c> otherwise</param>
		public bool IsActive(out double requiredHeat) {
			Item input = Inventory[0];

			requiredHeat = -1;

			if (input.IsAir)
				return false;

			var slots = (this as IInventoryMachine).GetExportSlotsOrDefault();
			var inv = Inventory;

			for (int i = 0; i < slots.Length; i++) {
				int slot = slots[i];

				Item output = inv[slot];

				if (output.IsAir || output.stack < output.maxStack)
					break;
			}

			requiredHeat = TechMod.Sets.ReinforcedFurnace.MinimumHeatForConversion[input.type];
			if (requiredHeat < 0)
				return false;

			if (activeRecipe is null) {
				inv = new Item[] { input };
				activeRecipe = TechRecipes.Sets.ReinforcedFurnace.All.Where(m => m.IngredientSetMatches(this)).FirstOrDefault();
			}

			return activeRecipe is not null;
		}

		#region Implement ISoundEmittingMachine
		private SlotId burning = SlotId.Invalid;
		private bool servPlayingSound;

		public void OnSoundPlayingPacketReceived(in SlotId soundSlot, int id, int extraInformation) {
			if (id == RegisteredSounds.IDs.ReinforcedFurnace.Burning)
				burning = soundSlot;
		}

		public void OnSoundUpdatePacketReceived(int id, SoundStyle data, NetcodeSoundMode mode, Vector2? location, int extraInformation) {
			if (id == RegisteredSounds.IDs.ReinforcedFurnace.Burning && SoundEngine.TryGetActiveSound(burning, out var activeSound)) {
				if ((mode & NetcodeSoundMode.SendPosition) == NetcodeSoundMode.SendPosition)
					activeSound.Position = location;
				if ((mode & NetcodeSoundMode.SendVolume) == NetcodeSoundMode.SendVolume)
					activeSound.Volume = data.Volume;
			}
		}

		public void OnSoundStopPacketReceived(int id, int extraInformation) {
			if (id == RegisteredSounds.IDs.ReinforcedFurnace.Burning) {
				if (SoundEngine.TryGetActiveSound(burning, out var activeSound))
					activeSound.Stop();

				burning = SlotId.Invalid;
			}
		}
		#endregion

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			writer.Write(CurrentTemperature);

			var progress = Progress;
			var speed = progress.SpeedFactor;

			writer.Write(progress.Progress);
			writer.Write(speed.Additive);
			writer.Write(speed.Multiplicative);
			writer.Write(speed.Flat);
			writer.Write(speed.Base);

			writer.Write(oldItem);

			writer.Write(activeRecipe is null);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			CurrentTemperature = reader.ReadDouble();

			float progress = reader.ReadSingle();
			float speedAdditive = reader.ReadSingle();
			float speedMultiplicative = reader.ReadSingle();
			float speedFlat = reader.ReadSingle();
			float speedBase = reader.ReadSingle();

			Progress = new CraftingProgress() {
				Progress = progress,
				SpeedFactor = new StatModifier(speedAdditive, speedMultiplicative, speedFlat, speedBase)
			};

			oldItem = reader.ReadInt32();

			if (reader.ReadBoolean())
				activeRecipe = null;
		}
		#endregion
	}
}
