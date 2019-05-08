using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryPagesWriter
{
    //slo by najprv poslat info o tom, co sa bude posielat ak to ma byt dynamicke
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct MyStruct
    {
        public int MyNumber;

        public int MyAnotherNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string MyText;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string MyAnotherText;

    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var virtualMappedFile = MemoryMappedFile.CreateNew("MyTestMapFile", 10000))
            {
                bool mutexCreated;
                
                using (MemoryMappedViewStream stream = virtualMappedFile.CreateViewStream())
                {

                    BinaryWriter writer = new BinaryWriter(stream);

                    int index = 0;
                    while (true)
                    {
                        writer.Seek(0, SeekOrigin.Begin);
                        Console.ReadKey();
                        //C++ ignores this
                        //Mutex mutex = new Mutex(true, "myTestMapFileMutext", out mutexCreated);
                        writer.Write(GetByteArray(new MyStruct { MyNumber = index++, MyAnotherNumber = 9027, MyText = "Hello World!" + index, MyAnotherText = "Hello another World!" + index }));
                        writer.Flush();

                        //C++ ignores this
                        //mutex.ReleaseMutex();
                    }

                }
            }
        }

        static byte[] GetByteArray(MyStruct myStruct)
        {
            int size = 255 + 255 + 8;
            byte[] buffer = new byte[size];

            IntPtr memoryPointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(myStruct, memoryPointer, true);
            Marshal.Copy(memoryPointer, buffer, 0, size);
            Marshal.FreeHGlobal(memoryPointer);

            return buffer;
        }
    }
}
