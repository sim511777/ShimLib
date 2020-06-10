using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLibTest {
    public class Drawing {
        public static unsafe void DrawCircle(IntPtr buf, int bw, int bh, int cx, int cy, int radius, int iCol) {
            int* ptr = (int*)buf;

            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

            do {
                DrawPixel(ptr, bw, bh, cx + x, cy + y, iCol);
                DrawPixel(ptr, bw, bh, cx + x, cy - y, iCol);
                DrawPixel(ptr, bw, bh, cx - x, cy + y, iCol);
                DrawPixel(ptr, bw, bh, cx - x, cy - y, iCol);
                DrawPixel(ptr, bw, bh, cx + y, cy + x, iCol);
                DrawPixel(ptr, bw, bh, cx + y, cy - x, iCol);
                DrawPixel(ptr, bw, bh, cx - y, cy + x, iCol);
                DrawPixel(ptr, bw, bh, cx - y, cy - x, iCol);

                if (d < 0) {
                    d += 2 * x + 1;
                } else {
                    d += 2 * (x - y) + 1;
                    --y;
                }
                ++x;
            } while (x <= y);
        }

        public static unsafe void FillCircle(IntPtr buf, int bw, int bh, int cx, int cy, int radius, int iCol) {
            int* ptr = (int*)buf;
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

            while (x <= y) {
                DrawHLine(ptr, bw, bh, cx - x, cx + x, cy + y, iCol);
                DrawHLine(ptr, bw, bh, cx - x, cx + x, cy - y, iCol);
                DrawHLine(ptr, bw, bh, cx - y, cx + y, cy + x, iCol);
                DrawHLine(ptr, bw, bh, cx - y, cx + y, cy - x, iCol);

                if (d < 0) {
                    d += 2 * x + 1;
                } else {
                    d += 2 * (x - y) + 1;
                    --y;
                }
                ++x;
            }
        }

        public static unsafe void DrawLine(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
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
        }

        public static unsafe void DrawRectangle(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            DrawLine(buf, bw, bh, x1, y1, x2, y1, iCol);
            DrawLine(buf, bw, bh, x2, y1, x2, y2, iCol);
            DrawLine(buf, bw, bh, x2, y2, x1, y2, iCol);
            DrawLine(buf, bw, bh, x1, y2, x1, y1, iCol);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DrawPixel(int *ptr, int bw, int bh, int x, int y, int iCol) {
            if (x >= 0 && x < bw && y >= 0 && y < bh)
                *(ptr + bw * y + x) = iCol;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DrawHLine(int* ptr, int bw, int bh, int x1, int x2, int y, int iCol) {
            if (x1 >= bw || x2 < 0 || y < 0)
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
    }
}
