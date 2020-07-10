using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class Drawing {
        public static void DrawCircle(IntPtr buf, int bw, int bh, int cx, int cy, int radius, int iCol) {
            MidpointCircle(buf, bw, bh, cx, cy, radius, iCol, false);
        }

        public static void FillCircle(IntPtr buf, int bw, int bh, int cx, int cy, int radius, int iCol) {
            MidpointCircle(buf, bw, bh, cx, cy, radius, iCol, true);
        }

        private static unsafe void MidpointCircle(IntPtr buf, int bw, int bh, int cx, int cy, int radius, int iCol, bool fill) {
            int x1 = cx - radius;
            int x2 = cx + radius;
            int y1 = cy - radius;
            int y2 = cy + radius;
            if (x1 >= bw || x2 < 0 || y1 >= bh || y2 < 0)
                return;

            int* ptr = (int*)buf;

            int x = 0;
            int y = radius;
            int d = 1 - radius;

            while (x <= y) {
                if (fill) {
                    DrawHLine(ptr, bw, bh, cx - x, cx + x, cy + y, iCol);
                    DrawHLine(ptr, bw, bh, cx - x, cx + x, cy - y, iCol);
                    DrawHLine(ptr, bw, bh, cx - y, cx + y, cy + x, iCol);
                    DrawHLine(ptr, bw, bh, cx - y, cx + y, cy - x, iCol);
                } else {
                    DrawPixel(ptr, bw, bh, cx + x, cy + y, iCol);
                    DrawPixel(ptr, bw, bh, cx + x, cy - y, iCol);
                    DrawPixel(ptr, bw, bh, cx - x, cy + y, iCol);
                    DrawPixel(ptr, bw, bh, cx - x, cy - y, iCol);
                    DrawPixel(ptr, bw, bh, cx + y, cy + x, iCol);
                    DrawPixel(ptr, bw, bh, cx + y, cy - x, iCol);
                    DrawPixel(ptr, bw, bh, cx - y, cy + x, iCol);
                    DrawPixel(ptr, bw, bh, cx - y, cy - x, iCol);
                }

                ++x;
                if (d < 0) {
                    d += 2 * x + 1;
                } else {
                    --y;
                    d += 2 * (x - y) + 3;
                }
            }
        }

        public static unsafe void DrawLine(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            if ((x1 < 0 && x2 < 0) || (x1 >= bw && x2 > bw) || (y1 < 0 && y2 < 0) ||(y1 >= bh && y2 >= bh))
                return;

            int dx = (x2 > x1) ? (x2 - x1) : (x1 - x2);
            int dy = (y2 > y1) ? (y2 - y1) : (y1 - y2);
            int sx = (x2 > x1) ? 1 : -1;
            int sy = (y2 > y1) ? 1 : -1;
            int dx2 = dx * 2;
            int dy2 = dy * 2;

            int* ptr = (int*)buf;
            int x = x1;
            int y = y1;
            if (dy < dx) {
                int d = dy2 - dx;
                while (x != x2) {
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                    x += sx;
                    d += dy2;
                    if (d > 0) {
                        y += sy;
                        d -= dx2;
                    }
                }
            } else {
                int d = dx2 - dy;
                while (y != y2) {
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                    y += sy;
                    d += dx2;
                    if (d > 0) {
                        x += sx;
                        d -= dy2;
                    }
                }
            }
            DrawPixel(ptr, bw, bh, x2, y2, iCol); // 끝점도 찍음
        }

        public static unsafe void DrawRectangle(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            if (x1 > x2) Util.Swap(ref x1, ref x2);
            if (y1 > y2) Util.Swap(ref y1, ref y2);
            if (x1 >= bw || x2 < 0 || y1 >= bh || y2 < 0)
                return;

            DrawLine(buf, bw, bh, x1, y1, x2, y1, iCol);
            DrawLine(buf, bw, bh, x2, y1, x2, y2, iCol);
            DrawLine(buf, bw, bh, x2, y2, x1, y2, iCol);
            DrawLine(buf, bw, bh, x1, y2, x1, y1, iCol);
        }

        public static unsafe void FillRectangle(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            if (x1 > x2) Util.Swap(ref x1, ref x2);
            if (y1 > y2) Util.Swap(ref y1, ref y2);
            if (x1 >= bw || x2 < 0 || y1 >= bh || y2 < 0)
                return;

            int* ptr = (int*)buf;

            for (int y = y1; y <= y2; y++) {
                DrawHLine(ptr, bw, bh, x1, x2, y, iCol);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void DrawPixel(int *ptr, int bw, int bh, int x, int y, int iCol) {
            if (x >= 0 && x < bw && y >= 0 && y < bh)
                *(ptr + bw * y + x) = iCol;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DrawHLine(int* ptr, int bw, int bh, int x1, int x2, int y, int iCol) {
            if (y < 0 || y >= bh || x1 >= bw || x2 < 0)
                return;

            if (x1 < 0)
                x1 = 0;
            if (x2 >= bw)
                x2 = bw - 1;

            int* ptr1 = ptr + (bw * y) + x1;
            int size = x2 - x1 + 1;
            while (size-- > 0)
                *ptr1++ = iCol;
        }

        public static unsafe void DrawImage(IntPtr dbuf, int dw, int dh, IntPtr sbuf, int sw, int sh, int x0, int y0) {
            if (x0 >= dw || x0 <= -sw || y0 >= dh || y0 <= -sh)
	            return;

            int dx1 = x0;
            int dx2 = x0 + sw;
            int sx1 = 0;
            int sx2 = sw;

            if (dx1 < 0) {
	            sx1 -= dx1;
	            dx1 = 0;
            }
            if (dx2 > dw) {
	            sx2 -= dx2 - dw;;
	            dx2 = dw;
            }
            int copyw = dx2 - dx1;

            int dy1 = y0;
            int dy2 = y0 + sh;
            int sy1 = 0;
            int sy2 = sh;

            if (dy1 < 0) {
	            sy1 -= dy1;
	            dy1 = 0;
            }
            if (dy2 > dh) {
	            sy2 -= dy2 - dh;
	            dy2 = dh;
            }

            for (int dy = dy1, sy = sy1; dy < dy2; dy++, sy++) {
                int* dptr = (int*)dbuf + dw * dy + dx1;
                int* sptr = (int*)sbuf + sw * sy + sx1;
                Util.Memcpy((IntPtr)dptr, (IntPtr)sptr, copyw * 4);
            }
        }

        // 이미지 버퍼를 디스플레이 버퍼에 복사
        public static unsafe void DrawImageZoom(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, Int64 panx, Int64 pany, double zoom, int bytepp, int bgColor, bool bufIsFloat) {
            // 인덱스 버퍼 생성
            int[] siys = new int[dbh];
            int[] sixs = new int[dbw];
            for (int y = 0; y < dbh; y++) {
                int siy = (int)Math.Floor((y - pany) / zoom);
                siys[y] = (sbuf == IntPtr.Zero || siy < 0 || siy >= sbh) ? -1 : siy;
            }
            for (int x = 0; x < dbw; x++) {
                int six = (int)Math.Floor((x - panx) / zoom);
                sixs[x] = (sbuf == IntPtr.Zero || six < 0 || six >= sbw) ? -1 : six;
            }

            for (int y = 0; y < dbh; y++) {
                int siy = siys[y];
                byte* sptr = (byte*)sbuf + (Int64)sbw * siy * bytepp;
                int* dp = (int*)dbuf + (Int64)dbw * y;
                for (int x = 0; x < dbw; x++, dp++) {
                    int six = sixs[x];
                    if (siy == -1 || six == -1) {   // out of boundary of image
                        *dp = bgColor;
                    } else {
                        byte* sp = &sptr[six * bytepp];
                        if (bufIsFloat) {
                            if (bytepp == 4) {
                                int v = (int)*(float*)sp;
                                *dp = v | v << 8 | v << 16 | 0xff << 24;
                            } else if (bytepp == 8) {
                                int v = (int)*(double*)sp;
                                *dp = v | v << 8 | v << 16 | 0xff << 24;
                            }
                        } else {
                            if (bytepp == 1) {          // 8bit gray
                                int v = sp[0];
                                *dp = v | v << 8 | v << 16 | 0xff << 24;
                            } else if (bytepp == 2) {   // 16bit gray (*.hra)
                                int v = sp[0];
                                *dp = v | v << 8 | v << 16 | 0xff << 24;
                            } else if (bytepp == 3) {   // 24bit bgr
                                *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                            } else if (bytepp == 4) {   // 32bit bgra
                                *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                            }
                        }
                    }
                }
            }
        }
    }
}
