using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class ImageDrawing {
        public ImageBox imgBox { get; private set; }
        public IntPtr buf { get; private set; }
        public int bw { get; private set; }
        public int bh { get; private set; }
        public ImageDrawing(ImageBox _imgBox, IntPtr _buf, int _bw, int _bh) {
            imgBox = _imgBox;
            buf = _buf;
            bw = _bw;
            bh = _bh;
        }

        public void DrawLine(PointF pt1, PointF pt2, Color col) {
            var ptd1 = imgBox.ImgToDisp(pt1);
            var ptd2 = imgBox.ImgToDisp(pt2);
            Drawing.DrawLine(buf, bw, bh, ptd1.X, ptd1.Y, ptd2.X, ptd2.Y, col.ToArgb());
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Color col) {
            DrawLine(new PointF(x1, y1), new PointF(x2, y2), col);
        }

        public void DrawRectangle(PointF pt1, PointF pt2, Color col, bool fill = false) {
            var ptd1 = imgBox.ImgToDisp(pt1);
            var ptd2 = imgBox.ImgToDisp(pt2);
            if (!fill)
                Drawing.DrawRectangle(buf, bw, bh, ptd1.X, ptd1.Y, ptd2.X, ptd2.Y, col.ToArgb());
            else
                Drawing.FillRectangle(buf, bw, bh, ptd1.X, ptd1.Y, ptd2.X, ptd2.Y, col.ToArgb());
        }

        public void DrawRectangle(float x1, float y1, float x2, float y2, Color col, bool fill = false) {
            DrawRectangle(new PointF(x1, y1), new PointF(x2, y2), col, fill);
        }

        public void DrawCircle(PointF pt, float r, Color col, bool fill = false) {
            var ptd = imgBox.ImgToDisp(pt);
            var rd = (int)(imgBox.GetZoomFactor() * r);
            if (!fill)
                Drawing.DrawCircle(buf, bw, bh, ptd.X, ptd.Y, rd, col.ToArgb());
            else
                Drawing.FillCircle(buf, bw, bh, ptd.X, ptd.Y, rd, col.ToArgb());
        }

        public void DrawCircle(float x, float y, float r, Color col, bool fill = false) {
            DrawCircle(new PointF(x, y), r, col, fill);
        }

        public void DrawSquare(PointF pt, float size, Color col, bool fixSize, bool fill = false) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X - sizeh;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X + sizeh;
            float y2 = pt.Y + sizeh;
            DrawRectangle(x1, y1, x2, y2, col, fill);
        }

        public void DrawSquare(float x, float y, float size, Color col, bool fixSize, bool fill = false) {
            DrawSquare(new PointF(x, y), size, col, fixSize, fill);
        }

        public void DrawCircleSize(PointF pt, float size, Color col, bool fixSize, bool fill = false) {
            float r = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            DrawCircle(pt, r, col, fill);
        }

        public void DrawCircleSize(float x, float y, float size, Color col, bool fixSize, bool fill = false) {
            DrawCircleSize(new PointF(x, y), size, col, fixSize, fill);
        }

        public void DrawPlus(PointF pt, float size, Color col, bool fixSize) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X;
            float y2 = pt.Y + sizeh;
            float x3 = pt.X - sizeh;
            float y3 = pt.Y;
            float x4 = pt.X + sizeh;
            float y4 = pt.Y;
            DrawLine(x1, y1, x2, y2, col);
            DrawLine(x3, y3, x4, y4, col);
        }

        public void DrawPlus(float x, float y, float size, Color col, bool fixSize) {
            DrawPlus(new PointF(x, y), size, col, fixSize);
        }

        public void DrawCross(PointF pt, float size, Color col, bool fixSize) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X - sizeh;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X + sizeh;
            float y2 = pt.Y + sizeh;
            float x3 = pt.X + sizeh;
            float y3 = pt.Y - sizeh;
            float x4 = pt.X - sizeh;
            float y4 = pt.Y + sizeh;
            DrawLine(x1, y1, x2, y2, col);
            DrawLine(x3, y3, x4, y4, col);
        }

        public void DrawCross(float x, float y, float size, Color col, bool fixSize) {
            DrawCross(new PointF(x, y), size, col, fixSize);
        }

        public void DrawString(string text, PointF pt, Color col, Color fillCol = default(Color)) {
            Point ptd = imgBox.ImgToDisp(pt);
            DrawStringWnd(text, ptd, col, fillCol);
        }

        public void DrawString(string text, float x, float y, Color col, Color fillCol = default(Color)) {
            DrawString(text, new PointF(x, y), col, fillCol);
        }

        public void DrawStringWnd(string text, Point ptd, Color col, Color fillCol = default(Color)) {
            if (!fillCol.IsEmpty) {
                Size size = imgBox.FontRender.MeasureString(text);
                Drawing.FillRectangle(buf, bw, bh, ptd.X, ptd.Y, ptd.X + size.Width, ptd.Y + size.Height, fillCol.ToArgb());
            }
            imgBox.FontRender.DrawString(text, buf, bw, bh, ptd.X, ptd.Y, col);
        }

        public void DrawStringWnd(string text, int x, int y, Color col, Color fillCol = default(Color)) {
            DrawStringWnd(text, new Point(x, y), col, fillCol);
        }

        public void DrawImage(IntPtr imgBuf, int imgBW, int imgBH, int x0, int y0) {
            Drawing.DrawImage(buf, bw, bh, imgBuf, imgBW, imgBH, x0, y0);
        }
    }
}
