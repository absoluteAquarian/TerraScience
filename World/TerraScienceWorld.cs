using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;
using TerraScience.API.CrossMod.MagicStorage;
using TerraScience.API.Interfaces;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.Tiles;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.World{
	public class TerraScienceWorld : ModSystem {
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight){
			CustomWorldGen.MoreShinies(tasks);
		}

		public override void SaveWorldData(TagCompound tag)	{
			["networks"] = NetworkCollection.Save(),
			["mufflers"] = MachineMufflerTile.mufflers
		};

		public override void LoadWorldData(TagCompound tag){
			NetworkCollection.EnsureNetworkIsInitialized();

			if(tag.ContainsKey("networks"))
				NetworkCollection.Load(tag.GetCompound("networks"));

			if(tag.GetList<Point16>("mufflers") is List<Point16> list)
				MachineMufflerTile.mufflers = list;
		}

		public override void NetSend(BinaryWriter writer){
			TagIO.Write(SaveWorldData(new TagCompound()), writer);
		}

		public override void NetReceive(BinaryReader reader){
			LoadWorldData(TagIO.Read(reader));
		}

		public override void OnWorldLoad(){
			NetworkCollection.Unload();
			NetworkCollection.EnsureNetworkIsInitialized();
		}

		public override void PreUpdateWorld(){
			NetworkCollection.EnsureNetworkIsInitialized();
		}

		public override void PostUpdateWorld(){
			NetworkCollection.CleanupNetworks();
		}

		private void DrawNetTiles<TEntry>(List<TEntry> entries, Color drawColor) where TEntry : struct, INetworkable, INetworkable<TEntry>{
			var texture = ModContent.Request<Texture2D>("TerraScience/Content/Items/FakeCapsuleFluidItem");

			Rectangle screen = new Rectangle((int)Main.screenPosition.X - 4, (int)Main.screenPosition.Y - 4, (int)(Main.screenWidth * Main.GameZoomTarget) + 8, (int)(Main.screenHeight * Main.GameZoomTarget) + 8);
			foreach(var wire in entries){
				Vector2 screenPos = wire.Position.ToWorldCoordinates(0, 0);

				Rectangle tile = new Rectangle((int)screenPos.X, (int)screenPos.Y, 16, 16);
				if(!tile.Intersects(screen))
					continue;

				screenPos -= Main.screenPosition;
				
				Main.spriteBatch.Draw(texture.Value, screenPos, null, drawColor);
			}
		}

		public static void DrawItemInPipe(ItemNetworkPath path, SpriteBatch spriteBatch){
			//We need to emulate drawing items in the world, but shrinked into the bounds of a pipe
			Item item = ItemIO.Load(path.itemData);

			var offset = MiscUtils.GetLightingDrawOffset();

			spriteBatch.DrawItemInWorld(item, path.worldCenter - Main.screenPosition + offset, new Vector2(3.85f * 2));
		}

		public override void PostDrawTiles(){
			bool began = false;

			//Draw any junction stuff
			if(Main.LocalPlayer.HeldItem.ModItem is TransportJunctionItem){
				if(!string.IsNullOrWhiteSpace(TransportJunctionItem.display) && TransportJunctionItem.displayTimer >= 0){
					Vector2 measure = FontAssets.MouseText.Value.MeasureString(TransportJunctionItem.display);
					Vector2 position = Main.LocalPlayer.Top - new Vector2(0, 20) - Main.screenPosition;

					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
					began = true;

					Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
						TransportJunctionItem.display,
						position.X,
						position.Y,
						Color.Yellow,
						Color.Black,
						measure / 2f);
				}

				if(TransportJunctionItem.displayTimer >= 0)
					TransportJunctionItem.displayTimer--;
			}else{
				TransportJunctionItem.display = null;
				TransportJunctionItem.displayTimer = -1;
			}

			if(!TechMod.debugging){
				if(began)
					Main.spriteBatch.End();

				return;
			}

			if(Main.LocalPlayer.HeldItem.type != ModContent.ItemType<DebugTool>() || Main.LocalPlayer.inventory[58].type == ModContent.ItemType<DebugTool>()){
				if(began)
					Main.spriteBatch.End();

				return;
			}

			Point16 mouse = Main.MouseWorld.ToTileCoordinates16();

			bool hasWire = NetworkCollection.HasWireAt(mouse, out WireNetwork wireNet);
			bool hasItem = NetworkCollection.HasItemPipeAt(mouse, out ItemNetwork itemNet);
			bool hasFluid = NetworkCollection.HasFluidPipeAt(mouse, out FluidNetwork fluidNet);
			if(!hasWire && !hasItem && !hasFluid){
				if(began)
					Main.spriteBatch.End();

				return;
			}

			if(!began)
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

			//For every wire on-screen in the network the player's mouse is hovering over, draw an indicator
			if(hasWire)
				DrawNetTiles(wireNet.GetEntries(), Color.Blue * 0.45f);
			if(hasItem)
				DrawNetTiles(itemNet.GetEntries(), Color.Green * 0.45f);
			if(hasFluid)
				DrawNetTiles(fluidNet.GetEntries(), Color.Red * 0.45f);

			//Then draw what network is being targeted
			Vector2 offset = Main.MouseScreen + new Vector2(20, 20);
			bool hasOffset = false;

			if(wireNet != null){
				hasOffset = true;

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Targeting Wire Network (ID: {wireNet.ID})",
					offset.X,
					offset.Y,
					Color.Blue,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Stored TF: {(float)wireNet.StoredFlux :0.##} / {(float)wireNet.Capacity :0.##} TF",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Exported Flux: {(float)wireNet.totalExportedFlux :0.##} TF/t ({(float)wireNet.totalExportedFlux * 60 :0.##} TF/s)",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);
			}
			if(itemNet != null){
				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Targeting Item Network (ID: {itemNet.ID})",
					offset.X,
					!hasOffset ? offset.Y : (offset.Y += 20),
					Color.Green,
					Color.Black,
					Vector2.Zero);

				hasOffset = true;

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Item Stacks in Network: {itemNet.paths.Count}",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Connected Chests: {itemNet.chests.Count}",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Pipes Connected to Chests: {itemNet.pipesConnectedToChests.Count}",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Pipes Connected to Machines: {itemNet.pipesConnectedToMachines.Count}",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);
			}
			if(fluidNet != null){
				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Targeting Fluid Network (ID: {fluidNet.ID})",
					offset.X,
					!hasOffset ? offset.Y : (offset.Y += 20),
					Color.Red,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Fluid Type: {(fluidNet.fluidType.ProperEnumName())}",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value,
					$"Stored Fluid: {fluidNet.StoredFluid :0.##} / {fluidNet.Capacity :0.##} L",
					offset.X,
					offset.Y += 20,
					Color.White,
					Color.Black,
					Vector2.Zero);
			}

			Main.spriteBatch.End();
		}
	}
}
