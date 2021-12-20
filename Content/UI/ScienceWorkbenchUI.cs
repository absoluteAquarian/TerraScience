using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.UI;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.UI {
	public class ScienceWorkbenchItemRegistry{
		public delegate string GetDisplay(int currentTick);

		public readonly GetDisplay GetFirstDisplay;
		public readonly GetDisplay GetSecondDisplay;

		/// <summary>
		/// The string representing the description for the item.  Lines should be separated by a newline character
		/// </summary>
		public readonly string Description;

		public ScienceWorkbenchItemRegistry(GetDisplay firstDisplayFunc, GetDisplay secondDisplayFunc, string description){
			GetFirstDisplay = firstDisplayFunc;
			GetSecondDisplay = secondDisplayFunc;
			Description = description;
		}
	}

	public class ScienceWorkbenchUI : MachineUI{
		public UIItemSlot item;

		public override string Header => "Science Workbench";

		public override int TileType => ModContent.TileType<ScienceWorkbench>();

		internal override void PanelSize(out int width, out int height){
			width = 400;
			height = 400;
		}

		internal override void UpdateText(List<UIText> text){
			// TODO: update text indicating what mahcine is in the slot
		}

		internal override void UpdateEntity(){
			// TODO: update UI stuff containing pictures, information, etc.
		}

		internal override void InitializeText(List<UIText> text) {
			// TODO: text for displaying what machine is in the slot
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){
			slots.Add(item = new UIItemSlot(){
				ValidItemFunc = item => item.IsAir || item.modItem is MachineItem
			});
		}

		internal override void InitializeOther(UIPanel panel){
			// TODO: UI slots for open/close images, other displays and information text (scrolling???)
		}

		public override void PreClose(){
			if(!item.StoredItem.IsAir){
				Main.LocalPlayer.QuickSpawnClonedItem(item.StoredItem, item.StoredItem.stack);
				item.SetItem(new Item());
			}
		}
	}
}
