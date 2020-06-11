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
        private bool isAscii;
        private bool[] charHalfs;
        public FontRenderer(Bitmap bmp, int _fontBW, int _fontBH, bool _isAscii) {
            fontImage = new ImageBuffer(bmp);
            fontBW = _fontBW;
            fontBH = _fontBH;
            isAscii = _isAscii;
            if (!isAscii) {
                GetCharHalfs();
            }
        }

        private unsafe void GetCharHalfs() {
            charHalfs = new bool[65536];
            for (int ch = 0; ch < 65536; ch++) {
                int fontX = ch * fontBW;
                int fontImgY = fontX / fontImage.bw;
                int fontImgX = fontX % fontImage.bw + fontBW / 2;
                bool hasBlackPixel = false;
                for (int y = 0; y < fontBH; y++) {
                    byte* ptr = (byte*)fontImage.buf + (fontImgY + y) * fontImage.bw + fontImgX;
                    for (int x = 0; x < fontBW / 2; x++, ptr++) {
                        if (*ptr == 0) {
                            hasBlackPixel = true;
                            break;
                        }
                    }
                    if (hasBlackPixel)
                        break;
                }
                charHalfs[ch] = !hasBlackPixel;
            }
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
                if (isAscii) {
                    if (ch >= 32 && ch <= 128) {
                        int fontX = (ch - 32) * fontBW;
                        int fontImgY = fontX / fontImage.bw;
                        int fontImgX = fontX % fontImage.bw;
                        DrawChar(fontImgX, fontImgY, fontBW, dispBuf, dispBW, dispBH, x, y, icolor);
                    }
                    x += fontBW;
                } else {
                    int fontX = ch * fontBW;
                    int fontImgY = fontX / fontImage.bw * fontBH;
                    int fontImgX = fontX % fontImage.bw;
                    int fontBW2 = charHalfs[ch] ? fontBW / 2 : fontBW;
                    DrawChar(fontImgX, fontImgY, fontBW2, dispBuf, dispBW, dispBH, x, y, icolor);
                    x += fontBW2;
                }
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
                    y += fontBH;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                    continue;
                }
                if (isAscii) {
                    x += fontBW;
                } else {
                    int fontBW2 = charHalfs[ch] ? fontBW / 2 : fontBW;
                    x += fontBW2;
                }
                if (x > maxX) maxX = x;
            }
            y += fontBH;

            return new Size(maxX, maxY);
        }

        private unsafe void DrawChar(int fontImgX, int fontImgY, int fontBW2, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, int icolor) {
            for (int y = 0; y < fontBH; y++) {
                if (dy + y < 0 || dy + y >= dispBH)
                    continue;
                int* dst = (int*)dispBuf + dispBW * (dy + y) + dx;
                byte* src = (byte*)fontImage.buf + fontImage.bw * (fontImgY + y) + fontImgX;
                for (int x = 0; x < fontBW2; x++, src++, dst++) {
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
