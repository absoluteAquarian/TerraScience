using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles{
	public abstract class JunctionMergeable : ModTile{
		public abstract JunctionType MergeType{ get; }

		internal static JunctionMerge[] mergeTypes;

		public sealed override void SetDefaults(){
			SafeSetDefaults();
			//Non-solid, but this is required.  Explanation is in TechMod.PreUpdateEntities()
			Main.tileSolid[Type] = false;
			Main.tileNoSunLight[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.FlattenAnchors = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.addTile(Type);
		}

		public virtual void SafeSetDefaults(){ }

		internal static bool AtLeastOneSurroundingTileIsActive(int i, int j)
			=> (i > 0 && Framing.GetTileSafely(i - 1, j).active()) || (j > 0 && Framing.GetTileSafely(i, j - 1).active()) || (i < Main.maxTilesX - 1 && Framing.GetTileSafely(i + 1, j).active()) || (j < Main.maxTilesY - 1 && Framing.GetTileSafely(i, j + 1).active());

		public override bool CanPlace(int i, int j){
			//This hook is called just before the tile is placed, which means we can fool the game into thinking this tile is solid when it really isn't
			TechMod.Instance.SetNetworkTilesSolid();
			return AtLeastOneSurroundingTileIsActive(i, j);
		}

		public override void PlaceInWorld(int i, int j, Item item){
			//(Continuing from CanPlace)... then I can just set it back to false here
			TechMod.Instance.ResetNetworkTilesSolid();
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak){
			Tile source = Framing.GetTileSafely(i, j);

			//Determine how this tile should merge
			Tile up = j >= 0 ? Framing.GetTileSafely(i, j - 1) : null;
			Tile left = i >= 0 ? Framing.GetTileSafely(i - 1, j) : null;
			Tile right = i < Main.maxTilesX ? Framing.GetTileSafely(i + 1, j) : null;
			Tile down = j < Main.maxTilesY ? Framing.GetTileSafely(i, j + 1) : null;

			bool canMergeUp = up != null && CheckTileMerge(i, j, dirX: 0, dirY: -1);
			bool canMergeLeft = left != null && CheckTileMerge(i, j, dirX: -1, dirY: 0);
			bool canMergeRight = right != null && CheckTileMerge(i, j, dirX: 1, dirY: 0);
			bool canMergeDown = down != null && CheckTileMerge(i, j, dirX: 0, dirY: 1);

			//Default to the "no merge" frame
			int frameX = 0;
			int frameY = 0;

			//Fortunately, the tilesets for these tiles are much easier to work with
			if(!canMergeUp && !canMergeLeft && !canMergeRight && !canMergeDown){  //None connected
				//Only one frame for this, the default
				//0000
			}else if(canMergeUp && !canMergeLeft && !canMergeRight && !canMergeDown){  //Main connection: Up
				//1000
				frameX = 6;
				frameY = 3;
			}else if(canMergeUp && canMergeLeft && !canMergeRight && !canMergeDown){
				//1100
				frameX = Main.rand.NextBool() ? 2 : 5;
				frameY = 3;
			}else if(canMergeUp && !canMergeLeft && canMergeRight && !canMergeDown){
				//1010
				frameX = Main.rand.NextBool() ? 0 : 3;
				frameY = 3;
			}else if(canMergeUp && canMergeLeft && canMergeRight && !canMergeDown){
				//1110
				frameX = Main.rand.NextBool() ? 1 : 4;
				frameY = 3;
			}else if(canMergeUp && !canMergeLeft && !canMergeRight && canMergeDown){
				//1001
				frameX = 6;
				frameY = 2;
			}else if(canMergeUp && canMergeLeft && !canMergeRight && canMergeDown){
				//1101
				frameX = Main.rand.NextBool() ? 2 : 5;
				frameY = 2;
			}else if(canMergeUp && !canMergeLeft && canMergeRight && canMergeDown){
				//1011
				frameX = Main.rand.NextBool() ? 0 : 3;
				frameY = 2;
			}else if(!canMergeUp && canMergeLeft && !canMergeRight && !canMergeDown){  //Main connection: Left
				//0100
				frameX = 3;
				frameY = 0;
			}else if(!canMergeUp && canMergeLeft && canMergeRight && !canMergeDown){
				//0110
				frameX = 2;
				frameY = 0;
			}else if(!canMergeUp && canMergeLeft && !canMergeRight && canMergeDown){
				//0101
				frameX = Main.rand.NextBool() ? 2 : 5;
				frameY = 1;
			}else if(!canMergeUp && canMergeLeft && canMergeRight && canMergeDown){
				//0111
				frameX = Main.rand.NextBool() ? 1 : 4;
				frameY = 1;
			}else if(!canMergeUp && !canMergeLeft && canMergeRight && !canMergeDown){  //Main connection: Right
				//0010
				frameX = 1;
				frameY = 0;
			}else if(!canMergeUp && !canMergeLeft && canMergeRight && canMergeDown){
				//0011
				frameX = Main.rand.NextBool() ? 0 : 3;
				frameY = 1;
			}else if(!canMergeUp && !canMergeLeft && !canMergeRight && canMergeDown){  //Main connection: Down
				//0001
				frameX = 6;
				frameY = 1;
			}else if(canMergeUp && canMergeLeft && canMergeRight && canMergeDown){  //All connected
				//1111
				frameX = Main.rand.NextBool() ? 1 : 4;
				frameY = 2;
			}

			source.frameX = (short)(frameX * 18);
			source.frameY = (short)(frameY * 18);

			//Custom logic is used
			return false;
		}

		const int junctionColumns = 16;

		private bool CheckTileMerge(int i, int j, int dirX, int dirY){
			if(MergeType == JunctionType.None)
				return false;

			if((dirX == 0 && dirY == 0) || (dirX != 0 && dirY != 0))
				return false;

			if(dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
				return false;

			int targetX = i + dirX;
			int targetY = j + dirY;

			if(targetX < 0 || targetX >= Main.maxTilesX || targetY < 0 || targetY >= Main.maxTilesY)
				return false;

			Tile target = Framing.GetTileSafely(targetX, targetY);
			ModTile targetModTile = ModContent.GetModTile(target.type);

			if(!target.active())
				return false;

			//If this merge type is "Items" and the tile is a chest, merge
			if(MergeType == JunctionType.Items && (TileID.Sets.BasicChest[target.type] || (targetModTile != null && (targetModTile.adjTiles.Contains(TileID.Containers) || targetModTile.adjTiles.Contains(TileID.Containers2))))){
				return true;
			}

			if(target.type < TileID.Count)
				return false;

			if(targetModTile is JunctionMergeable merge){
				bool hasSameMerge = merge.MergeType == MergeType;
				
				//Need to check if the target is a pump and the pump is pointing in the right direction
				if(merge is ItemPumpTile || merge is FluidPumpTile){
					int frame = target.frameX / 18;
					return hasSameMerge && ((dirY == 1 && frame == 0) || (dirX == 1 && frame == 1) || (dirY == -1 && frame == 2) || (dirX == -1 && frame == 3));
				}else
					return hasSameMerge;
			}else if(targetModTile is TransportJunction){
				int frameX = target.frameX / 18;

				//Junction has the default frame?  Don't let them merge...
				if(frameX == 0)
					return false;

				//Invalid frame?  Don't merge
				if(frameX < 0 || frameX > junctionColumns)
					return false;

				//dirX == -1 means the junction must have a frame showing this mergetype on its right edge
				//dirX == 1 means the junction must have a frame showing this mergetype on its left edge
				//dirY == -1 means the junction must have a frame showing this mergetype on its bottom edge
				//dirY == 1 means the junction must have a frame showing this mergetype on its top edge

				//See Content/Tiles/TransportJunction.png for reference

				JunctionMerge otherMerge = mergeTypes[frameX];

				int shift = ((int)MergeType - 1) * 4;

				int mask = 0;
				if(dirX == -1)
					mask |= 0x4;
				else if(dirX == 1)
					mask |= 0x2;

				if(dirY == -1)
					mask |= 0x08;
				else if(dirY == 1)
					mask |= 0x01;

				mask <<= shift;

				return ((int)otherMerge & mask) != 0;
			}else if(targetModTile is Machine && TileUtils.tileToEntity.TryGetValue(targetModTile.Type, out MachineEntity entity)){
				return (MergeType == JunctionType.Wires && entity is PoweredMachineEntity)
					|| (MergeType == JunctionType.Items && (entity.GetInputSlots().Length > 0 || entity.GetOutputSlots().Length > 0))
					|| (MergeType == JunctionType.Fluids && (entity is ILiquidMachine || entity is IGasMachine));
			}else if(((MergeType == JunctionType.Items && targetModTile is ItemPumpTile) || (MergeType == JunctionType.Fluids && targetModTile is FluidPumpTile)) && ((dirX == -1 && target.frameX / 18 == 3) || (dirX == 1 && target.frameX / 18 == 1) || (dirY == -1 && target.frameX / 18 == 2) || (dirY == 1 && target.frameX / 18 == 0)))
				return true;


			//Tile wasn't a junction-mergeable tile
			return false;
		}

		internal static void InitMergeArray(){
			mergeTypes = new JunctionMerge[16]{
				JunctionMerge.None,
				JunctionMerge.Wires_LeftRight,
				JunctionMerge.Items_LeftRight,
				JunctionMerge.Fluids_LeftRight,
				JunctionMerge.Wires_UpDown,
				JunctionMerge.Items_UpDown,
				JunctionMerge.Fluids_UpDown,
				JunctionMerge.Wires_All,
				JunctionMerge.Items_All,
				JunctionMerge.Fluids_All,
				JunctionMerge.Cross_ItemUpDown_WireLeftRight,
				JunctionMerge.Cross_FluidUpDown_WireLeftRight,
				JunctionMerge.Cross_WireUpDown_ItemLeftRight,
				JunctionMerge.Cross_FluidLeftRight_ItemLeftRight,
				JunctionMerge.Cross_WireUpDown_FluidLeftRight,
				JunctionMerge.Cross_ItemUpDown_FluidLeftRight
			};
		}

		internal static int FindMergeIndex(JunctionMerge type) => Array.FindIndex(mergeTypes, m => m == type);
	}

	[Flags]
	public enum JunctionType{
		None = 0,
		Wires = 1,
		Items = 2,
		Fluids = 4
	}

	[Flags]
	public enum JunctionMerge{
		None = 0x0,

		Wire_Up = 0x1,
		Wire_Left = 0x2,
		Wire_Right = 0x4,
		Wire_Down = 0x8,

		Wires_LeftRight = Wire_Left | Wire_Right,
		Wires_UpDown = Wire_Up | Wire_Down,
		Wires_All = Wire_Up | Wire_Left | Wire_Right | Wire_Down,

		Item_Up = 0x10,
		Item_Left = 0x20,
		Item_Right = 0x40,
		Item_Down = 0x80,

		Items_LeftRight = Item_Left | Item_Right,
		Items_UpDown = Item_Up | Item_Down,
		Items_All = Item_Up | Item_Left | Item_Right | Item_Down,

		Fluid_Up = 0x100,
		Fluid_Left = 0x200,
		Fluid_Right = 0x400,
		Fluid_Down = 0x800,

		Fluids_LeftRight = Fluid_Left | Fluid_Right,
		Fluids_UpDown = Fluid_Up | Fluid_Down,
		Fluids_All = Fluid_Up | Fluid_Left | Fluid_Right | Fluid_Down,

		Cross_WireUpDown_ItemLeftRight = Wires_UpDown | Items_LeftRight,
		Cross_WireUpDown_FluidLeftRight = Wires_UpDown | Fluids_LeftRight,
		Cross_ItemUpDown_WireLeftRight = Items_UpDown | Wires_LeftRight,
		Cross_ItemUpDown_FluidLeftRight = Items_UpDown | Fluids_LeftRight,
		Cross_FluidUpDown_WireLeftRight = Fluids_UpDown | Wires_LeftRight,
		Cross_FluidLeftRight_ItemLeftRight = Fluids_UpDown | Wires_LeftRight
	}
}
