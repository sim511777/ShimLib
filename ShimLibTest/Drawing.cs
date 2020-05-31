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

            int f = 1 - radius;
            int ddF_x = 0;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            while (x <= y) {
                DrawPixel(ptr, bw, bh, (cx + x), (cy + y), iCol);
                DrawPixel(ptr, bw, bh, (cx - x), (cy + y), iCol);
                DrawPixel(ptr, bw, bh, (cx + x), (cy - y), iCol);
                DrawPixel(ptr, bw, bh, (cx - x), (cy - y), iCol);
                DrawPixel(ptr, bw, bh, (cx + y), (cy + x), iCol);
                DrawPixel(ptr, bw, bh, (cx - y), (cy + x), iCol);
                DrawPixel(ptr, bw, bh, (cx + y), (cy - x), iCol);
                DrawPixel(ptr, bw, bh, (cx - y), (cy - x), iCol);

                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x + 1;
            }
        }

        public static unsafe void DrawLineBresenham(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
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

        public static unsafe void DrawLineEquation(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (dx == 0 && dy == 0)
                return;
            if (dy == 0) {
                DrawHLine(buf, bw, bh, y1, x1, x2, iCol);
                return;
            }
            if (dx == 0) {
                DrawVLine(buf, bw, bh, x1, y1, y2, iCol);
                return;
            }

            int* ptr = (int*)buf;
            float m = (float)dy / dx;
            if (m >= -1 && m <= 1) {
                int step = dx > 0 ? 1 : -1;
                for (int x = x1; x != x2; x += step) {
                    float fy = m * (x - x1) + y1;
                    int y = (int)(fy + 0.5f);
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                }
            } else {
                int step = dy > 0 ? 1 : -1;
                m = 1 / m;
                for (int y = y1; y != y2; y += step) {
                    float fx = m * (y - y1) + x1;
                    int x = (int)(fx + 0.5f);
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                }
            }
        }

        public static unsafe void DrawLineDda(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (dx == 0 && dy == 0)
                return;
            if (dy == 0) {
                DrawHLine(buf, bw, bh, y1, x1, x2, iCol);
                return;
            }
            if (dx == 0) {
                DrawVLine(buf, bw, bh, x1, y1, y2, iCol);
                return;
            }

            int* ptr = (int*)buf;
            float m = (float)dy / dx;
            if (m >= -1 && m <= 1) {
                int step = dx > 0 ? 1 : -1;
                m *= step;
                float fy = y1;
                for (int x = x1; x != x2; x += step, fy += m) {
                    int y = (int)(fy + 0.5f);
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                }
            } else {
                int step = dy > 0 ? 1 : -1;
                m = 1 / m;
                m *= step;
                float fx = x1;
                for (int y = y1; y != y2; y += step, fx += m) {
                    int x = (int)(fx + 0.5f);
                    DrawPixel(ptr, bw, bh, x, y, iCol);
                }
            }
        }

        public static unsafe void DrawHLine(IntPtr buf, int bw, int bh, int y, int x1, int x2, int iCol) {
            int* ptr = (int*)buf;
            int step = x2 > x1 ? 1 : -1;
            for (int x = x1; x != x2; x += step) {
                DrawPixel(ptr, bw, bh, x, y, iCol);
            }
        }

        public static unsafe void DrawVLine(IntPtr buf, int bw, int bh, int x, int y1, int y2, int iCol) {
            int* ptr = (int*)buf;
            int step = y2 > y1 ? 1 : -1;
            for (int y = y1; y != y2; y += step) {
                DrawPixel(ptr, bw, bh, x, y, iCol);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void DrawPixel(int *ptr, int bw, int bh, int x, int y, int iCol) {
            if (x >= 0 && x < bw && y >= 0 && y < bh)
                *(ptr + bw * y + x) = iCol;
        }
    }
}
