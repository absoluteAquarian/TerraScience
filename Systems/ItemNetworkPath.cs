using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Systems.Pathfinding;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Systems {
	/// <summary>
	/// An object representing an item going through an <seealso cref="ItemNetwork"/>
	/// </summary>
	public class ItemNetworkPath{
		/// <summary>
		/// The data of the item passing through the network
		/// </summary>
		public TagCompound itemData;

		public ItemNetwork itemNetwork;

		public Vector2 worldCenter;
		internal Vector2 oldCenter;
		internal Vector2 moveDir;
		internal Vector2 finalDir;

		private Queue<Point16> currentPath;
		private Point16 dequeuedPoint;

		internal IReadOnlyCollection<Point16> Path => currentPath?.ToList().AsReadOnly();

		public readonly int id;

		private static int nextID;

		public bool needsPathRefresh = true;
		internal bool wander;
		internal bool enteringChestOrMachine;

		internal bool delayPathCalc;

		public bool removed;

		private ItemNetworkPath(){
			id = nextID++;
		}

		public TagCompound Save()
			=> new TagCompound(){
				["data"] = itemData,
				["pos"] = worldCenter
			};

		public static ItemNetworkPath Load(TagCompound tag)
			=> new ItemNetworkPath(){
				itemData = tag.GetCompound("data"),
				worldCenter = tag.Get<Vector2>("pos")
			};

		public void Delete(){
			currentPath = null;
			itemData = null;
			itemNetwork = null;
			removed = true;
		}
		
		/// <summary>
		/// Creates a new <seealso cref="ItemNetworkPath"/> object
		/// </summary>
		/// <param name="item">The item to be sent into the network</param>
		/// <param name="network">The network the item is being sent into</param>
		/// <param name="pumpSource">The tile location of the item pump</param>
		public static ItemNetworkPath CreateObject(Item item, ItemNetwork network, Point16 pumpSource, List<Point16> pathOverride = null){
			if(!network.HasEntryAt(pumpSource))
				return null;

			if(item.IsAir)
				return null;

			ItemNetworkPath path = new ItemNetworkPath(){
				itemData = ItemIO.Save(item),
				itemNetwork = network,
				worldCenter = pumpSource.ToWorldCoordinates()
			};

			if(pathOverride?.Count > 0){
				path.currentPath = new Queue<Point16>(pathOverride);
				path.needsPathRefresh = false;

				path.dequeuedPoint = path.currentPath.Dequeue();
			}

			return path;
		}

		internal static ItemNetworkPath CreateDummyObject(Item item)
			=> new ItemNetworkPath(){
				removed = true,
				itemData = ItemIO.Save(item)
			};

		private int sendBackCounts = 10;

		public Point16 finalTarget;

		/// <summary>
		/// Attempts to move the item in this object along the <seealso cref="itemNetwork"/>
		/// </summary>
		public void Update(){
			if(removed)
				return;

			oldCenter = worldCenter;

			var newTilePos = worldCenter.ToTileCoordinates16();

			if(delayPathCalc && itemNetwork.HasEntryAt(newTilePos))
				delayPathCalc = false;

			CalculatePath();

			MoveAlongNetwork();

			newTilePos = worldCenter.ToTileCoordinates16();
			Tile sourceTile = Framing.GetTileSafely(newTilePos);

			if(ModContent.GetModTile(sourceTile.type) is ItemPumpTile pump){
				//Force the move direction to be away from the pump
				moveDir = -(pump.GetBackwardsOffset(newTilePos) - newTilePos).ToVector2();

				if(wander){
					//Get a new path
					needsPathRefresh = true;
					delayPathCalc = false;
				}
			}

			if(!delayPathCalc && (!sourceTile.active() || !itemNetwork.HasEntryAt(newTilePos))){
				Item data = ItemIO.Load(itemData);

				//If the tile is a machine or chest, try to put this item in there
				Chest chest = null;
				MachineEntity entity = null;
				bool sendBack = true;

				if((entity = FindConnectedMachine(itemNetwork, newTilePos, data)) != null || (chest = FindConnectedChest(itemNetwork, newTilePos, data, out _)) != null){
					if(chest is Chest){
						sendBack = !chest.TryInsertItems(data);

						if(sendBack)
							itemData = ItemIO.Save(data);
					}else
						entity.InputItemFromNetwork(this, out sendBack);

					if(!sendBack)
						itemNetwork.RemovePath(this.id);
					else{
						sendBackCounts--;
						
						moveDir *= -1;
						delayPathCalc = true;

						if(sendBackCounts > 0){
							//Send the item back into the network and force it to create a new path
							needsPathRefresh = true;
						}else{
							//Tried and repeatedly failed to get in the same machine.  Wander around
							sendBackCounts = 10;
							wander = true;
						}
					}
				}else{
					if(FindConnectedChest(itemNetwork, newTilePos, data, out _) is null && FindConnectedMachine(itemNetwork, newTilePos, data) is null)
						SpawnInWorld();
					else{
						wander = true;
						moveDir *= -1;
						delayPathCalc = true;
						needsPathRefresh = true;

						worldCenter = itemNetwork.GetEntries().Select(pipe => pipe.Position).OrderBy(p => Vector2.DistanceSquared(p.ToWorldCoordinates(), worldCenter)).First().ToWorldCoordinates();
					}
				}
			}

			if(wander)
				finalTarget = Point16.Zero;
		}

		internal void SpawnInWorld(){
			Item data = ItemIO.Load(itemData);

			//Spawn a fake item, since its data will be overwritten anyway
			int index = Item.NewItem(worldCenter, ItemID.DirtBlock);
			if(index < Main.maxItems){
				Item spawned = Main.item[index];
				Vector2 oldPos = spawned.position;

				Main.item[index] = data;
				data.position = oldPos;
				data.velocity = (moveDir * 2).RotatedByRandom(MathHelper.ToRadians(20));

				if(data.velocity.LengthSquared() > 8 * 8)
					data.velocity = Vector2.Normalize(data.velocity) * 8;

				if(Main.netMode != NetmodeID.SinglePlayer)
					NetMessage.SendData(MessageID.SyncItem, number: index);
			}

			//Item is no longer in the network.  Remove this path and drop the item onto the ground
			itemNetwork.RemovePath(this.id);
		}

		internal static Chest FindConnectedChest(ItemNetwork itemNetwork, Point16 pos, Item incoming, out Point16 location){
			location = default;
			if(!(itemNetwork.pipesConnectedToChests.Contains(pos) || itemNetwork.pipesConnectedToChests.Contains(pos + new Point16(0, -1)) || itemNetwork.pipesConnectedToChests.Contains(pos + new Point16(-1, 0)) || itemNetwork.pipesConnectedToChests.Contains(pos + new Point16(1, 0)) || itemNetwork.pipesConnectedToChests.Contains(pos + new Point16(0, 1))))
				return null;

			int up = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y - 1);
			if(up > -1){
				location = pos + new Point16(0, -1);

				if(!Main.chest[up].IsFull(incoming))
					return Main.chest[up];
			}

			int left = ChestUtils.FindChestByGuessingImproved(pos.X - 1, pos.Y);
			if(left > -1){
				location = pos + new Point16(-1, 0);
				
				if(!Main.chest[left].IsFull(incoming))
					return Main.chest[left];
			}

			int right = ChestUtils.FindChestByGuessingImproved(pos.X + 1, pos.Y);
			if(right > -1){
				location = pos + new Point16(1, 0);
				
				if(!Main.chest[right].IsFull(incoming))
					return Main.chest[right];
			}

			int down = ChestUtils.FindChestByGuessingImproved(pos.X, pos.Y + 1);
			if(down > -1){
				location = pos + new Point16(0, 1);
				
				if(!Main.chest[down].IsFull(incoming))
					return Main.chest[down];
			}

			location = default;
			return null;
		}

		internal static MachineEntity FindConnectedMachine(ItemNetwork network, Point16 location, Item incoming){
			if(TileMachineCanBeInputInto(network, location + new Point16(0, -1), incoming, out MachineEntity entity))
				return entity;
			
			if(TileMachineCanBeInputInto(network, location + new Point16(-1, 0), incoming, out entity))
				return entity;
			
			if(TileMachineCanBeInputInto(network, location + new Point16(1, 0), incoming, out entity))
				return entity;
			
			if(TileMachineCanBeInputInto(network, location + new Point16(0, 1), incoming, out entity))
				return entity;

			return null;
		}

		internal static bool ConnectedToMagicStorageSystem(ItemNetwork network, Point16 location, out Point16 dirPos)
			=> DirConnectedToMagicStorageSystem(network, location, new Point16(0, -1), out dirPos)
				|| DirConnectedToMagicStorageSystem(network, location, new Point16(-1, 0), out dirPos)
				|| DirConnectedToMagicStorageSystem(network, location, new Point16(1, 0), out dirPos)
				|| DirConnectedToMagicStorageSystem(network, location, new Point16(0, 1), out dirPos);

		private static bool DirConnectedToMagicStorageSystem(ItemNetwork network, Point16 location, Point16 dir, out Point16 dirPos){
			dirPos = location + dir;
			return network.HasEntryAt(location) && MagicStorageHandler.HasStorageHeartAt(dirPos) || MagicStorageHandler.HasStorageAccessAt(dirPos) || MagicStorageHandler.HasRemoteStorageAccessAt(dirPos);
		}

		private static bool TileMachineCanBeInputInto(ItemNetwork network, Point16 position, Item incoming, out MachineEntity machineEntity){
			machineEntity = null;
			return network.HasMachineAt(position) && TileEntityUtils.TryFindMachineEntity(position, out machineEntity) && machineEntity.CanBeInput(incoming);
		}

		private void CalculatePath(){
			bool prevWander = wander;

			if(needsPathRefresh && !delayPathCalc){
				needsPathRefresh = false;

				//Find the nearest machine/chest that can have items input into it
				//If no chest or machine could be found, get the path to a pipe with only one connection (the item will be thrown out of there)
				if(itemNetwork.chests.Count == 0 && itemNetwork.pipesConnectedToChests.Count == 0 && itemNetwork.pipesConnectedToMachines.Count == 0)
					wander = true;
				else{
					wander = false;
					
					SetMovementPath();
				}
			}

			if(!prevWander && wander)
				moveDir *= -1;
		}

		internal void SetMovementPath(){
			List<ItemPathResult> pathToTargets = new List<ItemPathResult>();

			var worldTile = worldCenter.ToTileCoordinates16();
			Item item = ItemIO.Load(itemData);

			foreach(var pos in itemNetwork.pipesConnectedToChests){
				Chest chest = FindConnectedChest(itemNetwork, pos, item, out Point16 location);
				if(chest is null)
					continue;

				var path = AStar.SimulateItemNetwork(itemNetwork, worldTile, location, out float travelTime);
				if(path != null)
					pathToTargets.Add(new ItemPathResult(){ time = travelTime, list = path });
			}

			foreach(var pos in itemNetwork.pipesConnectedToMachines){
				//First, check if any of the connected machines can actually have the items in this path object be input into them
				MachineEntity entity = FindConnectedMachine(itemNetwork, pos, item);
				if(entity is null)
					continue;

				//At least one machine can be input into.  Add the pipe to the calculation
				var path = AStar.SimulateItemNetwork(itemNetwork, worldTile, pos, out float travelTime);
				if(path != null)
					pathToTargets.Add(new ItemPathResult(){ time = travelTime, list = path });
			}

			if(pathToTargets.Count == 0){
				wander = true;
				currentPath = null;
			}else{
				//Get the shortest path
				List<Point16> list = pathToTargets.OrderBy(t => t.time).Where(t => t.list != null && t.list.Count > 0).FirstOrDefault().list;

				if((list?.Count ?? 0) == 0){
					wander = true;
					currentPath = null;
					return;
				}

				//The list goes from target -> source
				//We need it to go from source -> target
				list.Reverse();

				//Try to guess the state of the target machine in the future
				//If there are other item paths going to the same machine, account for them before doing anything else
				var last = list.Last();
				var otherPaths = itemNetwork.paths.Where(p => p.currentPath?.Count > 0 && p.currentPath.Last() == last).ToList();

				if(otherPaths.Count > 0){
					if((FindConnectedMachine(itemNetwork, last, item) is MachineEntity entity && !SimulateMachineInput(entity, item, otherPaths)) || (FindConnectedChest(itemNetwork, last, item, out _) is Chest chest && !SimulateChestInput(chest, item, otherPaths))){
						wander = true;
						currentPath = null;
						return;
					}
				}

				currentPath = new Queue<Point16>(list);
				dequeuedPoint = currentPath.Dequeue();
			}
		}

		internal static bool SimulateChestInput(Chest chest, Item incoming, List<ItemNetworkPath> paths){
			//Entries need to be populated or Chest.ToString() throws in the debugger
			Chest simulation = new Chest(){
				item = new Item[chest.item.Length].Populate(() => new Item())
			};

			for(int i = 0; i < chest.item.Length; i++)
				simulation.item[i] = chest.item[i].Clone();

			//Insert the other items
			bool successful = true;
			foreach(var path in paths)
				successful &= simulation.TryInsertItems(ItemIO.Load(path.itemData));

			//Then this one
			successful &= simulation.TryInsertItems(incoming);

			return successful;
		}

		internal static bool SimulateMachineInput(MachineEntity entity, Item incoming, List<ItemNetworkPath> paths){
			if(entity.HijackSimulateInput(incoming, paths, out bool successful))
				return successful;

			var inputs = entity.GetInputSlots();

			if(inputs.Length == 0)
				return false;

			//Entries need to be populated or Chest.ToString() throws in the debugger
			Chest simulation = new Chest(){
				item = new Item[inputs.Length].Populate(() => new Item())
			};

			for(int i = 0; i < simulation.item.Length; i++)
				simulation.item[i] = entity.RetrieveItem(inputs[i]).Clone();

			//Insert the other items
			successful = true;
			foreach(var path in paths){
				var item = ItemIO.Load(path.itemData);

				successful &= entity.CanBeInput(item) && simulation.TryInsertItemsIntoMachineSimulation(item, inputs, (slot, data) => entity.CanInputItem(slot, data));

				if(!successful)
					return false;
			}

			//Then this one
			successful &= entity.CanBeInput(incoming) && simulation.TryInsertItemsIntoMachineSimulation(incoming, inputs, (slot, data) => entity.CanInputItem(slot, data));

			return successful;
		}

		private void MoveAlongNetwork(){
			//Items will move from tile center to tile center
			//The movement speed will be based on the pipe the item is currently inside of
			Point16 worldTile = worldCenter.ToTileCoordinates16();

			//If the item is entering its target chest/machine, movement should just update like normal
			if(enteringChestOrMachine && finalDir != Vector2.Zero){
				float finalMove = 16 * AStar.GetItemMovementProgressFactorAt(dequeuedPoint);

				//Access the machine/chest at the target.  If the next step would put the item in the machine/chest
				//  and the machine/chest has the capacity to do so, let the item move
				Point16 target = worldTile + finalDir.ToPoint16();
				Point16 nextTile = (worldCenter + finalMove * finalDir).ToTileCoordinates16();
				if(target == nextTile){
					int chestID;
					Item data = ItemIO.Load(itemData);

					if((MagicStorageHandler.handler.ModIsActive && ConnectedToMagicStorageSystem(itemNetwork, target, out Point16 msPos) && MagicStorageHandler.TryDepositItem(data, msPos, checkOnly: true, out _))
							|| (TileEntityUtils.TryFindMachineEntity(target, out MachineEntity machine) && machine.CanBeInput(data))
							|| ((chestID = ChestUtils.FindChestByGuessingImproved(target.X, target.Y)) > -1 && !Main.chest[chestID].IsFull(data)))
						moveDir = finalDir;
					else{
						moveDir = Vector2.Zero;

						finalDir = Vector2.Zero;

						wander = true;
					}
				}else
					moveDir = finalDir;

				worldCenter += finalMove * moveDir;

				return;
			}

			//If the tile isn't in the network, bail immediately
			if(!itemNetwork.HasEntryAt(worldTile))
				return;

			float move = 16 * AStar.GetItemMovementProgressFactorAt(worldTile);

			if(!wander){
				//If, for whatever reason, this item isn't moving, make it wander and give it a random direction
				if(moveDir == Vector2.Zero){
					SetRandomMoveDir();

					//Call this method again so that the proper logic can be done
					MoveAlongNetwork();
					return;
				}
				
				Point16 next = default;
				Vector2 currentWorld;
				Vector2 nextWorld;
				if(currentPath.Count > 0){
					//Move towards the next entry in the queue
					next = currentPath.Peek();

					//These two centers will only change once the item goes to the next thing in the queue, so using them for the direction is fine
					currentWorld = dequeuedPoint.ToWorldCoordinates();
					nextWorld = next.ToWorldCoordinates();

					moveDir = Vector2.Normalize(nextWorld - currentWorld);
				}

				worldCenter += move * moveDir;

				Point16 nextMove = (worldCenter + moveDir).ToTileCoordinates16();

				//Get the next point if the item moved past the pipe center
				//  OR if the item's position somehow ended up being not the current point and the next point
				if(MovedPastPipeCenter(out Vector2 overStep) || (nextMove != dequeuedPoint && (currentPath.Count == 0 || nextMove != next))){
					//Snap the tile back to the center, get a new movement direction, then apply the overstep
					worldCenter = worldTile.ToWorldCoordinates();
					
					if(currentPath.Count > 0)
						dequeuedPoint = currentPath.Dequeue();

					//If this is the last point, then the machine or chest will be connected to this pipe (prioritize machines first)
					//Otherwise, there are still more pipes to move through
					if(currentPath.Count == 0){
						enteringChestOrMachine = true;

						SetFinalDir();
					}else{
						next = currentPath.Peek();

						currentWorld = dequeuedPoint.ToWorldCoordinates();
						nextWorld = next.ToWorldCoordinates();

						moveDir = Vector2.Normalize(nextWorld - currentWorld);
					}

					worldCenter += moveDir * overStep.Length();
				}
			}else{
				//Make the item wander around the network since it has no valid target positions
				//Have an equally random chance to move in any direction that wouldn't make the item move backwards
				//If the item, for some reason, has no movement direction, make it a random one and make it wander
				if(moveDir == Vector2.Zero)
					SetRandomMoveDir();

				worldCenter += move * moveDir;

				if(MovedPastPipeCenter(out Vector2 overStep)){
					worldCenter = worldTile.ToWorldCoordinates();

					WeightedRandom<Vector2> wRand = new WeightedRandom<Vector2>(Main.rand);
					if(moveDir.X != 1 && itemNetwork.HasEntryAt(worldTile + new Point16(-1, 0)))
						wRand.Add(new Vector2(-1, 0), 1);
					if(moveDir.X != -1 && itemNetwork.HasEntryAt(worldTile + new Point16(1, 0)))
						wRand.Add(new Vector2(1, 0), 1);
					if(moveDir.Y != 1 && itemNetwork.HasEntryAt(worldTile + new Point16(0, -1)))
						wRand.Add(new Vector2(0, -1), 1);
					if(moveDir.Y != -1 && itemNetwork.HasEntryAt(worldTile + new Point16(0, 1)))
						wRand.Add(new Vector2(0, 1), 1);

					if(wRand.elements.Count > 0)
						moveDir = wRand.Get();

					worldCenter += moveDir * overStep.Length();
				}
			}
		}

		private void SetFinalDir(){
			Point16 up = dequeuedPoint + new Point16(0, -1);
			Point16 left = dequeuedPoint + new Point16(-1, 0);
			Point16 right = dequeuedPoint + new Point16(1, 0);
			Point16 down = dequeuedPoint + new Point16(0, 1);

			Tile tileUp = Framing.GetTileSafely(up);
			Tile tileLeft = Framing.GetTileSafely(left);
			Tile tileRight = Framing.GetTileSafely(right);
			Tile tileDown = Framing.GetTileSafely(down);

			if(HasMachine(up) && tileUp.active())
				finalDir = new Vector2(0, -1);
			else if(HasMachine(left) && tileLeft.active())
				finalDir = new Vector2(-1, 0);
			else if(HasMachine(right) && tileRight.active())
				finalDir = new Vector2(1, 0);
			else if(HasMachine(down) && tileDown.active())
				finalDir = new Vector2(0, 1);
			else if(HasChest(up) && tileUp.active())
				finalDir = new Vector2(0, -1);
			else if(HasChest(left) && tileLeft.active())
				finalDir = new Vector2(-1, 0);
			else if(HasChest(right) && tileRight.active())
				finalDir = new Vector2(1, 0);
			else if(HasChest(down) && tileDown.active())
				finalDir = new Vector2(0, 1);

			finalTarget = dequeuedPoint + finalDir.ToPoint16();
		}

		private bool MovedPastPipeCenter(out Vector2 overStep){
			Vector2 worldTileCenter = worldCenter.ToTileCoordinates16().ToWorldCoordinates();

			if((moveDir.X == -1 && worldCenter.X < worldTileCenter.X && oldCenter.X > worldTileCenter.X)
					|| (moveDir.X == 1 && worldCenter.X > worldTileCenter.X && oldCenter.X < worldTileCenter.X)
					|| (moveDir.Y == -1 && worldCenter.Y < worldTileCenter.Y && oldCenter.Y > worldTileCenter.Y)
					|| (moveDir.Y == 1 && worldCenter.Y > worldTileCenter.Y && oldCenter.Y < worldTileCenter.Y)){
				overStep = worldCenter - worldTileCenter;
				return true;
			}

			overStep = Vector2.Zero;
			return false;
		}

		private bool HasMachine(Point16 position){
			var item = ItemIO.Load(itemData);
			return itemNetwork.HasMachineAt(position) && TileEntityUtils.TryFindMachineEntity(position, out MachineEntity entity) && entity.CanBeInput(item);
		}

		private bool HasChest(Point16 position)
			=> itemNetwork.HasChestAt(position);

		private void SetRandomMoveDir(){
			//Get a random move direction that still keeps this item in the network
			bool valid = false;
			Point16 tileOrig = worldCenter.ToTileCoordinates16();

			int checkCount = 0;
			do{
				switch(Main.rand.Next(4)){
					case 0:
						if(itemNetwork.HasEntryAt(tileOrig + new Point16(0, -1))){
							moveDir = new Vector2(0, -1);
							valid = true;
						}
						break;
					case 1:
						if(itemNetwork.HasEntryAt(tileOrig + new Point16(-1, 0))){
							moveDir = new Vector2(-1, 0);
							valid = true;
						}
						break;
					case 2:
						if(itemNetwork.HasEntryAt(tileOrig + new Point16(1, 0))){
							moveDir = new Vector2(1, 0);
							valid = true;
						}
						break;
					case 3:
						if(itemNetwork.HasEntryAt(tileOrig + new Point16(0, 1))){
							moveDir = new Vector2(0, 1);
							valid = true;
						}
						break;
				}

				//Prevent infinite loops
				checkCount++;
			}while(!valid && checkCount < 100);

			if(checkCount >= 100){
				//Infinite loop happened.  Just set a random direction
				switch(Main.rand.Next(4)){
					case 0:
						moveDir = new Vector2(0, -1);
						break;
					case 1:
						moveDir = new Vector2(-1, 0);
						break;
					case 2:
						moveDir = new Vector2(1, 0);
						break;
					case 3:
						moveDir = new Vector2(0, 1);
						break;
				}
			}

			wander = true;
		}
	}
}
