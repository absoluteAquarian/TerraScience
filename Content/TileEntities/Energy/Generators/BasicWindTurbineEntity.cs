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
			 *  Main.windSpeedCurrent   | The current wind speed.  Always tries to increment/decrement towards Main.windSpeedSet by a
			 *                            factor of `0.001f * Main.dayRate` per tick
			 *  
			 *  Main.windSpeedTarget    | The target wind speed.  Set to Main.windSpeedTemp when Main.weatherCounter is <= 0.  Value is
			 *                            set to `genRand.Next(-100, 101) * 0.01f` on world load/generation.
			 *  
			 *  Main.weatherCounter     | The timer used for sending net messages for syncing world data.  It is decremented by Main.dayRate
			 *                            every tick.  Value is initialized to `rand.Next(3600, 18000)` -- when Main.windSpeedSet is set
			 *                            -- or `genRand.Next(3600, 18000)` -- during worldgen.
			 *                          
			 *  Main.windCounter        | The timer used for updating Main.windSpeedTarget.  Frozen when the FreezeWindDirectionAndStrength
			 *                            Journey Mode power is active.
			 *                          
			 *  Main.extremeWindCounter | The timer used for "extreme" winds.  Used to control Main.windSpeedTarget randomly.  Has a random
			 *                            chance to stop immediately every tick.  Stronger winds result in a longer "extreme" duration on
			 *                            average.  Only updates while Main.windCounter <= 0.
			 *  
			 *  Weather Radio Display   | Displayed wind speed is `(int)(Math.Abs(Main.windSpeedCurrent) * 50)`
			 */

			TerraFlux flux = new TerraFlux(0f);

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
