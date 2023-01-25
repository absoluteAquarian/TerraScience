using Microsoft.Xna.Framework;
using SerousCommonLib.API.Helpers;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.Systems;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class WindTurbineEntity : BasePowerGeneratorEntity, IReducedNetcodeMachine, IMachineUIAutoloading<WindTurbineEntity, WindTurbineUI> {
		public override int MachineTile => ModContent.TileType<WindTurbine>();

		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<WindTurbineEntity>();

		public override FluxStorage PowerStorage { get; } = new FluxStorage(new TerraFlux(4500d));

		public bool raining, storming, sandstorm, blizzard;
		private float windFlow;

		private int refreshTime;

		public float bladeRadians;
		public float bladeMomentum;

		public override double GetPowerGeneration(double ticks) {
			// Too lazy to copy the notes here.  Most of them are outdated anyway
			// https://github.com/absoluteAquarian/TerraScience/blob/6c417fb21be5f07849bf9fb13947e1e65a41e0b7/Content/TileEntities/Energy/Generators/BasicWindTurbineEntity.cs#L58

			// Dummy instance?  Use the player's center for the calculation and do it every tick
			if (IsDummyInstance)
				RefreshTileScanning(Main.LocalPlayer.Center);

			if (windFlow <= 0) {
				// Not possible to generate any power
				return 0d;
			}

			float bladeStrength = IsDummyInstance ? windFlow : bladeMomentum;

			float realWindStrength = Math.Abs(Main.windSpeedCurrent) * 50f * bladeStrength;

			// Default rate of 1 TF per tick, multiplied by a factor derived from the wind strength
			double perTick = 1d * realWindStrength / 14d;

			StatModifier boost = GetBoostModifier();

			return boost.ApplyTo(perTick) * ticks;
		}

		public StatModifier GetBoostModifier() {
			StatModifier boost = StatModifier.Default;

			if (raining) {
				// +8% increase, +0.1 flat
				boost += 0.08f;
				boost.Flat += 0.1f;
			}

			if (storming) {
				// +15% increase, +0.25 flat
				boost += 0.15f;
				boost.Flat += 0.25f;
			}

			if (sandstorm) {
				// +28% increase, +0.35 flat
				boost += 0.28f;
				boost.Flat += 0.35f;
			}

			if (blizzard) {
				// +20% increase, +0.3 flat
				boost += 0.2f;
				boost.Flat += 0.3f;
			}

			return boost;
		}

		public override void GeneratorUpdate() {
			// Only check for special weather once every second
			if (--refreshTime <= 0) {
				refreshTime = 60;

				// Lazily hardcoding the center as the center of the blades
				RefreshTileScanning(worldCenter: Position.ToWorldCoordinates());
			}

			const int momentum = 160;

			if (windFlow > 0) {
				bladeMomentum += 1f / momentum;

				if (bladeMomentum > windFlow)
					bladeMomentum = windFlow;
			} else {
				bladeMomentum -= 1f / momentum;

				if (bladeMomentum < windFlow)
					bladeMomentum = windFlow;
			}

			if (bladeMomentum > 0) {
				// Blades spin faster depending on the wind speed
				bladeRadians += Main.windSpeedCurrent * 50f / 28f * MathHelper.ToRadians(6) * bladeMomentum / 3f;
				bladeRadians = MathHelper.WrapAngle(bladeRadians);
			}

			Netcode.SendReducedData(this);
		}

		private void RefreshTileScanning(Vector2 worldCenter) {
			raining = storming = sandstorm = blizzard = false;

			bool rainingPotential = Main.IsItRaining;
			bool stormOrBlizzardPotential = Main.IsItStorming;
			bool sandstormPotential = Sandstorm.Happening;

			// Only permit boosts based on the biome the turbine is located inside of
			if (rainingPotential || stormOrBlizzardPotential || sandstormPotential) {
				var metrics = TileScanning.Scan(worldCenter);

				if (metrics is not null) {
					if (rainingPotential && !metrics.EnoughTilesForDesert && !metrics.EnoughTilesForSnow)
						raining = true;

					if (stormOrBlizzardPotential && !metrics.EnoughTilesForDesert) {
						if (metrics.EnoughTilesForSnow)
							blizzard = true;
						else {
							raining = false;
							storming = true;
						}
					}

					if (sandstormPotential && metrics.EnoughTilesForDesert)
						sandstorm = true;
				}
			}

			// Check for wind flow
			// Meaning, there should be enough "empty space" near the blades to have sensible wind flow
			int origX = (int)(worldCenter.X / 16), origY = (int)(worldCenter.Y / 16);
			bool anySolidTile = false;
			for (int x = -40; x <= 40; x++) {
				int checkX = origX + x;

				for (int y = -1; y <= 1; y++) {
					int checkY = origY + y;

					if (WorldGen.InWorld(checkX, checkY) && WorldGen.SolidOrSlopedTile(checkX, checkY)) {
						anySolidTile = true;
						goto checkWindFlow;
					}
				}
			}

			checkWindFlow:

			if (anySolidTile) {
				// No wind flow
				windFlow = 0;
			} else {
				// Wind flow depends on world height
				double surface = Main.worldSurface * 16;

				const double spaceBuffer = 10 * 16;

				if (worldCenter.Y < surface * 0.35 - spaceBuffer) {
					// No wind in space
					windFlow = 0;
				} else if (worldCenter.Y < surface * 0.35) {
					// Sharp decrease in wind speed at the boundary to space
					double relative = worldCenter.Y - (surface * 0.35 - spaceBuffer);
					double span = spaceBuffer;

					double factor = 1.8d * relative / span;

					windFlow = (float)factor;
				} else if (worldCenter.Y < surface) {
					// Near space = more power
					double relative = worldCenter.Y - surface * 0.35;
					double span = surface * 0.65;

					double factor = 1d + 0.8d * (1d - relative / span);

					windFlow = (float)factor;
				} else {
					// No wind below the surface
					windFlow = 0;
				}
			}
		}

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);

			tag["flags"] = (byte)new BitsByte(raining, storming, sandstorm, blizzard);

			if (refreshTime < 0)
				refreshTime = 0;

			tag["refresh"] = (byte)refreshTime;

			tag["radians"] = bladeRadians;
			tag["momentum"] = bladeMomentum;

			tag["flow"] = windFlow;
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			
			BitsByte bb = tag.GetByte("flags");
			bb.Retrieve(ref raining, ref storming, ref sandstorm, ref blizzard);

			refreshTime = tag.GetByte("refresh");

			bladeRadians = tag.GetFloat("radians");
			bladeMomentum = tag.GetFloat("momentum");

			windFlow = tag.GetFloat("flow");
		}

		public override void NetSend(BinaryWriter writer) {
			base.NetSend(writer);
			ReducedNetSend(writer);
		}

		public override void NetReceive(BinaryReader reader) {
			base.NetReceive(reader);
			ReducedNetReceive(reader);
		}

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			BitsByte bb = new(raining, storming, sandstorm, blizzard);
			writer.Write(bb);

			writer.Write((sbyte)refreshTime);

			writer.Write(bladeRadians);
			writer.Write(bladeMomentum);
			writer.Write(windFlow);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			BitsByte bb = reader.ReadByte();
			bb.Retrieve(ref raining, ref storming, ref sandstorm, ref blizzard);

			refreshTime = reader.ReadSByte();

			bladeRadians = reader.ReadSingle();
			bladeMomentum = reader.ReadSingle();
			windFlow = reader.ReadSingle();
		}
		#endregion
	}
}
