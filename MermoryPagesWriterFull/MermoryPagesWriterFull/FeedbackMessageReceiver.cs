using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryPagesWriterFull
{
    public class FeedbackMessageReceiver
    {
        public static bool Run = true;
        private readonly object Lock;
        
        public FeedbackMessageReceiver(object lockObj)
        {
            Lock = lockObj;
        }

        public Thread Listen()
        {
            Thread listener = new Thread(ReadSharedMemory);
            listener.IsBackground = true;
            listener.Start();
            return listener;
        }

        private void ReadSharedMemory(object obj)
        {
            using (var accessor = MemoryMappedFile.CreateOrOpen("FeedbackFile", 1 + 1024 * 2).CreateViewAccessor())
            {
                Mutex mutex = null;

                if (!Mutex.TryOpenExisting("FeedbackMutex", out mutex))
                {
                    mutex = new Mutex(false, "FeedbackMutex");
                }

                while (Run)
                {
                    if (mutex.WaitOne(1))
                    {
                        var isProcessed = accessor.ReadBoolean(0);

                        if (!isProcessed)
                        {
                            char[] messageArray = new char[1024];
                            accessor.ReadArray<char>(1, messageArray, 0, 1024);
                            string result = new string(messageArray).TrimEnd('\0');
                            lock (Lock)
                            {
                                Console.WriteLine(result);
                            }
                            accessor.Write(0, true);
                        }
                        mutex.ReleaseMutex();
                    }
                }
            }
        }
    }
}
