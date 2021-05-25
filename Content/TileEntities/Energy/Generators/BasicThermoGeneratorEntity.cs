using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic;
using TerraScience.Content.UI.Energy.Generators;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy.Generators{
	public class BasicThermoGeneratorEntity : GeneratorEntity{
		public override TerraFlux ExportRate => new TerraFlux(100f / 60f);

		public override TerraFlux FluxCap => new TerraFlux(2000f);

		public override int MachineTile => ModContent.TileType<BasicThermoGenerator>();

		public override int SlotsCount => 1;

		public int spinTimer = 0;

		private float fuelLeft = 0;
		private Item cachedFuelItem;

		public override TagCompound ExtraSave(){
			var tag = base.ExtraSave();
			tag.Add("fuelLeft", fuelLeft);
			tag.Add("cache", cachedFuelItem is null ? null : ItemIO.Save(cachedFuelItem));
			return tag;
		}

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);
			fuelLeft = tag.GetFloat("fuelLeft");
			cachedFuelItem = tag.GetCompound("cache") is TagCompound cache ? ItemIO.Load(cache) : null;
		}

		public override void PreUpdateReaction(){
			var item = this.RetrieveItem(0);
			ReactionInProgress = fuelLeft > 0;

			if(fuelLeft < 0)
				fuelLeft = 0;

			if(item.IsAir && !ReactionInProgress){
				ReactionProgress = 0;
				cachedFuelItem = null;
				fuelLeft = 0;
			}else if(fuelLeft <= 0){
				item.stack--;
				fuelLeft = 1;

				cachedFuelItem = new Item();
				cachedFuelItem.SetDefaults(item.type);

				if(item.stack <= 0)
					item.TurnToAir();
			}
		}

		public override bool UpdateReaction(){
			spinTimer++;

			TerraFlux flux = GetPowerGeneration(1);

			ImportFlux(ref flux);

			//Prevent additional fuel from being used until the flux has been used enough
			if(flux > new TerraFlux(0f))
				return false;

			//Burn one "fuel" item every 30 seconds
			fuelLeft -= 1f / 30f / 60f;
			ReactionProgress = (1f - fuelLeft) * 100f;

			if(fuelLeft <= 0)
				ReactionProgress = 100f;

			return fuelLeft <= 0;
		}

		public override void ReactionComplete(){
			Item item = this.RetrieveItem(0);
			item.stack--;
			
			if(item.stack <= 0)
				item.TurnToAir();
		}

		public override TerraFlux GetPowerGeneration(int ticks){
			TerraFlux flux;

			if(fuelLeft <= 0)
				return new TerraFlux(0f);

			Item fuel = cachedFuelItem;

			//The buffs "Well Fed" and "Tipsy" generate power based on the initial buff time
			//The Coal item generates 12 TF/s
			//"Wood"en items generate 8TF/s
			string name = Lang.GetItemNameValue(fuel.type);
			if(fuel.buffType == BuffID.WellFed)
				flux = new TerraFlux(10f * fuel.buffTime / (15 * 3600) / 60f);
			else if(fuel.buffType == BuffID.Tipsy)
				flux = new TerraFlux(5f * fuel.buffTime / (5 * 3600) / 60f);
			else if(fuel.type == ModContent.ItemType<Coal>())
				flux = new TerraFlux(12f / 60f);
			else if(name.Contains("Wood") || name.Contains("wood"))
				flux = new TerraFlux(8f / 60f);
			else
				flux = new TerraFlux(0f);

			return flux * ticks;
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override bool CanInputItem(int slot, Item item) 
			=> slot == 0 && BasicThermoGeneratorUI.ValidItem(item);
	}
}
