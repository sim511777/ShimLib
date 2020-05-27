using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLibTest {
    public class Drawing {
        
        public static unsafe void DrawLine(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol, int lineType) {
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

            if (lineType == 0)
                DrawLineEquation(buf, bw, bh, x1, y1, x2, y2, iCol);
            else if (lineType == 1)
                DrawLineDda(buf, bw, bh, x1, y1, x2, y2, iCol);
            else if (lineType == 2)
                DrawLineBresenham(buf, bw, bh, x1, y1, x2, y2, iCol);
        }

        private static unsafe void DrawLineBresenham(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            int* ptr = (int*)buf;
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2;
            int e2;

            while (x1 != x2 || y1 != y2) {
                if (x1 >= 0 && x1 < bw && y1 >= 0 && y1 < bh)
                    *(ptr + bw * y1 + x1) = iCol;
                
                e2 = err;
                if (e2 > -dx) {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dy) {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        private static unsafe void DrawLineEquation(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            int* ptr = (int*)buf;
            int dx = x2 - x1;
            int dy = y2 - y1;
            float m = (float)dy / dx;

            if (m >= -1 && m <= 1) {
                int step = dx > 0 ? 1 : -1;
                for (int x = x1; x != x2; x += step) {
                    if (x < 0 || x >= bw)
                        continue;
                    float fy = m * (x - x1) + y1;
                    int y = (int)(fy + 0.5f);
                    if (y < 0 || y >= bh)
                        continue;
                    *(ptr + bw * y + x) = iCol;
                }
            } else {
                int step = dy > 0 ? 1 : -1;
                m = 1 / m;
                for (int y = y1; y != y2; y += step) {
                    if (y < 0 || y >= bh)
                        continue;
                    float fx = m * (y - y1) + x1;
                    int x = (int)(fx + 0.5f);
                    if (x < 0 || x >= bw)
                        continue;
                    *(ptr + bw * y + x) = iCol;
                }
            }
        }

        private static unsafe void DrawLineDda(IntPtr buf, int bw, int bh, int x1, int y1, int x2, int y2, int iCol) {
            int* ptr = (int*)buf;
            int dx = x2 - x1;
            int dy = y2 - y1;
            float m = (float)dy / dx;
            
            if (m >= -1 && m <= 1) {
                int step = dx > 0 ? 1 : -1;
                m *= step;
                float fy = y1;
                for (int x = x1; x != x2; x += step, fy += m) {
                    if (x < 0 || x >= bw)
                        continue;
                    int y = (int)(fy + 0.5f);
                    if (y < 0 || y >= bh)
                        continue;
                    *(ptr + bw * y + x) = iCol;
                }
            } else {
                int step = dy > 0 ? 1 : -1;
                m = 1 / m;
                m *= step;
                float fx = x1;
                for (int y = y1; y != y2; y += step, fx += m) {
                    if (y < 0 || y >= bh)
                        continue;
                    int x = (int)(fx + 0.5f);
                    if (x < 0 || x >= bw)
                        continue;
                    *(ptr + bw * y + x) = iCol;
                }
            }
        }

        private static unsafe void DrawHLine(IntPtr buf, int bw, int bh, int y, int x1, int x2, int iCol) {
            if (x1 == x2 || y < 0 || y >= bh)
                return;

            int* ptr = (int*)buf + bw * y + x1;
            int step = x2 > x1 ? 1 : -1;
            for (int x = x1; x != x2; x += step, ptr += step) {
                if (x < 0 || x >= bw)
                    continue;
                *ptr = iCol;
            }
        }

        private static unsafe void DrawVLine(IntPtr buf, int bw, int bh, int x, int y1, int y2, int iCol) {
            if (y1 == y2 || x < 0 || x >= bw)
                return;

            int* ptr = (int*)buf + bw * y1 + x;
            int step = y2 > y1 ? 1 : -1;
            int bufStep = step * bw;
            for (int y = y1; y != y2; y += step, ptr += bufStep) {
                if (y < 0 || y >= bh)
                    continue;
                *ptr = iCol;
            }
        }
    }
}
