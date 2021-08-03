using Ionic.Zlib;
using System;
using System.IO;

namespace TerraScience.API.Networking{
	public class CompressedStreamReader : IDisposable{
		private MemoryStream ms;
		private BinaryWriter compressedWriter;
		private DeflateStream decompressor;
		private BinaryReader decompressedReader;

		public readonly int capacity;

		private bool initialized;

		public MemoryStream Memory => ms;

		public BinaryReader Reader => decompressedReader;

		public CompressedStreamReader(int capacity){
			this.capacity = capacity;
		}

		public void Initialize(BinaryReader source){
			if(initialized)
				return;

			ms = new MemoryStream(capacity);
			compressedWriter = new BinaryWriter(ms);

			compressedWriter.Write(source.ReadBytes(source.ReadInt32()));
			ms.Position = 0;

			decompressor = new DeflateStream(ms, CompressionMode.Decompress, leaveOpen: true);
			decompressedReader = new BinaryReader(decompressor);

			initialized = true;
		}

		private void CheckInitialized(){
			if(!initialized)
				throw new InvalidOperationException("Reader has not been initialized");
		}

		public bool ReadBoolean(){
			CheckInitialized();

			return decompressedReader.ReadBoolean();
		}

		public sbyte ReadSByte(){
			CheckInitialized();

			return decompressedReader.ReadSByte();
		}

		public byte ReadByte(){
			CheckInitialized();

			return decompressedReader.ReadByte();
		}

		public short ReadInt16(){
			CheckInitialized();

			return decompressedReader.ReadInt16();
		}

		public ushort ReadUInt16(){
			CheckInitialized();

			return decompressedReader.ReadUInt16();
		}

		public int ReadInt32(){
			CheckInitialized();

			return decompressedReader.ReadInt32();
		}

		public uint ReadUInt32(){
			CheckInitialized();

			return decompressedReader.ReadUInt32();
		}

		public long ReadInt64(){
			CheckInitialized();

			return decompressedReader.ReadInt64();
		}

		public ulong ReadUInt64(){
			CheckInitialized();

			return decompressedReader.ReadUInt64();
		}

		public float ReadSingle(){
			CheckInitialized();

			return decompressedReader.ReadSingle();
		}

		public double ReadDouble(){
			CheckInitialized();

			return decompressedReader.ReadDouble();
		}

		public decimal ReadDecimal(){
			CheckInitialized();

			return decompressedReader.ReadInt16();
		}

		public char ReadChar(){
			CheckInitialized();

			return decompressedReader.ReadChar();
		}

		public string ReadString(){
			CheckInitialized();

			return decompressedReader.ReadString();
		}

		public int Read(){
			CheckInitialized();

			return decompressedReader.Read();
		}

		public int Read(byte[] buffer, int index, int count){
			CheckInitialized();

			return decompressedReader.Read(buffer, index, count);
		}

		public int Read(char[] chars, int index, int count){
			CheckInitialized();

			return decompressedReader.Read(chars, index, count);
		}

		public int PeekChar(){
			CheckInitialized();

			return decompressedReader.PeekChar();
		}

		public void Close(){
			decompressedReader.Close();
			decompressor.Close();
			compressedWriter.Close();
			ms.Close();
		}

		private bool disposed;

		public void Dispose(){
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing){
			if(!disposed){
				disposed = true;

				if(disposing){
					decompressedReader.Dispose();
					decompressor.Dispose();
					compressedWriter.Dispose();
					ms.Dispose();
				}

				ms = null;
				compressedWriter = null;
				decompressor = null;
				decompressedReader = null;
			}
		}

		~CompressedStreamReader() => Dispose(false);
	}
}
