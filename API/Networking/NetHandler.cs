using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pipes;

namespace TerraScience.API.Networking {
	public static class NetHandler{
		/// <summary>
		/// Called whenever a net message / packet is received from a client (if this is a server) or the server (if this is a client). whoAmI is the ID of whomever sent the packet (equivalent to the Main.myPlayer of the sender), and reader is used to read the binary data of the packet.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="sender">The player the message is from.</param>
		public static void HandlePacket(BinaryReader reader, int sender){
			Message message = (Message)reader.ReadByte();

			// TODO: finish the rest of these messages
			switch(message){
				case Message.SyncTileEntity:
					ReceiveSyncTileEntity(reader);
					break;
				case Message.SyncNetworkCreation:

					break;
			}
		}

		public static void SendSyncTileEntity(TileEntity entity){
			ModPacket packet = TechMod.Instance.GetPacket();
			packet.Write((byte)Message.SyncTileEntity);

			packet.Write(entity.ID);

			bool exists = TileEntity.ByID.ContainsKey(entity.ID);
			packet.Write(exists);

			if(exists){
				//Set up a compressed stream
				CompressedStreamWriter compressedWriter = new CompressedStreamWriter(65536);

				TileEntity.Write(compressedWriter.Writer, entity);

				compressedWriter.WriteCompressedStream(packet);

				compressedWriter.Dispose();
			}

			packet.Send();
		}

		private static void ReceiveSyncTileEntity(BinaryReader reader){
			int entity = reader.ReadInt32();
			bool deleted = reader.ReadBoolean();

			if(deleted){
				if(TileEntity.ByID.TryGetValue(entity, out var existing)){
					TileEntity.ByID.Remove(entity);
					TileEntity.ByPosition.Remove(existing.Position);
				}

				return;
			}

			//Undo the compression
			CompressedStreamReader compressedReader = new CompressedStreamReader(65536);

			compressedReader.Initialize(reader);

			TileEntity.Read(compressedReader.Reader);

			compressedReader.Dispose();
		}

		public static void SendSyncNetworkCreation(Point16 orig){
			bool wire = NetworkCollection.HasWireAt(orig, out WireNetwork wireNet);
			bool item = NetworkCollection.HasItemPipeAt(orig, out ItemNetwork itemNet);
			bool fluid = NetworkCollection.HasFluidPipeAt(orig, out FluidNetwork fluidNet);

			if(!(wire || item || fluid))
				return;

			ModPacket packet = TechMod.Instance.GetPacket();
			packet.Write((byte)Message.SyncNetworkCreation);

			int netType = wire ? 0 : (item ? 1 : 2);

			packet.Write((byte)netType);

			int id = wireNet?.ID ?? itemNet?.ID ?? fluidNet.ID;

			packet.Write(id);

			Point16 entryOrig = wireNet?.GetEntries()[0].Position ?? itemNet?.GetEntries()[0].Position ?? fluidNet.GetEntries()[0].Position;

			packet.Write(entryOrig.X);
			packet.Write(entryOrig.Y);

			// TODO: finish this
		}
	}
}
