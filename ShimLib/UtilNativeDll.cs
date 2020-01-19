using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class UtilNativeDll {
        const string dll = "utilnative.dll";
        [DllImport(dll)] public unsafe static extern void CopyImageBufferZoom(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor, bool useParallel);
        [DllImport(dll)] public unsafe static extern void CopyImageBufferZoomIpl(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor, bool useParallel);
    }
}
