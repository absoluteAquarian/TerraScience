using Terraria.UI;

namespace TerraScience.Utilities{
	public static class UIUtils{
		//I want to kill whoever designed the Terraria UI system
		public static float GetCenterAlignmentHorizontal(UIElement parent, UIElement child){
			parent.Recalculate();
			child.Recalculate();

			var parentInner = parent.GetInnerDimensions();
			var childInner = child.GetInnerDimensions();

			float parentParentX = parent.Parent?.GetInnerDimensions().X ?? 0;

			return parentInner.X + parentInner.Width / 2f - childInner.Width - parentParentX;
		}

		public static float GetCenterAlignmentVertical(UIElement parent, UIElement child){
			parent.Recalculate();
			child.Recalculate();

			var parentInner = parent.GetInnerDimensions();
			var childInner = child.GetInnerDimensions();

			float parentParentY = parent.Parent?.GetInnerDimensions().Y ?? 0;

			return parentInner.Y + parentInner.Height / 2f - childInner.Height - parentParentY;
		}
	}
}
