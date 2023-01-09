using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public enum ArrowElementOrientation : byte {
		Left = 0,
		Up,
		Right,
		Down
	}

	public class BasicThinArrow : UIElement {
		public readonly ArrowElementOrientation orientation;
		private float targetLength;

		public BasicThinArrow(ArrowElementOrientation orientation, float targetLength) {
			this.orientation = orientation;
			this.targetLength = targetLength;

			ArrangeSelf();
		}

		private void ArrangeSelf() {
			// TODO: set Width and Height like how Thermostat.cs does it
		}
	}
}
