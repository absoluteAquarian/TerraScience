using MagicStorage.Components;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.UI {
	public class MagicStorageConnectorUI : MachineUI{
		public override string Header => "Connector";

		public override int TileType => ModContent.TileType<MagicStorageConnector>();

		internal override void PanelSize(out int width, out int height){
			width = 250;
			height = 180;
		}

		internal override void InitializeSlots(List<UIItemSlot> slots){ }

		internal override void InitializeText(List<UIText> text){
			UIText items = new UIText("Not active"){
				HAlign = 0.5f
			};
			items.Top.Set(87, 0);
			text.Add(items);
		}

		internal override void UpdateText(List<UIText> text){
			if(!MagicStorageHandler.handler.ModIsActive)
				text[0].SetText("Magic Storage is not enabled");
			else
				StrongRef_UpdateText(text);
		}

		private void StrongRef_UpdateText(List<UIText> text){
			var entityPos = UIEntity.Position;

			if(MagicStorageHandler.DelayInteractionsDueToWorldSaving){
				text[0].SetText("World is saving, cannot interact");
				return;
			}

			//The machine isn't considered a valid MS system tile, obviously, so we need to check for a heart on the connected tiles instead
			Point16 center = FindMagicStorageSystem(entityPos);

			if(center == badCheck){
				text[0].SetText("Not connected");
				return;
			}

			//There's some sort of "system center" found by FindMagicStorageSystem.  get the Heart it's connected to
			Tile tile = Framing.GetTileSafely(center);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(!tile.HasTile){
				text[0].SetText("Not connected");
				return;
			}

			Point16 origin = tile.TileCoord();

			if(!(mTile is StorageAccess access)){
				text[0].SetText("Invalid storage system detected");
				return;
			}

			var heart = access.GetHeart(center.X - origin.X, center.Y - origin.Y);

			//Copied from Magic Storage
			int numItems = 0;
			int capacity = 0;
			if(heart != null){
				foreach(TEAbstractStorageUnit abstractStorageUnit in heart.GetStorageUnits()){
					if(abstractStorageUnit is TEStorageUnit storageUnit){
						numItems += storageUnit.NumItems;
						capacity += storageUnit.Capacity;
					}
				}
			}

			text[0].SetText($"Items: {numItems} / {capacity}");
		}

		internal static readonly IEnumerable<Point16> checkNeighbors2x2 = new Point16[]{
			new Point16(-1, 0),
			new Point16(0, -1),
			new Point16(1, -1),
			new Point16(2, 0),
			new Point16(2, 1),
			new Point16(1, 2),
			new Point16(0, 2),
			new Point16(-1, 1)
		};

		internal static readonly Point16 badCheck = new Point16(-1, -1);

		internal static Point16 FindMagicStorageSystem(Point16 tileOrig){
			//Can't return a type from Magic Storage in this method or the entire class would be unusable
			foreach(var neighbor in checkNeighbors2x2){
				Tile tile = Framing.GetTileSafely(tileOrig + neighbor);
				ModTile mTile = ModContent.GetModTile(tile.type);

				if(!tile.HasTile || !(mTile is StorageConnector || mTile is StorageAccess))
					continue;

				var center = TEStorageComponent.FindStorageCenter(tileOrig + neighbor);

				if(center != badCheck)
					return center;
			}

			return badCheck;
		}
	}
}
