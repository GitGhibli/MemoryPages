using System;
using System.IO;
using System.Threading;

namespace MemoryPagesWriterFull
{
    class GoToLocationMMFWriter : IDisposable
    {
        private FileStream MyFile = File.Open("D:\\Temp\\GotoLocationFile", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        private BinaryReader Reader;

        public GoToLocationMMFWriter()
        {
            Reader = new BinaryReader(MyFile);
        }

        Mutex Mutex = new Mutex(false, "GoToLocationMutex");

        public void Write(float x, float y)
        {
            byte[] byteArray = new byte[sizeof(int) + 2 * sizeof(float) + 1];
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, byteArray, 0, sizeof(bool));
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, byteArray, 1, sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, byteArray, 5, sizeof(float));

            Mutex.WaitOne();
            MyFile.Seek(0, SeekOrigin.Begin);
            MyFile.Write(byteArray, 0, byteArray.Length);
            MyFile.Flush();
            Mutex.ReleaseMutex();

            while (true)
            {
                //tohle bych rekl, ze je zbytecne
                Thread.Sleep(1);
                Mutex.WaitOne();
                Reader.BaseStream.Seek(0, SeekOrigin.Begin);

                var isProcessed = Reader.ReadBoolean();
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
            Mutex.Close();
            Reader.Close();
        }
    }
}