using Terraria.GameContent.UI.Elements;

namespace TerraScience.API.UI{
	public class UITesseractKnownPanel : UIPanel{
		public readonly int PageSlot = -1;

		public UITesseractKnownPanel(int index){
			PageSlot = index;
		}
	}
}
