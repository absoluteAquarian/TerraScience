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

		public bool IsClosing{ get; internal set; }

		/// <summary>
		/// Called before the UI attempts to copy items stored in the entity into the UI, but after the UI state has been activated
		/// </summary>
		public virtual void DoSavedItemsCheck(){ }

		private List<UIText> Text;

		private List<UIItemSlot> ItemSlots;

		public abstract string Header{ get; }

		/// <summary>
		/// Create and append <seealso cref="UIText"/> instances here
		/// </summary>
		/// <param name="text">The list of text.  All entries are directly appended to the UI panel</param>
		internal abstract void InitializeText(List<UIText> text);

		/// <summary>
		/// Create and append <seealso cref="UIItemSlot"/> instances here
		/// </summary>
		/// <param name="slots">The list of item slots.  All entries are directly appended to the UI panel</param>
		internal abstract void InitializeSlots(List<UIItemSlot> slots);

		/// <summary>
		/// Gets the size of the UI panel
		/// </summary>
		/// <param name="width">The width of the panel in pixels</param>
		/// <param name="height">The height of the panel in pixels</param>
		internal abstract void PanelSize(out int width, out int height);

		/// <summary>
		/// Create and append other UI elements here.  The back panel as a <seealso cref="UIPanel"/> is provided as an argument
		/// </summary>
		/// <param name="panel">The back panel, cast to <seealso cref="UIPanel"/></param>
		internal virtual void InitializeOther(UIPanel panel){ }

		/// <summary>
		/// Update text here.  The <paramref name="text"/> list is the same list as the one from <seealso cref="InitializeText(List{UIText})"/>
		/// </summary>
		/// <param name="text">The list of text</param>
		internal virtual void UpdateText(List<UIText> text){ }

		/// <summary>
		/// General update method.  This method is called after <seealso cref="PreUpdateEntity"/>
		/// </summary>
		internal virtual void UpdateEntity(){ }

		/// <summary>
		/// This method is called after <seealso cref="PlayCloseSound"/> and before the slots in the UI state are saved to the entity
		/// </summary>
		public virtual void PreClose(){ }

		/// <summary>
		/// This method is called after the UI state is deactivated
		/// </summary>
		public virtual void PostClose(){ }

		/// <summary>
		/// This method is called after <seealso cref="PlayOpenSound"/> and before the UI state is activated
		/// </summary>
		public virtual void PreOpen(){ }

		/// <summary>
		/// This method is called after the UI state is initialized and its item slots have been loaded
		/// </summary>
		public virtual void PostOpen(){ }

		internal UIItemSlot GetSlot(int index) => ItemSlots[index];

		/// <summary>
		/// The type of the <seealso cref="Machine"/> tile this UI state is bound to
		/// </summary>
		public abstract int TileType{ get; }

		public int SlotsLength => ItemSlots.Count;

		/// <summary>
		/// Called when the UI is opened.  Change what sound is played here
		/// </summary>
		public virtual void PlayOpenSound() => Main.PlaySound(SoundID.MenuOpen);

		/// <summary>
		/// Called when the UI is closed.  Change what sound is played here
		/// </summary>
		public virtual void PlayCloseSound() => Main.PlaySound(SoundID.MenuClose);

		public int UIDelay = -1;

		private UIDragablePanel panel;

		internal void ClearSlots(){
			foreach(var slot in ItemSlots)
				slot.SetItem(new Item());
		}

		internal void LoadFromSlots(Item[] slots){
			//Possible if the slots hadn't been saved to yet
			if(slots?.Length != SlotsLength)
				return;

			for(int i = 0; i < SlotsLength; i++)
				ItemSlots[i].SetItem(slots[i]);
		}

		protected void AppendElement(UIElement element){
			panel.Append(element);
		}

		protected void RemoveElement(UIElement element){
			panel.Append(element);
		}

		protected bool PanelHasChild(UIElement element) => panel.HasChild(element);

		public sealed override void OnInitialize(){
			//Make the panel
			panel = new UIDragablePanel();

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
