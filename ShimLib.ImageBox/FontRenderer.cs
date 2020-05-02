using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class FontRenderer {
        private ImageBuffer fontImage;
        private int fontBW;
        private int fontBH;
        public FontRenderer(Bitmap bmp, int _fontBW, int _fontBH) {
            fontImage = new ImageBuffer(bmp);
            fontBW = _fontBW;
            fontBH = _fontBH;
        }
        
        public void DrawString(string text, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, Color color) {
            int icolor = color.ToArgb();
            int x = dx;
            int y = dy;
            foreach (char ch in text) {
                if (ch == '\r') {
                    continue;
                }
                if (ch == '\n') {
                    x = dx;
                    y += fontBH;
                    continue;
                }
                if (ch >= '0' && ch <= '9') {
                    int fontX = ('9' - '0') * fontBW;
                    DrawChar(fontX, dispBuf, dispBW, dispBH, x, y, icolor);
                }
                x += fontBW;
            }
        }

        private unsafe void DrawChar(int fontX, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, int icolor) {
            if (dx < 0 || dy < 0 || dx + fontBW >= dispBW || dy + fontBH >= dispBH)
                return;

            for (int y = 0; y < fontBH; y++) {
                byte* src = (byte*)fontImage.buf + fontImage.bw * y + fontX;
                int* dst = (int*)dispBuf + dispBW * (dy + y) + dx;
                for (int x = 0; x < fontBW; x++, src++, dst++) {
                    if (*src == 0) {
                        *dst = icolor;
                    }
                }
            }
        }
    }
}
