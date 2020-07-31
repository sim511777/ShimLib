using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class ImageGraphics {
        public ImageBox imgBox { get; private set; }
        public Graphics g { get; private set; }
        public ImageGraphics(ImageBox _imgBox, Graphics _g) {
            imgBox = _imgBox;
            g = _g;
        }

        public void DrawLine(PointF pt1, PointF pt2, Pen pen) {
            var ptd1 = imgBox.ImgToDisp(pt1);
            var ptd2 = imgBox.ImgToDisp(pt2);
            g.DrawLine(pen, ptd1, ptd2);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Pen pen) {
            DrawLine(new PointF(x1, y1), new PointF(x2, y2), pen);
        }

        public void DrawRectangle(PointF pt1, PointF pt2, Pen pen, bool fill = false, Brush fillBrush = null) {
            var ptd1 = imgBox.ImgToDisp(pt1);
            var ptd2 = imgBox.ImgToDisp(pt2);
            if (!fill)
                g.DrawRectangle(pen, ptd1.X, ptd1.Y, (ptd2.X - ptd1.X), (ptd2.Y - ptd1.Y));
            else
                g.FillRectangle(fillBrush, ptd1.X, ptd1.Y, (ptd2.X - ptd1.X), (ptd2.Y - ptd1.Y));
        }

        public void DrawRectangle(float x1, float y1, float x2, float y2, Pen pen, bool fill = false, Brush fillBrush = null) {
            DrawRectangle(new PointF(x1, y1), new PointF(x2, y2), pen, fill, fillBrush);
        }

        public void DrawEllipse(PointF pt1, PointF pt2, Pen pen, bool fill = false, Brush fillBrush = null) {
            var ptd1 = imgBox.ImgToDisp(pt1);
            var ptd2 = imgBox.ImgToDisp(pt2);
            if (!fill)
                g.DrawEllipse(pen, ptd1.X, ptd1.Y, (ptd2.X - ptd1.X), (ptd2.Y - ptd1.Y));
            else
                g.FillEllipse(fillBrush, ptd1.X, ptd1.Y, (ptd2.X - ptd1.X), (ptd2.Y - ptd1.Y));
        }

        public void DrawEllipse(float x1, float y1, float x2, float y2, Pen pen, bool fill = false, Brush fillBrush = null) {
            DrawEllipse(new PointF(x1, y1), new PointF(x2, y2), pen, fill, fillBrush);
        }

        public void DrawSquare(PointF pt, float size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X - sizeh;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X + sizeh;
            float y2 = pt.Y + sizeh;
            DrawRectangle(x1, y1, x2, y2, pen, fill, fillBrush);
        }

        public void DrawSquare(float x, float y, float size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            DrawSquare(new PointF(x, y), size, pen, fixSize, fill, fillBrush);
        }

        public void DrawCircle(PointF pt, float size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X - sizeh;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X + sizeh;
            float y2 = pt.Y + sizeh;
            DrawEllipse(x1, y1, x2, y2, pen, fill, fillBrush);
        }

        public void DrawCircle(float x, float y, float size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            DrawCircle(new PointF(x, y), size, pen, fixSize, fill, fillBrush);
        }

        public void DrawPlus(PointF pt, float size, Pen pen, bool fixSize) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X;
            float y2 = pt.Y + sizeh;
            float x3 = pt.X - sizeh;
            float y3 = pt.Y;
            float x4 = pt.X + sizeh;
            float y4 = pt.Y;
            DrawLine(x1, y1, x2, y2, pen);
            DrawLine(x3, y3, x4, y4, pen);
        }

        public void DrawPlus(float x, float y, float size, Pen pen, bool fixSize) {
            DrawPlus(new PointF(x, y), size, pen, fixSize);
        }

        public void DrawCross(PointF pt, float size, Pen pen, bool fixSize) {
            float sizeh = fixSize ? size * 0.5f / (float)imgBox.GetZoomFactor() : size * 0.5f;
            float x1 = pt.X - sizeh;
            float y1 = pt.Y - sizeh;
            float x2 = pt.X + sizeh;
            float y2 = pt.Y + sizeh;
            float x3 = pt.X + sizeh;
            float y3 = pt.Y - sizeh;
            float x4 = pt.X - sizeh;
            float y4 = pt.Y + sizeh;
            DrawLine(x1, y1, x2, y2, pen);
            DrawLine(x3, y3, x4, y4, pen);
        }

        public void DrawCross(float x, float y, float size, Pen pen, bool fixSize) {
            DrawCross(new PointF(x, y), size, pen, fixSize);
        }

        // font가 null 이면 ImageBox.Font 로 그림
        // fillBrush가 null 이면 채우기 없음
        public void DrawString(string text, PointF pt, Font font, Brush fontBrush, Brush fillBrush) {
            if (font == null)
                font = imgBox.Font;

            Point ptd = imgBox.ImgToDisp(pt);

            if (fillBrush != null) {
                SizeF size = g.MeasureString(text, font);
                g.FillRectangle(fillBrush, ptd.X, ptd.Y, (int)size.Width, (int)size.Height);
            }
            g.DrawString(text, font, fontBrush, ptd);
        }

        public void DrawString(string text, float x, float y, Font font, Brush fontBrush, Brush fillBrush) {
            DrawString(text, new PointF(x, y), font, fontBrush, fillBrush);
        }

        public void DrawStringWnd(string text, Point ptd, Font font, Brush fontBrush, Brush fillBrush) {
            if (font == null)
                font = imgBox.Font;

            if (fillBrush != null) {
                SizeF size = g.MeasureString(text, font);
                g.FillRectangle(fillBrush, ptd.X, ptd.Y, (int)size.Width, (int)size.Height);
            }
            g.DrawString(text, font, fontBrush, ptd);
        }

        public void DrawStringWnd(string text, int x, int y, Font font, Brush fontBrush, Brush fillBrush) {
            DrawStringWnd(text, new Point(x, y), font, fontBrush, fillBrush);
        }

        public void DrawPolygon(PointF[] pts, Pen pen) {
            var dispPts = pts.Select(pt => imgBox.ImgToDisp(pt)).ToArray();
            g.DrawPolygon(pen, dispPts);
        }
    }
}
