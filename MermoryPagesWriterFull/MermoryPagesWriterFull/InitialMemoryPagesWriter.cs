using ECC.Opsu.Gis3D.Contract;
using Interoperability;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryPagesWriterFull
{
    class InitialMemoryPagesWriter : IDisposable
    {
        private int MemoryFileSize;

        Mutex Mutex = new Mutex(false, "MMFMutex");

        private const int LayerProxySize = sizeof(int) * 2 + 256 + 4 * sizeof(byte);
        private const int Gis3DProxySize = sizeof(int) * 2 + (3 * 256) + 3 * sizeof(float);

        private MemoryMappedViewStream stream;
        private MemoryMappedViewAccessor accessor;
        private MemoryMappedFile memoryMappedFile;
        private BinaryWriter writer;

        public InitialMemoryPagesWriter(int memoryFileSize)
        {
            MemoryFileSize = memoryFileSize;
            memoryMappedFile = MemoryMappedFile.CreateNew("InitializationMapFile", memoryFileSize, MemoryMappedFileAccess.ReadWrite);
            stream = memoryMappedFile.CreateViewStream();
            accessor = memoryMappedFile.CreateViewAccessor();
            writer = new BinaryWriter(stream);
        }

        public void Write(Layer[] layers, Gis3DObject[] gis3DObjects)
        {
            var byteArray = GetByteArray(MapToProxy(layers), MapToProxy(gis3DObjects));

            Mutex.WaitOne();
            writer.Write(byteArray);
            writer.Flush();
            Mutex.ReleaseMutex();

            while (true)
            {
                Thread.Sleep(1);
                Mutex.WaitOne();
                byte[] array = new byte[byteArray.Length];
                accessor.ReadArray<byte>(0, array, 0, array.Length);
                var isProcessed = accessor.ReadBoolean(byteArray.Length - 1);
                if (isProcessed)
                {
                    Mutex.ReleaseMutex();
                    return;
                }

                Mutex.ReleaseMutex();
            }
        }

        public byte[] GetByteArray(LayerProxy[] layers, Gis3DObjectProxy[] gis3Ds)
        {
            int messageSize = 12 + LayerProxySize * layers.Length + Gis3DProxySize * gis3Ds.Length + 1;

            byte[] buffer = new byte[messageSize]; //+4 messageSize +2 bool isStructureReady, bool isProcessed

            Buffer.BlockCopy(BitConverter.GetBytes(messageSize), 0, buffer, 0, sizeof(int));

            ToByteArray(layers, LayerProxySize, layers.Length, buffer, sizeof(int));
            ToByteArray(gis3Ds, Gis3DProxySize, gis3Ds.Length, buffer, LayerProxySize * layers.Length + 2 * sizeof(int));
            
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, buffer, messageSize-1, sizeof(bool)); //Message processed

            return buffer;
        }

        private static void ToByteArray<T>(T[] array, int unitSize, int count, byte[] buffer, int offset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(count), 0, buffer, offset, sizeof(int));

            IntPtr memoryPointer = Marshal.AllocHGlobal(unitSize*count);

            int i = 0;
            foreach (var unit in array)
            {
                Marshal.StructureToPtr(unit, memoryPointer + unitSize * i++, false);
            }

            Marshal.Copy(memoryPointer, buffer, offset+sizeof(int), unitSize * count);
            Marshal.FreeHGlobal(memoryPointer);
        }

        private static LayerProxy[] MapToProxy(Layer[] originalArray)
        {
            var result = new LayerProxy[originalArray.Length];

            int i = 0;
            foreach(var original in originalArray)
            {
                result[i++] = new LayerProxy()
                {
                    Id = original.Id,
                    Name = original.Name,
                    ParentId = original.ParentId ?? -1,
                    R = original.Color.R,
                    G = original.Color.G,
                    B = original.Color.B,
                    A = original.Color.A
                };
            }

            return result;
        }

        private static Gis3DObjectProxy[] MapToProxy(Gis3DObject[] originalArray)
        {
            var result = new Gis3DObjectProxy[originalArray.Length];

            int i = 0;

            foreach (var original in originalArray)
            {
                result[i++] = new Gis3DObjectProxy()
                {
                    Id = original.Id,
                    Description = original.Description,
                    Name = original.Name,
                    ShortName = original.ShortName,
                    LayerId = original.LayerId,
                    X = original.X,
                    Y = original.Y,
                    Height = original.Height
                };
            }

            return result;
        }

        public void Dispose()
        {
            writer.Close();
            writer.Dispose();
            stream.Close();
            stream.Dispose();
            memoryMappedFile.Dispose();
        }
    }
}
