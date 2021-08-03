using System.IO;
using Terraria.ModLoader.IO;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy{
	public abstract class PoweredMachineEntity : MachineEntity{
		public override TagCompound ExtraSave()
			=> new TagCompound(){
				["flux"] = (float)StoredFlux
			};

		public override void ExtraLoad(TagCompound tag){
			StoredFlux = new TerraFlux(tag.GetFloat("flux"));
		}

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write((float)StoredFlux);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			StoredFlux = new TerraFlux(reader.ReadSingle());
		}

		public TerraFlux StoredFlux{ get; set; }

		public abstract TerraFlux FluxUsage{ get; }

		public abstract TerraFlux FluxCap{ get; }

		/// <summary>
		/// Adds the incoming <paramref name="flux"/> to this machine's flux pool.  Any excess <paramref name="flux"/> is left in the parameter for the network to collect.
		/// </summary>
		/// <param name="flux">The incoming Terra Flux power</param>
		public void ImportFlux(ref TerraFlux flux){
			if(flux + StoredFlux > FluxCap){
				TerraFlux old = StoredFlux;
				StoredFlux = FluxCap;
				flux -= StoredFlux - old;
			}else{
				StoredFlux += flux;
				flux = new TerraFlux(0f);
			}
		}

		internal bool CheckFluxRequirement(TerraFlux amount, bool use = true){
			if(StoredFlux < amount)
				return false;

			if(use)
				StoredFlux -= amount;

			return true;
		}
	}
}
