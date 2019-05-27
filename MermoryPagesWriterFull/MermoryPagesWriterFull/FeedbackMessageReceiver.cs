using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryPagesWriterFull
{
    public class FeedbackMessageReceiver
    {
        private readonly object Lock;
        private static int MessageIndex = 0;

        public FeedbackMessageReceiver(object lockObj)
        {
            Lock = lockObj;
        }

        public void Listen()
        {
            Thread listener = new Thread(ReadSharedMemory);
            listener.IsBackground = true;
            listener.Start();
        }

        private void ReadSharedMemory(object obj)
        {
            using (var reader = MemoryMappedFile.CreateOrOpen("FeedbackFile", 4 + 1024 * 2 + 1).CreateViewAccessor())
            {
                Mutex mutex = null;

                if (!Mutex.TryOpenExisting("FeedbackMutex", out mutex))
                {
                    mutex = new Mutex(false, "FeedbackMutex");
                }

                while (true)
                {
                    if (mutex.WaitOne(1))
                    {
                        byte[] buffer = new byte[4 + 1024 * 2 + 1];
                        reader.ReadArray(0, buffer, 0, buffer.Length);
                        reader.Read(0, out int messageIndex);

                        if (messageIndex > MessageIndex)
                        {
                            char[] messageArray = new char[1024];
                            reader.ReadArray<char>(4, messageArray, 0, 1024);
                            string result = new string(messageArray).TrimEnd('\0');
                            lock (Lock)
                            {
                                Console.WriteLine(result);
                            }
                            MessageIndex++;
                        }

                        mutex.ReleaseMutex();
                    }
                }
            }
        }
    }
}
