using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HexLight.Util
{
    class StructInterop
    {
        public static byte[] StructToByteArray<T>(T struc)
        {
            int size = Marshal.SizeOf(typeof(T));

            // Convert struct to byte array
            byte[] bytes = new byte[size];
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(struc, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return bytes;
        }

        public static T ByteArrayToStruct<T>(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(T));

            // Convert to target struct
            // Note that it does not copy the data, it assigns the struct to the same memory as the byte array.
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                return stuff;
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
