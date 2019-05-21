using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryPagesWriter
{
    class GoToLocationMMFWriter : IDisposable
    {

        Mutex Mutex = new Mutex(false, "GoToLocationMutex");

        int messageIndex = 0;

        private const int GotoStructSize = 8;
        private MemoryMappedViewStream stream;
        private MemoryMappedViewAccessor accessor;
        private MemoryMappedFile memoryMappedFile;
        private BinaryWriter writer;

        public GoToLocationMMFWriter()
        {
            memoryMappedFile = MemoryMappedFile.CreateNew("GotoLocationFile", 13, MemoryMappedFileAccess.ReadWrite);
            stream = memoryMappedFile.CreateViewStream();
            accessor = memoryMappedFile.CreateViewAccessor();
            writer = new BinaryWriter(stream);
        }

        public void Write(float x, float y)
        {
            byte[] byteArray = new byte[sizeof(int) + 2*sizeof(float)+1];
            Buffer.BlockCopy(BitConverter.GetBytes(++messageIndex), 0, byteArray, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byteArray, 4, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byteArray, 8, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, byteArray, 12, sizeof(bool));

            Mutex.WaitOne();
            writer.Write(byteArray);
            writer.Flush();
            Mutex.ReleaseMutex();

            writer.Seek(0, SeekOrigin.Begin);

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