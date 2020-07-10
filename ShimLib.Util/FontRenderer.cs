using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class FontRenderer {
        private IntPtr fontBuf;
        private int fontBw;
        private int fontBh;
        private int fw;
        private int fh;
        public FontRenderer(Bitmap bmp, int _fw, int _fh) {
            int bytepp = 0;
            ImageUtil.BitmapToImageBuffer(bmp, ref fontBuf, ref fontBw, ref fontBh, ref bytepp);
            fw = _fw;
            fh = _fh;
        }
        
        ~FontRenderer() {
            Util.FreeBuffer(ref fontBuf);
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
                    y += fh;
                    continue;
                }
                if (ch >= 32 && ch <= 128) {
                    int fontX = (ch - 32) * fw;
                    int fontImgY = fontX / fontBw;
                    int fontImgX = fontX % fontBw;
                    DrawChar(fontImgX, fontImgY, dispBuf, dispBW, dispBH, x, y, icolor);
                }
                x += fw;
            }
        }

        public Size MeasureString(string text) {
            int maxX = 0;
            int maxY = 0;
            int x = 0;
            int y = 0;
            foreach (char ch in text) {
                if (ch == '\r') {
                    continue;
                }
                if (ch == '\n') {
                    x = 0;
                    y += fh;
                    continue;
                }
                maxX = Math.Max(maxX, x + fw);
                maxY = Math.Max(maxY, y + fh);
                x += fw;
            }

            return new Size(maxX, maxY);
        }

        private unsafe void DrawChar(int fontImgX, int fontImgY, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, int icolor) {
            int x1 = dx;
            int y1 = dy;
            int x2 = dx + fw - 1;
            int y2 = dy + fh - 1;
            if (x1 >= dispBW || x2 < 0 || y1 >= dispBH || y2 < 0)
                return;

            for (int y = 0; y < fh; y++) {
                if (dy + y < 0 || dy + y >= dispBH)
                    continue;
                int* dst = (int*)dispBuf + dispBW * (dy + y) + dx;
                byte* src = (byte*)fontBuf + fontBw * (fontImgY + y) + fontImgX;
                for (int x = 0; x < fw; x++, src++, dst++) {
                    if (dx + x < 0 || dx + x >= dispBW)
                        continue;
                    if (*src == 0) {
                        *dst = icolor;
                    }
                }
            }
        }
    }
}
