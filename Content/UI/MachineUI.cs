using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public abstract class MachineUI : UIState{
		public static string UIDecimalFormat(float value) => $"{value :0.##}";

		public string MachineName;

		public MachineEntity UIEntity;

		public bool CheckedForSavedItemCount;

		public bool Active;

		public virtual void DoSavedItemsCheck(){ }

		private List<UIText> Text;

		private List<UIItemSlot> ItemSlots;

		public abstract string Header{ get; }
		internal abstract void InitializeText(List<UIText> text);
		internal abstract void InitializeSlots(List<UIItemSlot> slots);
		internal abstract void PanelSize(out int width, out int height);

		internal virtual void InitializeOther(UIPanel panel){ }

		internal virtual void UpdateText(List<UIText> text){ }

		internal virtual void UpdateEntity(){ }

		public virtual void PreClose(){ }

		internal UIItemSlot GetSlot(int index) => ItemSlots[index];

		public abstract int TileType{ get; }

		public int SlotsLength => ItemSlots.Count;

		public virtual void PlayOpenSound() => Main.PlaySound(SoundID.MenuOpen);
		public virtual void PlayCloseSound() => Main.PlaySound(SoundID.MenuClose);

		public int UIDelay = -1;

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
			UIText header = new UIText(Header, 1, true) {
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
		}

		public sealed override void Update(GameTime gameTime){
			base.Update(gameTime);

			if(UIDelay > 0)
				UIDelay--;

			Main.playerInventory = true;

			if (UIEntity != null) {
				//Get the machine's center position
				Vector2 middle = TileUtils.TileEntityCenter(UIEntity, TileType);

				(ModContent.GetModTile(TileType) as Machine).GetDefaultParams(out _, out uint width, out uint height, out _);

				//Check if the inventory key was pressed or if the player is too far away from the tile.  If so, close the UI
				bool tooFar = Math.Abs(Main.LocalPlayer.Center.X - middle.X) > width * 8 + Main.LocalPlayer.lastTileRangeX * 16;
				tooFar |= Math.Abs(Main.LocalPlayer.Center.Y - middle.Y) > height * 8 + Main.LocalPlayer.lastTileRangeY * 16;
				if (Main.LocalPlayer.GetModPlayer<TerraSciencePlayer>().InventoryKeyPressed || tooFar){
					TechMod.Instance.machineLoader.HideUI(MachineName);
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
