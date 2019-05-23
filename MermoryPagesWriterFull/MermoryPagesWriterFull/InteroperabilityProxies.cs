using System.Runtime.InteropServices;

namespace Interoperability
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct LayerProxy
    {
        public int Id;

        //nullable
        public int ParentId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;

        public byte R;

        public byte G;

        public byte B;

        public byte A;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct Gis3DObjectProxy
    {
        public int Id;
        public int LayerId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ShortName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;

        public float X;
        public float Y;
        public float Height;
    }
}
