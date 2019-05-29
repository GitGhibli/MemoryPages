using ECC.Opsu.Gis3D.Contract;
using Interoperability;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryPagesWriterFull
{
    class InitialMessageSender
    {
        const string fileName = "D:\\Temp\\InitializationFile";
        private int MemoryFileSize;

        Mutex Mutex = new Mutex(false, "MMFMutex");

        private int LayerProxySize = Marshal.SizeOf<LayerProxy>();
        private int Gis3DProxySize = Marshal.SizeOf<Gis3DObjectProxy>();

        private void Write(string file, byte[] byteArray)
        {
            Mutex.WaitOne();
            using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                stream.Write(byteArray, 0, byteArray.Length);
            }
            Mutex.ReleaseMutex();
        }

        public void Send(Layer[] layers, Gis3DObject[] gis3DObjects)
        {
            var byteArray = GetByteArray(MapToProxy(layers), MapToProxy(gis3DObjects));

            Write(fileName, byteArray);

            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {

                while (true)
                {
                    Thread.Sleep(1);
                    Mutex.WaitOne();
                    file.Seek(byteArray.Length - sizeof(bool), SeekOrigin.Begin);
                    var isProcessed = reader.ReadBoolean();
                    if (isProcessed)
                    {
                        Mutex.ReleaseMutex();
                        break;
                    }

                    Mutex.ReleaseMutex();
                }
            }

            File.Delete(fileName);
        }

        public byte[] GetByteArray(LayerProxy[] layers, Gis3DObjectProxy[] gis3Ds)
        {
            int messageSize = 12 + LayerProxySize * layers.Length + Gis3DProxySize * gis3Ds.Length + 1;

            byte[] buffer = new byte[messageSize]; //+4 messageSize +2 bool isStructureReady, bool isProcessed

            Buffer.BlockCopy(BitConverter.GetBytes(messageSize), 0, buffer, 0, sizeof(int));

            ToByteArray(layers, LayerProxySize, layers.Length, buffer, sizeof(int));
            ToByteArray(gis3Ds, Gis3DProxySize, gis3Ds.Length, buffer, LayerProxySize * layers.Length + 2 * sizeof(int));

            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, buffer, messageSize - 1, sizeof(bool)); //Message processed

            return buffer;
        }

        private static void ToByteArray<T>(T[] array, int unitSize, int count, byte[] buffer, int offset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(count), 0, buffer, offset, sizeof(int));

            IntPtr memoryPointer = Marshal.AllocHGlobal(unitSize * count);

            int i = 0;
            foreach (var unit in array)
            {
                Marshal.StructureToPtr(unit, memoryPointer + unitSize * i++, false);
            }

            Marshal.Copy(memoryPointer, buffer, offset + sizeof(int), unitSize * count);
            Marshal.FreeHGlobal(memoryPointer);
        }

        private static LayerProxy[] MapToProxy(Layer[] originalArray)
        {
            var result = new LayerProxy[originalArray.Length];

            int i = 0;
            foreach (var original in originalArray)
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
    }
}
