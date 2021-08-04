using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TerraScience.API.UI;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.UI{
	public class ItemCacheUI : MachineUI{
		public override string Header => "Item Cache";

		public override int TileType => ModContent.TileType<ItemCache>();

		private int withdrawAmount;

		private UIItemSlotWrapper depositSlot, withdrawSlot;
		private UIText withdrawCount;
		private ClickableButton depositButton, withdrawButton,
			withdrawPlusOne, withdrawMinusOne,
			withdrawPlusTen, withdrawMinusTen,
			withDrawPlusHundred, withdrawMinusHundred;

		internal override void PanelSize(out int width, out int height){
			width = 400;
			height = 440;
		}

		internal override void InitializeSlots(List<UIItemSlotWrapper> slots){
			PanelSize(out int width, out int height);

			//Two slots, one for depositing items and one for withdrawing items
			depositSlot = new UIItemSlotWrapper(){
				ValidItemFunc = item => item.IsAir || (UIEntity as ItemCacheEntity).CanBeInput(item)
			};
			depositSlot.Top.Set(120, 0f);
			depositSlot.Left.Set(80, 0f);
			slots.Add(depositSlot);

			withdrawSlot = new UIItemSlotWrapper(){
				ValidItemFunc = item => item.IsAir  //Only retrieving items allowed
			};
			slots.Add(withdrawSlot);
			withdrawSlot.Top.Set(depositSlot.Top.Pixels, 0f);
			withdrawSlot.Left.Set(width - 80 - withdrawSlot.GetInnerDimensions().Width, 0f);
			withdrawSlot.Recalculate();

			//And an additional third slot for displaying whether the cache is locked
			UIItemSlotWrapper locked = new UIItemSlotWrapper(scale: 1f){
				ValidItemFunc = item => false  //No interacting with the slot
			};
			slots.Add(locked);
			locked.Top.Set(height - locked.GetInnerDimensions().Height - 20, 0f);
			locked.Left.Set(30, 0f);
			locked.Recalculate();
		}

		internal override void InitializeText(List<UIText> text){
			UIText storedItemName = new UIText("Storing: None", 1.3f){
				HAlign = 0.5f
			};
			storedItemName.Top.Set(58, 0f);
			text.Add(storedItemName);

			UIText storedItemTotalStack = new UIText("0 / 0", 1.3f){
				HAlign = 0.5f
			};
			storedItemTotalStack.Top.Set(87, 0f);
			text.Add(storedItemTotalStack);

			withdrawCount = new UIText("Withdrawing: 0");
			text.Add(withdrawCount);
		}

		internal override void InitializeOther(UIPanel panel){
			/*Layout
			 *    DEPOSIT              WITHDRAW
			 *                     [Withdrawing: #]
			 *                      [-1]      [+1]
			 *                      [-10]    [+10]
			 *                      [-100]  [+100]
			 *
			 *
			 *  (number buttons are centered horizontally with each other)
			 */

			//Deposit button
			depositButton = new ClickableButton("Deposit");
			depositButton.Left.Set(UIUtils.GetCenterAlignmentHorizontal(depositSlot, depositButton), 0f);
			depositButton.Top.Set(depositSlot.Top.Pixels + depositSlot.Height.Pixels + 20, 0f);
			panel.Append(depositButton);

			withdrawButton = new ClickableButton("Withdraw");
			withdrawButton.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawSlot, withdrawButton), 0f);
			withdrawButton.Top.Set(depositButton.Top.Pixels, 0f);
			panel.Append(withdrawButton);

			withdrawCount.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawButton, withdrawCount), 0f);
			withdrawCount.Top.Set(withdrawButton.Top.Pixels + 40, 0f);
			withdrawCount.Recalculate();

			//Minus buttons
			withdrawMinusOne = new ClickableButton("-1");
			withdrawMinusOne.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawCount, withdrawMinusOne) - 40, 0f);
			withdrawMinusOne.Top.Set(withdrawCount.Top.Pixels + 40, 0f);
			panel.Append(withdrawMinusOne);

			withdrawMinusTen = new ClickableButton("-10");
			withdrawMinusTen.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawMinusOne, withdrawMinusTen), 0f);
			withdrawMinusTen.Top.Set(withdrawMinusOne.Top.Pixels + 40, 0f);
			panel.Append(withdrawMinusTen);

			withdrawMinusHundred = new ClickableButton("-100");
			withdrawMinusHundred.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawMinusTen, withdrawMinusHundred), 0f);
			withdrawMinusHundred.Top.Set(withdrawMinusTen.Top.Pixels + 40, 0f);
			panel.Append(withdrawMinusHundred);

			//Plus buttons
			withdrawPlusOne = new ClickableButton("+1");
			withdrawPlusOne.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawCount, withdrawPlusOne) + 40, 0f);
			withdrawPlusOne.Top.Set(withdrawMinusOne.Top.Pixels, 0f);
			panel.Append(withdrawPlusOne);

			withdrawPlusTen = new ClickableButton("+10");
			withdrawPlusTen.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawPlusOne, withdrawPlusTen), 0f);
			withdrawPlusTen.Top.Set(withdrawMinusTen.Top.Pixels, 0f);
			panel.Append(withdrawPlusTen);

			withDrawPlusHundred = new ClickableButton("+100");
			withDrawPlusHundred.Left.Set(UIUtils.GetCenterAlignmentHorizontal(withdrawPlusTen, withDrawPlusHundred), 0f);
			withDrawPlusHundred.Top.Set(withdrawMinusHundred.Top.Pixels, 0f);
			panel.Append(withDrawPlusHundred);
		}

		public override void PreOpen(){
			withdrawAmount = 0;
		}

		internal override void UpdateText(List<UIText> text){
			ItemCacheEntity entity = UIEntity as ItemCacheEntity;

			Item top = UIEntity.RetrieveItem(-1);
			int itemType = entity.locked ? entity.lockItemType : top.type;

			text[0].SetText("Storing: " + (itemType <= 0 ? "Nothing" : Lang.GetItemNameValue(itemType)));
			text[1].SetText($"{entity.GetTotalItems()} / {entity.GetPerStackMax() * entity.MaxStacks}");

			withdrawCount.SetText("Withdrawing: " + withdrawAmount);
		}

		public override void UpdateMisc(){
			//Check button presses here
			ItemCacheEntity entity = UIEntity as ItemCacheEntity;

			//Desposit the item if applicable
			Item depositItem = UIEntity.RetrieveItem(0);
			if(depositButton.LeftClick && !depositItem.IsAir){
				if(UIEntity.CanBeInput(depositItem)){
					var path = ItemNetworkPath.CreateDummyObject(depositItem);

					UIEntity.InputItemFromNetwork(path, out bool sendBack);

					//Items were sent back?  Keep them in the slot
					if(sendBack)
						GetSlot(0).SetItem(ItemIO.Load(path.itemData));
					else
						GetSlot(0).SetItem(0, 0);
				}
			}

			//Withdraw items if applicable
			if(withdrawButton.LeftClick && withdrawAmount > 0){
				//Using the Hijack methods here are fine until the item extraction code is moved to MachineEntity
				UIEntity.HijackGetItemInventory(out Item[] inventory);
				UIEntity.HijackExtractItem(inventory, -1, withdrawAmount, out Item extracted);

				//Not null?  An item was extracted
				if(extracted != null){
					var slot = GetSlot(1);
					Item existing = slot.StoredItem;

					bool sendBack = false;

					if(existing.IsAir)
						slot.SetItem(extracted);
					else if(extracted.type == existing.type){
						if(extracted.stack + existing.stack <= existing.maxStack){
							//Add to the stack
							existing.stack += extracted.stack;
						}else if(existing.stack < existing.maxStack){
							//Remove from the inserted item and insert it back into the machine
							extracted.stack -= existing.maxStack - existing.stack;
							existing.stack = existing.maxStack;

							sendBack = true;
						}else
							sendBack = true;  //Failsafe
					}

					if(sendBack)
						UIEntity.InputItemFromNetwork(ItemNetworkPath.CreateDummyObject(extracted), out _);
				}

				withdrawAmount = 0;
			}

			//Handle changing how many items are withdrawn
			if(withdrawMinusOne.LeftClick)
				HandleAmountButtonClick(-1);
			else if(withdrawMinusTen.LeftClick)
				HandleAmountButtonClick(-10);
			else if(withdrawMinusHundred.LeftClick)
				HandleAmountButtonClick(-100);
			else if(withdrawPlusOne.LeftClick)
				HandleAmountButtonClick(1);
			else if(withdrawPlusTen.LeftClick)
				HandleAmountButtonClick(10);
			else if(withDrawPlusHundred.LeftClick)
				HandleAmountButtonClick(100);

			var keySlot = GetSlot(2);
			if(entity.locked)
				keySlot.SetItem(ItemID.GoldenKey, 1);
			else{
				keySlot.StoredItem.type = ItemID.None;
				keySlot.StoredItem.netID = 0;
				keySlot.StoredItem.stack = 0;
			}
		}

		private void HandleAmountButtonClick(int amountToModify){
			var entity = UIEntity as ItemCacheEntity;

			withdrawAmount += amountToModify;

			//Clamp by how many items are in the system, then by max stack
			withdrawAmount = Utils.Clamp(withdrawAmount, 0, entity.GetTotalItems());
			withdrawAmount = Utils.Clamp(withdrawAmount, 0, entity.GetPerStackMax());
		}
	}
}
