using TerraScience.Content.Tiles;

namespace TerraScience.Systems.Pipes{
	public class FluidNetwork : Network<FluidPipe, FluidTransportTile> {
		internal override JunctionType Type => JunctionType.Fluids;
	}
}
