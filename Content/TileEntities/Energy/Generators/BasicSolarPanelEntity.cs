using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Generators{
	public class BasicSolarPanelEntity : GeneratorEntity{
		public override int MachineTile => ModContent.TileType<BasicSolarPanel>();

		public override int SlotsCount => 0;

		public override TerraFlux FluxCap => new TerraFlux(3000);

		//max export is rougly 6TF/t
		public override TerraFlux ExportRate => new TerraFlux(6);

		public float panelRotation;

		public bool eclipseReduce;
		public bool rainReduce;

		public override void ReactionComplete(){
			TerraFlux flux = GetPowerGeneration(ticks: 1);

			ImportFlux(ref flux);
		}

		public override bool UpdateReaction() => true;

		const float fullDay = 54000;

		public override void PreUpdateReaction(){
			float factor = (fullDay / 2 - (float)Main.time) / (fullDay / 2);
			float desiredRotation = !Main.dayTime ? 0 : MathHelper.ToRadians(-55) * factor;

			//Ease into the desired rotation
			panelRotation = MathHelper.Lerp(panelRotation, desiredRotation, 0.25f);

			if(!Main.dayTime){
				//Only generate power during the day
				ReactionProgress = 0;
				ReactionInProgress = false;
			}else{
				//Always adding energy
				ReactionProgress = 100;
				ReactionInProgress = true;
			}
		}

		public override TerraFlux GetPowerGeneration(int ticks){
			if(!Main.dayTime)
				return new TerraFlux(0f);

			//Base rate: 40 TF/s
			TerraFlux flux = new TerraFlux(40f / 60f);

			//Power generation is at max when it's noon
			float factor = (fullDay - (float)Math.Abs(fullDay / 2 - Main.time)) / fullDay;

			eclipseReduce = Main.eclipse;
			rainReduce = Main.raining && Main.windSpeed != 0;

			if(eclipseReduce){
				//Eclipe reduces power gen
				factor *= 0.06f;
			}

			if(rainReduce){
				//Rain blockage is dependent on wind speed
				//Get a factor of (current speed) / 28mph
				float realWind = Math.Abs(Main.windSpeed) * 100;
				float reduce = 0.12f * realWind / 28f;

				if(reduce > 0.5f)
					reduce = 0.5f;

				factor *= 1f - reduce;
			}

			return flux * factor * ticks;
		}
	}
}
