using Terraria;
using TerraScience.Content.TileEntities;
using TerraScience.Content.UI;
using TerraScience.Systems;

namespace TerraScience.API.UI{
	public class UITesseractItemSlot : UIItemSlot{
		public readonly int Slot;

		public TesseractUI parentUI;

		public UITesseractItemSlot(int slot, TesseractUI parentUI) : base(){
			Slot = slot;
			this.parentUI = parentUI;

			OnItemChanged = item => {
				//Update the entry's item to the new item
				ref var entryItem = ref GetItemRef();
				entryItem = item.Clone();
				base.storedItem = entryItem.Clone();
			};
		}

		public override Item StoredItem => GetItemRef();

		public ref Item GetItemRef(){
			if(parentUI is null)
				return ref base.storedItem;

			var entity = parentUI.UIEntity as TesseractEntity;
			var net = entity.BoundNetwork;

			if(!TesseractNetwork.TryGetEntry(net, out var entry)){
				//Invalid entry, set the bound network to null
				entity.BoundNetwork = null;
				return ref base.storedItem;
			}

			return ref entry.items[Slot];
		}
	}
}
