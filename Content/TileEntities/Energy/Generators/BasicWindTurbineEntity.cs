using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Generators{
	public class BasicWindTurbineEntity : GeneratorEntity{
		public override int MachineTile => ModContent.TileType<BasicWindTurbine>();

		public override TerraFlux FluxCap => new TerraFlux(3000);

		//max export is rougly 6TF/t
		public override TerraFlux ExportRate => new TerraFlux(6);

		public float bladeRotation;

		public bool rainBoost;
		public bool sandstormBoost;

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			BitsByte bb = new BitsByte(rainBoost, sandstormBoost);
			writer.Write(bb);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			BitsByte bb = reader.ReadByte();
			bb.Retrieve(ref rainBoost, ref sandstormBoost);
		}

		public override void ReactionComplete(){
			TerraFlux flux = GetPowerGeneration(ticks: 1);

			ImportFlux(ref flux);
		}

		public override bool UpdateReaction() => true;

		public override void PreUpdateReaction(){
			//Always adding energy
			ReactionProgress = 100;
			ReactionInProgress = true;

			bladeRotation += MathHelper.ToRadians(Main.windSpeedCurrent * 6f);

			rainBoost = Main.raining;
			sandstormBoost = Sandstorm.Happening;
		}

		public override TerraFlux GetPowerGeneration(int ticks){
			/*  Notes about wind speed:
			 *  
			 *  Main.windSpeed        | The current wind speed.  Always tries to increment/decrement towards Main.windSpeedSet by a
			 *                          factor of `0.001f * Main.dayRate` per tick
			 *  
			 *  Main.windSpeedSet     | The target wind speed.  Set to Main.windSpeedTemp when Main.weatherCounter is <= 0.  Value is
			 *                          set to `genRand.Next(-100, 101) * 0.01f` on world load/generation.
			 *  
			 *  Main.windSpeedSpeed   | The rate at which Main.windSpeedTemp is changed.  Starts at 0, then is incremented by
			 *                          `rand.Next(-10, 11) * 0.0001f` every tick.  Value is clamped to +/- 0.002f
			 *  
			 *  Main.windSpeedTemp    | The next value that Main.windSpeedSet will be set to.  Modified by Main.windSpeedSpeed.
			 *                          If it's currently raining, then this variable is modified by Main.windSpeedSpeed * 2 instead.
			 *                          Value is clamped to +/- `(0.3f + 0.5f * Main.cloudAlpha)`
			 *  
			 *  Main.weatherCounter   | The timer used for modifying Main.windSpeedSet and also sending net messages for syncing
			 *                          world data.  It is decremented by Main.dayRate every tick.  Value is initialized to
			 *                          `rand.Next(3600, 18000)` -- when Main.windSpeedSet is set -- or `genRand.Next(3600, 18000)`
			 *                          -- during worldgen.
			 *  
			 *  Weather Radio Display | Displayed wind speed is `Math.Abs(Main.windSpeed) * 100`
			 */

			TerraFlux flux = TerraFlux.Zero;

			float realWind = Math.Abs(Main.windSpeedCurrent) * 100;

			//Flux = 1TF/t multiplied by a factor of Wind Speed / 28mph
			float tfPerTick = 1f;
			flux += tfPerTick * realWind / 28f;
			flux *= 0.333f;

			//Wind turbine is in a sandstorm/blizzard?
			//Sandstorm: Multiply the generation by 1.15x and add a flat 0.5TF/t increase
			//Blizzard/Raining: Multiply the generation by 1.085x and add a flat 0.2TF/t increase
			if(sandstormBoost)
				flux *= 1.15f;
			if(rainBoost)
				flux *= 1.085f;

			if(sandstormBoost)
				flux += 0.5f;
			if(rainBoost)
				flux += 0.2f;

			flux *= ticks;

			return flux;
		}

		// TODO: portable battery slot???
		public override int SlotsCount => 0;
	}
}
