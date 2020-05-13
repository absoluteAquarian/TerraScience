using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public abstract class MachineUI : UIState{
		public string MachineName;

		public MachineEntity UIEntity;

		public bool CheckedForSavedItemCount;

		public bool Active;

		public abstract void DoSavedItemsCheck();

		private List<UIText> Text;

		private List<UIItemSlot> ItemSlots;

		public abstract string GetHeader();
		internal abstract void InitializeText(List<UIText> text);
		internal abstract void InitializeSlots(List<UIItemSlot> slots);
		internal abstract void PanelSize(out int width, out int height);

		internal virtual void InitializeOther(UIPanel panel){ }

		internal virtual void UpdateText(List<UIText> text){ }

		internal virtual void UpdateEntity(){ }

		public virtual void PreClose(){ }

		public virtual bool RequiresUI() => false;

		internal UIItemSlot GetSlot(int index) => ItemSlots[index];

		public int SlotsLength{ get; internal set; }

		public abstract Tile[,] GetStructure();

		internal void ClearSlots(){
			foreach(var slot in ItemSlots)
				slot.StoredItem.TurnToAir();
		}

		public void LoadToSlots(List<Item> slots){
			//Possible if the slots hadn't been saved to yet
			if(slots.Count != SlotsLength)
				return;

			for(int i = 0; i < SlotsLength; i++)
				ItemSlots[i].SetItem(slots[i], slots[i].stack);
		}

		public sealed override void OnInitialize(){
			//Make the panel
			UIDragablePanel panel = new UIDragablePanel();

			PanelSize(out int width, out int height);

			panel.Width.Set(width, 0);
			panel.Height.Set(height, 0);
			panel.HAlign = panel.VAlign = 0.5f;
			Append(panel);

			//Make the header text
			UIText header = new UIText(GetHeader(), 1, true) {
				HAlign = 0.5f
			};
			header.Top.Set(10, 0);
			panel.Append(header);

			//Make the rest of the text
			Text = new List<UIText>();
			InitializeText(Text);
			foreach(var text in Text)
				panel.Append(text);

			//Make the item slots
			ItemSlots = new List<UIItemSlot>();
			InitializeSlots(ItemSlots);
			foreach(var slot in ItemSlots)
				panel.Append(slot);

			InitializeOther(panel);

			SlotsLength = ItemSlots.Count;
		}

		public sealed override void Update(GameTime gameTime){
			base.Update(gameTime);

			Main.playerInventory = true;

			if (UIEntity != null) {
				// Get the machine's center position
				Vector2 middle = TileUtils.TileEntityCenter(UIEntity, GetStructure());

				// Check if the inventory key was pressed or if the player is 5 blocks away from the tile.  If so, close the UI
				if (Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed || Vector2.Distance(Main.LocalPlayer.Center, middle) > 5 * 16) {
					ModContent.GetInstance<TerraScience>().machineLoader.HideUI(MachineName);
					Main.playerInventory = false;
					return;
				}

				PreUpdateEntity();
				
				UpdateEntity();
				
				UpdateMisc();

				UpdateText(Text);
			}
		}

		/// <summary>
		/// For update tasks that should happen before this machine's MachineEntity is processed.
		/// </summary>
		public virtual void PreUpdateEntity(){ }

		/// <summary>
		/// For update tasks that should happen after this machine's MachineEntity is processed.
		/// </summary>
		public virtual void UpdateMisc(){ }
	}
}
