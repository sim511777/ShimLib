using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointD = System.Windows.Point;

namespace ShimLib {
    public class ImageGraphics {
        public ImageBox imgBox { get; private set; }
        public Graphics g { get; private set; }
        public ImageGraphics(ImageBox _imgBox, Graphics _g) {
            imgBox = _imgBox;
            g = _g;
        }

        public void DrawLine(PointD pt1, PointD pt2, Pen pen) {
            PointD ptd1 = imgBox.ImgToDisp(pt1);
            PointD ptd2 = imgBox.ImgToDisp(pt2);
            g.DrawLine(pen, ptd1.ToFloat(), ptd2.ToFloat());
        }

        public void DrawLine(double x1, double y1, double x2, double y2, Pen pen) {
            DrawLine(new PointD(x1, y1), new PointD(x2, y2), pen);
        }

        public void DrawRectangle(PointD pt1, PointD pt2, Pen pen, bool fill = false, Brush fillBrush = null) {
            PointD ptd1 = imgBox.ImgToDisp(pt1);
            PointD ptd2 = imgBox.ImgToDisp(pt2);
            if (!fill)
                g.DrawRectangle(pen, (float)ptd1.X, (float)ptd1.Y, (float)(ptd2.X - ptd1.X), (float)(ptd2.Y - ptd1.Y));
            else
                g.FillRectangle(fillBrush, (float)ptd1.X, (float)ptd1.Y, (float)(ptd2.X - ptd1.X), (float)(ptd2.Y - ptd1.Y));
        }

        public void DrawRectangle(double x1, double y1, double x2, double y2, Pen pen, bool fill = false, Brush fillBrush = null) {
            DrawRectangle(new PointD(x1, y1), new PointD(x2, y2), pen, fill, fillBrush);
        }

        public void DrawEllipse(PointD pt1, PointD pt2, Pen pen, bool fill = false, Brush fillBrush = null) {
            PointD ptd1 = imgBox.ImgToDisp(pt1);
            PointD ptd2 = imgBox.ImgToDisp(pt2);
            if (!fill)
                g.DrawEllipse(pen, (float)ptd1.X, (float)ptd1.Y, (float)(ptd2.X - ptd1.X), (float)(ptd2.Y - ptd1.Y));
            else
                g.FillEllipse(fillBrush, (float)ptd1.X, (float)ptd1.Y, (float)(ptd2.X - ptd1.X), (float)(ptd2.Y - ptd1.Y));
        }

        public void DrawEllipse(double x1, double y1, double x2, double y2, Pen pen, bool fill = false, Brush fillBrush = null) {
            DrawEllipse(new PointD(x1, y1), new PointD(x2, y2), pen, fill, fillBrush);
        }

        public void DrawSquare(PointD pt, double size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            double sizeh = fixSize ? size * 0.5 / imgBox.GetZoomFactor() : size * 0.5;
            double x1 = pt.X - sizeh;
            double y1 = pt.Y - sizeh;
            double x2 = pt.X + sizeh;
            double y2 = pt.Y + sizeh;
            DrawRectangle(x1, y1, x2, y2, pen, fill, fillBrush);
        }

        public void DrawSquare(double x, double y, double size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            DrawSquare(new PointD(x, y), size, pen, fixSize, fill, fillBrush);
        }

        public void DrawCircle(PointD pt, double size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            double sizeh = fixSize ? size * 0.5 / imgBox.GetZoomFactor() : size * 0.5;
            double x1 = pt.X - sizeh;
            double y1 = pt.Y - sizeh;
            double x2 = pt.X + sizeh;
            double y2 = pt.Y + sizeh;
            DrawEllipse(x1, y1, x2, y2, pen, fill, fillBrush);
        }

        public void DrawCircle(double x, double y, double size, Pen pen, bool fixSize, bool fill = false, Brush fillBrush = null) {
            DrawCircle(new PointD(x, y), size, pen, fixSize, fill, fillBrush);
        }

        public void DrawPlus(PointD pt, double size, Pen pen, bool fixSize) {
            double sizeh = fixSize ? size * 0.5 / imgBox.GetZoomFactor() : size * 0.5;
            double x1 = pt.X;
            double y1 = pt.Y - sizeh;
            double x2 = pt.X;
            double y2 = pt.Y + sizeh;
            double x3 = pt.X - sizeh;
            double y3 = pt.Y;
            double x4 = pt.X + sizeh;
            double y4 = pt.Y;
            DrawLine(x1, y1, x2, y2, pen);
            DrawLine(x3, y3, x4, y4, pen);
        }

        public void DrawPlus(double x, double y, double size, Pen pen, bool fixSize) {
            DrawPlus(new PointD(x, y), size, pen, fixSize);
        }

        public void DrawCross(PointD pt, double size, Pen pen, bool fixSize) {
            double sizeh = fixSize ? size * 0.5 / imgBox.GetZoomFactor() : size * 0.5;
            double x1 = pt.X - sizeh;
            double y1 = pt.Y - sizeh;
            double x2 = pt.X + sizeh;
            double y2 = pt.Y + sizeh;
            double x3 = pt.X + sizeh;
            double y3 = pt.Y - sizeh;
            double x4 = pt.X - sizeh;
            double y4 = pt.Y + sizeh;
            DrawLine(x1, y1, x2, y2, pen);
            DrawLine(x3, y3, x4, y4, pen);
        }

        public void DrawCross(double x, double y, double size, Pen pen, bool fixSize) {
            DrawCross(new PointD(x, y), size, pen, fixSize);
        }

        public void DrawString(string text, PointD pt, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            PointD ptd = imgBox.ImgToDisp(pt);
            SizeF size = g.MeasureString(text, imgBox.Font);
            if (fill)
                g.FillRectangle(fillBrush, (float)ptd.X, (float)ptd.Y, size.Width, size.Height);
            g.DrawString(text, imgBox.Font, fontBrush, ptd.ToFloat());
        }

        public void DrawString(string text, double x, double y, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            DrawString(text, new PointD(x, y), fontBrush, fill, fillBrush);
        }

        public void DrawString(string text, Font font, PointD pt, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            PointD ptd = imgBox.ImgToDisp(pt);
            SizeF size = g.MeasureString(text, font);
            if (fill)
                g.FillRectangle(fillBrush, (float)ptd.X, (float)ptd.Y, size.Width, size.Height);
            g.DrawString(text, font, fontBrush, ptd.ToFloat());
        }

        public void DrawString(string text, Font font, double x, double y, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            DrawString(text, font, new PointD(x, y), fontBrush, fill, fillBrush);
        }

        public void DrawStringScreen(string text, PointD pt, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            SizeF size = g.MeasureString(text, imgBox.Font);
            if (fill)
                g.FillRectangle(fillBrush, (float)pt.X, (float)pt.Y, size.Width, size.Height);
            g.DrawString(text, imgBox.Font, fontBrush, pt.ToFloat());
        }

        public void DrawStringScreen(string text, int x, int y, Brush fontBrush, bool fill = false, Brush fillBrush = null) {
            DrawStringScreen(text, new PointD(x, y), fontBrush, fill, fillBrush);
        }
    }
}
