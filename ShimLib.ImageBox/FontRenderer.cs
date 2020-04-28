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
        
        public static void DrawString(string text, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, Color color) {
            int icolor = color.ToArgb();
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            int x = dx;
            int y = dy;
            foreach (var b in bytes) {
                var fontBuffer = fontBuffers[b];
                if (b == 0x0d) {
                    continue;
                }
                if (b == 0x0a) {
                    x = dx;
                    y += fontBuffer.bh;
                    continue;
                }
                DrawByte(fontBuffer, dispBuf, dispBW, dispBH, x, y, icolor);
                x += fontBuffer.bw;
            }
        }

        private unsafe static void DrawByte(ImageBuffer fbuf, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, int icolor) {
            if (dx < 0 || dy < 0 || dx + fbuf.bw >= dispBW || dy + fbuf.bh >= dispBH)
                return;

            for (int y = 0; y < fbuf.bh; y++) {
                byte* src = (byte*)fbuf.buf + fbuf.bw * y;
                int* dst = (int*)dispBuf + dispBW * (dy + y) + dx;
                for (int x = 0; x < fbuf.bw; x++, src++, dst++) {
                    if (*src == 0) {
                        *dst = icolor;
                    }
                }
            }
        }
    }
}
