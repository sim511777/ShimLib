using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class FontRenderer {
        static ImageBuffer[] fontBuffers;
        static unsafe FontRenderer() {
            fontBuffers = new ImageBuffer[256];
            using (ImageBuffer fontImage = new ImageBuffer(Properties.Resources.tom_thumb_new)) {
                int bw = 4;
                int bh = 6;
                int bytepp = 1;
                for (int i = 0; i < 256; i++) {
                    IntPtr buf = Marshal.AllocHGlobal(bw * bytepp * bh);
                    fontBuffers[i] = new ImageBuffer(buf, bw, bh, bytepp, true);
                    for (int y = 0; y < bh; y++) {
                        IntPtr src = fontImage.buf + fontImage.bw * y + bw * i;
                        IntPtr dst = buf + bw * y;
                        Util.Memcpy4(dst, src, 1);
                    }
                }
            }
        }
        
        public static void DrawString(IntPtr dispBuf, int bw, int bh, string text, int x, int y, Color color) {
            int icolor = color.ToArgb();
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            foreach (var b in bytes) {
                var fontBuffer = fontBuffers[b];
                DrawByte(dispBuf, bw, bh, x, y, fontBuffer, icolor);
                x += fontBuffer.bw;
            }
        }

        private unsafe static void DrawByte(IntPtr dispBuf, int bw, int bh, int dx, int dy, ImageBuffer fbuf, int icolor) {
            if (dx < 0 || dy < 0 || dx + fbuf.bw >= bw || dy + fbuf.bh >= bh)
                return;
            for (int y = 0; y < fbuf.bh; y++) {
                byte* src = (byte*)fbuf.buf + fbuf.bw * y;
                int* dst = (int*)dispBuf + bw * (dy + y) + dx;
                for (int x = 0; x < fbuf.bw; x++, src++, dst++) {
                    if (*src == 0) {
                        *dst = icolor;
                    }
                }
            }
        }
    }
}
