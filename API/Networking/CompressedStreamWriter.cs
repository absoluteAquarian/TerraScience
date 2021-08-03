using Ionic.Zlib;
using System;
using System.IO;

namespace TerraScience.API.Networking{
	public class CompressedStreamWriter : IDisposable{
		private MemoryStream ms;
		private DeflateStream compressor;
		private BufferedStream compressedBuffer;
		private BinaryWriter compressedWriter;

		public readonly int capacity;
		private bool initialized;

		public MemoryStream Memory => ms;

		public BinaryWriter Writer => compressedWriter;

		public CompressedStreamWriter(int capacity){
			//Postpone creating the streams until the object is actually used
			this.capacity = capacity;

			if(capacity <= 0)
				throw new ArgumentException("Stream capacity was too small: " + capacity);
		}

		private void CheckInitialized(){
			if(initialized)
				return;

			ms = new MemoryStream(capacity);
			compressor = new DeflateStream(ms, CompressionMode.Compress, leaveOpen: true);
			compressedBuffer = new BufferedStream(compressor, capacity);
			compressedWriter = new BinaryWriter(compressedBuffer);

			initialized = true;
		}

		public void Write(bool value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(sbyte value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(byte value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(short value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(ushort value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(int value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(uint value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(long value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(ulong value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(float value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(double value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(decimal value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(char value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(string value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(byte[] value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(char[] value){
			CheckInitialized();

			compressedWriter.Write(value);
		}

		public void Write(byte[] buffer, int index, int count){
			CheckInitialized();

			compressedWriter.Write(buffer, index, count);
		}

		public void Write(char[] chars, int index, int count){
			CheckInitialized();

			compressedWriter.Write(chars, index, count);
		}

		public void WriteCompressedStream(BinaryWriter writer){
			Flush();

			if(ms.Length <= ushort.MaxValue)
				writer.Write((ushort)ms.Length);
			else
				writer.Write(ms.Length);

			writer.Write(ms.ToArray());
		}

		public void Flush(){
			compressedWriter.Flush();
			compressedBuffer.Flush();
			compressor.Flush();
		}

		public void Close(){
			compressedWriter.Close();
			compressedBuffer.Close();
			compressor.Close();
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
					compressedWriter.Dispose();
					compressedBuffer.Dispose();
					compressor.Dispose();
					ms.Dispose();
				}

				ms = null;
				compressor = null;
				compressedBuffer = null;
				compressedWriter = null;
			}
		}

		~CompressedStreamWriter() => Dispose(false);
	}
}
