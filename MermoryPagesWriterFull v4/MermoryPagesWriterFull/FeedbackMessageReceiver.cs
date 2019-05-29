using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MemoryPagesWriterFull
{
    public class FeedbackMessageReceiver
    {

        string fileName = EnvironmentSettings.GetSharedAbsolutePath("FeedbackFile");
        private readonly object Lock;
        private static int MessageIndex = 0;
        private const int messageLength = 1024;

        public FeedbackMessageReceiver(object lockObj)
        {
            Lock = lockObj;
        }

        public void Listen()
        {
            Thread listener = new Thread(ReadSharedMemory);
            listener.Start();
        }

        private void ReadSharedMemory(object obj)
        {
            using (var stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                Mutex mutex = null;

                if (!Mutex.TryOpenExisting("FeedbackMutex", out mutex))
                {
                    mutex = new Mutex(false, "FeedbackMutex");
                }

                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(true); //indicates no new messages;

                    using (var reader = new BinaryReader(stream, Encoding.Unicode))
                    {
                        while (true)
                        {
                            if (mutex.WaitOne(1))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                var isProcessed = reader.ReadBoolean();

                                if (!isProcessed)
                                {
                                    var message = reader.ReadChars(messageLength);
                                    string result = new string(message).TrimEnd('\0');
                                    Console.WriteLine(result);
                                    stream.Seek(0, SeekOrigin.Begin);
                                    writer.Write(true);
                                    writer.Flush();
                                }

                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }
            }
        }
    }
}
