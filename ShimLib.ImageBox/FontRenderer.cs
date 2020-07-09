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
        private bool isAscii;
        private bool[] charHalfs;
        public FontRenderer(Bitmap bmp, int _fw, int _fh, bool _isAscii) {
            int bytepp = 0;
            ImageUtil.BitmapToImageBuffer(bmp, ref fontBuf, ref fontBw, ref fontBh, ref bytepp);
            fw = _fw;
            fh = _fh;
            isAscii = _isAscii;
            if (!isAscii) {
                GetCharHalfs();
            }
        }
        
        ~FontRenderer() {
            Util.FreeBuffer(ref fontBuf);
        }

        private unsafe void GetCharHalfs() {
            charHalfs = new bool[65536];
            for (int ch = 0; ch < 65536; ch++) {
                int fontX = ch * fw;
                int fontImgY = fontX / fontBw;
                int fontImgX = fontX % fontBw + fw / 2;
                bool hasBlackPixel = false;
                for (int y = 0; y < fh; y++) {
                    byte* ptr = (byte*)fontBuf + (fontImgY + y) * fontBw + fontImgX;
                    for (int x = 0; x < fw / 2; x++, ptr++) {
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
                    y += fh;
                    continue;
                }
                if (isAscii) {
                    if (ch >= 32 && ch <= 128) {
                        int fontX = (ch - 32) * fw;
                        int fontImgY = fontX / fontBw;
                        int fontImgX = fontX % fontBw;
                        DrawChar(fontImgX, fontImgY, fw, dispBuf, dispBW, dispBH, x, y, icolor);
                    }
                    x += fw;
                } else {
                    int fontX = ch * fw;
                    int fontImgY = fontX / fontBw * fh;
                    int fontImgX = fontX % fontBw;
                    int fontBW2 = charHalfs[ch] ? fw / 2 : fw;
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
                    y += fh;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                    continue;
                }
                if (isAscii) {
                    x += fw;
                } else {
                    int fontBW2 = charHalfs[ch] ? fw / 2 : fw;
                    x += fontBW2;
                }
                if (x > maxX) maxX = x;
            }
            y += fh;

            return new Size(maxX, maxY);
        }

        private unsafe void DrawChar(int fontImgX, int fontImgY, int fontBW2, IntPtr dispBuf, int dispBW, int dispBH, int dx, int dy, int icolor) {
            for (int y = 0; y < fh; y++) {
                if (dy + y < 0 || dy + y >= dispBH)
                    continue;
                int* dst = (int*)dispBuf + dispBW * (dy + y) + dx;
                byte* src = (byte*)fontBuf + fontBw * (fontImgY + y) + fontImgX;
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
