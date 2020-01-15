using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ShimLib {
    public class ZoomPictureBox : Control {
        // 디스플레이용 버퍼
        private int dispBW;
        private int dispBH;
        private IntPtr dispBuf;
        private Bitmap dispBmp;

        // 기본 폰트
        private Font defaultFont = SystemFonts.DefaultFont;
        private Font pixelFont = new Font("돋움", 8);

        // 이미지용 버퍼
        [Browsable(false)]
        public int ImgBW { get; private set; } = 0;
        [Browsable(false)]
        public int ImgBH { get; private set; } = 0;
        [Browsable(false)]
        public IntPtr ImgBuf { get; private set; } = IntPtr.Zero;
        [Browsable(false)]
        public int ImgBytepp { get; private set; } = 1;

        // 생성자
        public ZoomPictureBox() {
            DoubleBuffered = true;
        }

        // 소멸자
        ~ZoomPictureBox() {
            FreeDispBuf();
        }

        // 화면 표시 옵션
        public bool UseDrawPixelValue { get; set; } = true;
        public bool UseDrawInfo { get; set; } = true;
        public bool UseDrawCenterLine { get; set; } = true;
        public bool UseDrawDrawTime { get; set; } = true;

        // 마우스 동작 옵션
        public bool UseMouseMove { get; set; } = true;
        public bool UseMouseWheelZoom { get; set; } = true;

        // 줌 파라미터
        // ZoomLevel = 0 => ZoomFactor = 1;
        // { 1/1024d,  3/2048d,  1/512d,  3/1024d,  1/256d,  3/512d,  1/128d,  3/256d,  1/64d,  3/128d,  1/32d,  3/64d,  1/16d,  3/32d,  1/8d,  3/16d,  1/4d,  3/8d,  1/2d,  3/4d,  1d,  3/2d,  2d,  3d,  4d,  6d,  8d,  12d,  16d,  24d,  32d,  48d,  64d,  96d,  128d,  192d,  256d,  384d,  512d,  768d,  1024d, };
        public int ZoomLevel { get; set; }
        private void GetZoomFactorComponents(out int exp_num, out int c) {
            exp_num = (ZoomLevel >= 0) ? ZoomLevel / 2 : (ZoomLevel - 1) / 2;
            if (ZoomLevel % 2 != 0)
                exp_num--;
            c = (ZoomLevel % 2 != 0) ? 3 : 1;
        }
        public double GetZoomFactor() {
            GetZoomFactorComponents(out int exp_num, out int c);
            return c * Math.Pow(2, exp_num);
        }
        private string GetZoomText() {
            GetZoomFactorComponents(out int exp_num, out int c);
            return (exp_num >= 0) ? (c * (int)Math.Pow(2, exp_num)).ToString() : c.ToString() + "/" + ((int)Math.Pow(2, -exp_num)).ToString();
        }

        // 패닝 파라미터
        public Point PtPanning { get; set; }

        // 이미지 버퍼 세팅
        public void SetImgBuf(IntPtr buf, int bw, int bh, int bytepp) {
            ImgBuf = buf;
            ImgBW = bw;
            ImgBH = bh;
            ImgBytepp = bytepp;

            Invalidate();
        }

        // 사각형을 피팅 되도록 줌 변경
        public void ZoomToRect(int x, int y, int width, int height) {
            double scale1 = (double)ClientRectangle.Width / width;
            double scale2 = (double)ClientRectangle.Height / height;
            double wantedZoomFactor = Math.Min(scale1, scale2);
            ZoomLevel = Util.IntClamp((int)Math.Floor(Math.Log(wantedZoomFactor) / Math.Log(Math.Sqrt(2))), -20, 20);
            double ZoomFactor = GetZoomFactor();
            int panX = (int)Math.Floor((ClientRectangle.Width - width * ZoomFactor) / 2 - x * ZoomFactor);
            int panY = (int)Math.Floor((ClientRectangle.Height - height * ZoomFactor) / 2 - y * ZoomFactor);
            PtPanning = new System.Drawing.Point(panX, panY);
        }

        // 줌 리셋
        public void ZoomReset() {
            ZoomLevel = 0;
            PtPanning = Point.Empty;
        }

        // 리사이즈 할때
        protected override void OnLayout(LayoutEventArgs e) {
            base.OnLayout(e);

            AllocDispBuf();
            Invalidate();
        }

        // 페인트 할때
        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics;
            var t0 = Stopwatch.GetTimestamp();

            Util.CopyImageBufferZoom(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, PtPanning.X, PtPanning.Y, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb());
            var t1 = Stopwatch.GetTimestamp();

            g.DrawImageUnscaledAndClipped(dispBmp, new Rectangle(0, 0, dispBW, dispBH));
            var t2 = Stopwatch.GetTimestamp();

            if (UseDrawPixelValue)
                DrawPixelValue(g);
            var t3 = Stopwatch.GetTimestamp();

            if (UseDrawCenterLine)
                DrawCenterLine(g);
            var t4 = Stopwatch.GetTimestamp();

            base.OnPaint(e);
            var t5 = Stopwatch.GetTimestamp();

            if (UseDrawInfo)
                DrawInfo(g);
            var t6 = Stopwatch.GetTimestamp();
            
            if (UseDrawDrawTime) {
                var freqMs = 1000.0/Stopwatch.Frequency;
                string info =
$@"CopyImage : {(t1-t0)*freqMs:0.0}ms
DrawImage : {(t2-t1)*freqMs:0.0}ms
PixelValue : {(t3-t2)*freqMs:0.0}ms
CenterLine : {(t4-t3)*freqMs:0.0}ms
OnPaint : {(t5-t4)*freqMs:0.0}ms
CursorInfo : {(t6-t5)*freqMs:0.0}ms
Total : {(t6-t0)*freqMs:0.0}ms";
                DrawDrawTime(g, info);
            }
        }

        // 마우스 휠
        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);

            if (!UseMouseWheelZoom)
                return;

            if (Control.ModifierKeys == Keys.Control) {
                WheelScroll(e, true);
            } else if (Control.ModifierKeys == Keys.Shift) {
                WheelScroll(e, false);
            } else {
                bool fixPanning = (Control.ModifierKeys == Keys.Alt);
                WheelZoom(e, fixPanning);
            }
            Invalidate();
        }

        // 휠 스크롤
        private void WheelScroll(MouseEventArgs e, bool vertical) {
            int scroll = (e.Delta > 0) ? 128 : -128;
            if (vertical)
                PtPanning += new Size(0, scroll);
            else
                PtPanning += new Size(scroll, 0);
        }

        // 휠 줌
        private void WheelZoom(MouseEventArgs e, bool fixPanning) {
            var ptImg = DispToImg(e.Location);

            var zoomFacotrOld = GetZoomFactor();
            ZoomLevel = Util.IntClamp((e.Delta > 0) ? ZoomLevel + 1 : ZoomLevel - 1, -20, 20);
            if (fixPanning)
                return;

            var zoomFactorNew = GetZoomFactor();
            int sizeX = (int)Math.Floor(ptImg.X * (zoomFacotrOld - zoomFactorNew));
            int sizeY = (int)Math.Floor(ptImg.Y * (zoomFacotrOld - zoomFactorNew));
            PtPanning += new Size(sizeX, sizeY);
        }

        // 마우스 다운
        bool mouseDown = false;
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                mouseDown = true;
        }

        // 마우스 무브
        Point ptMouseLast;
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            var ptMouse = e.Location;
            if (UseMouseMove && mouseDown)
                PtPanning += ((Size)ptMouse - (Size)ptMouseLast);

            ptMouseLast = ptMouse;
            Invalidate();
        }

        // 마우스 업
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
                mouseDown = false;
        }

        // 표시 버퍼 생성
        private void AllocDispBuf() {
            FreeDispBuf();

            dispBW = Math.Max(ClientSize.Width, 64);
            dispBH = Math.Max(ClientSize.Height, 64);
            dispBuf = Marshal.AllocHGlobal((IntPtr)(dispBW * dispBH * 4));
            dispBmp = new Bitmap(dispBW, dispBH, dispBW * 4, PixelFormat.Format32bppPArgb, dispBuf);
        }

        // 표시 버퍼 해제
        private void FreeDispBuf() {
            if (dispBmp != null)
                dispBmp.Dispose();
            if (dispBuf != IntPtr.Zero)
                Marshal.FreeHGlobal(dispBuf);
        }

        // 중심선 표시
        private void DrawCenterLine(Graphics g) {
            if (ImgBuf == IntPtr.Zero)
                return;

            Pen pen = new Pen(Color.Yellow);
            pen.DashStyle = DashStyle.Dot;

            var rect = ClientRectangle;
            var ptImgL = new PointF(0, ImgBH / 2);
            var ptImgR = new PointF(ImgBW, ImgBH / 2);
            var ptDispL = ImgToDisp(ptImgL);
            var ptDispR = ImgToDisp(ptImgR);
            ptDispL.X = Util.IntClamp(ptDispL.X, -1, rect.Width);
            ptDispR.X = Util.IntClamp(ptDispR.X, -1, rect.Width);
            g.DrawLine(pen, ptDispL, ptDispR);
            var ptImgT = new PointF(ImgBW / 2, 0);
            var ptImgB = new PointF(ImgBW / 2, ImgBH);
            var ptDispT = ImgToDisp(ptImgT);
            var ptDispB = ImgToDisp(ptImgB);
            ptDispT.Y = Util.IntClamp(ptDispT.Y, -1, rect.Height);
            ptDispB.Y = Util.IntClamp(ptDispB.Y, -1, rect.Height);
            g.DrawLine(pen, ptDispT, ptDispB);
            pen.Dispose();
        }

        // 이미지 픽셀값 표시
        Brush[] pseudo = {
            Brushes.White,      // 0~31
            Brushes.LightCyan,  // 32~63
            Brushes.DodgerBlue, // 63~95
            Brushes.Yellow,     // 96~127
            Brushes.Brown,      // 128~159
            Brushes.Magenta,    // 160~191
            Brushes.Red    ,    // 192~223
            Brushes.Black,      // 224~255
        };
        private void DrawPixelValue(Graphics g) {
            double ZoomFactor = GetZoomFactor();
            double pixeValFactor = (ImgBytepp == 1) ? 1 : ((ImgBytepp == 2 ? 5.0/3 : 3));
            if (ZoomFactor < 20 * pixeValFactor)
                return;

            var ptDisp1 = Point.Empty;
            var ptDisp2 = (Point)ClientSize;
            var ptImg1 = DispToImg(ptDisp1);
            var ptImg2 = DispToImg(ptDisp2);
            int imgX1 = Util.IntClamp((int)Math.Floor(ptImg1.X), 0, ImgBW-1);
            int imgY1 = Util.IntClamp((int)Math.Floor(ptImg1.Y), 0, ImgBH-1);
            int imgX2 = Util.IntClamp((int)Math.Floor(ptImg2.X), 0, ImgBW-1);
            int imgY2 = Util.IntClamp((int)Math.Floor(ptImg2.Y), 0, ImgBH-1);

            for (int imgY = imgY1; imgY <= imgY2; imgY++) {
                for (int imgX = imgX1; imgX <= imgX2; imgX++) {
                    var ptImg = new PointF(imgX, imgY);
                    var ptDisp = ImgToDisp(ptImg);
                    string pixelValText = GetImagePixelValueText(imgX, imgY);
                    int pixelVal = GetImagePixelValueAverage(imgX, imgY);
                    var brush = pseudo[pixelVal / 32];
                    g.DrawString(pixelValText.ToString(), pixelFont, brush, ptDisp.X, ptDisp.Y);
                }
            }
        }

        // 좌상단 정보 표시
        private void DrawInfo(Graphics g) {
            Point ptCur = ptMouseLast;
            PointF ptImg = DispToImg(ptCur);
            int imgX = (int)Math.Floor(ptImg.X);
            int imgY = (int)Math.Floor(ptImg.Y);
            string pixelVal = GetImagePixelValueText(imgX, imgY);
            string info = $"zoom={GetZoomText()} ({imgX},{imgY})={pixelVal}";

            var rect = g.MeasureString(info, defaultFont);
            g.FillRectangle(Brushes.White, 0, 0, rect.Width, rect.Height);
            g.DrawString(info, defaultFont, Brushes.Black, 0, 0);
        }

        // 렌더링 시간 표시
        private void DrawDrawTime(Graphics g, string info) {
            var rect = g.MeasureString(info, defaultFont);
            g.FillRectangle(Brushes.White, ClientSize.Width - 150, 0, rect.Width, rect.Height);
            g.DrawString(info, defaultFont, Brushes.Black, ClientSize.Width - 150, 0);
        }

        // 표시 픽셀 좌표를 이미지 좌표로 변환
        public PointF DispToImg(Point pt) {
            double ZoomFactor = GetZoomFactor();
            float x = (float)((pt.X - PtPanning.X) / ZoomFactor);
            float y = (float)((pt.Y - PtPanning.Y) / ZoomFactor);
            return new PointF(x, y);
        }

        // 픽셀 사각형을 이미지 사각형으로 변환
        public RectangleF DispToImg(Rectangle rect) {
            double ZoomFactor = GetZoomFactor();
            float x = (float)((rect.X - PtPanning.X) / ZoomFactor);
            float y = (float)((rect.Y - PtPanning.Y) / ZoomFactor);
            float width = (float)Math.Floor(rect.Width / ZoomFactor);
            float height = (float)Math.Floor(rect.Height / ZoomFactor);
            return new RectangleF(x, y, width, height);
        }

        // 이미지 좌표를 표시 픽셀 좌표로 변환
        public Point ImgToDisp(PointF pt) {
            double ZoomFactor = GetZoomFactor();
            int x = (int)Math.Floor(pt.X * ZoomFactor + PtPanning.X);
            int y = (int)Math.Floor(pt.Y * ZoomFactor + PtPanning.Y);
            return new Point(x, y);
        }

        // 이미지 사각형을 픽셀 사각형으로 변환
        public Rectangle ImgToDisp(RectangleF rect) {
            double ZoomFactor = GetZoomFactor();
            int x = (int)Math.Floor(rect.X * ZoomFactor + PtPanning.X);
            int y = (int)Math.Floor(rect.Y * ZoomFactor + PtPanning.Y);
            int width = (int)Math.Floor(rect.Width * ZoomFactor);
            int height = (int)Math.Floor(rect.Height * ZoomFactor);
            return new Rectangle(x, y, width, height);
        }

        // 이미지 픽셀값 문자열 리턴
        private string GetImagePixelValueText(int x, int y) {
            if (ImgBuf == IntPtr.Zero || x < 0 || x >= ImgBW || y < 0 || y >= ImgBH)
                return "";
            IntPtr ptr = (IntPtr)(ImgBuf.ToInt64() + ((long)ImgBW * y + x) * ImgBytepp);
            if (ImgBytepp == 1)
                return Marshal.ReadByte(ptr).ToString();
            if (ImgBytepp == 2)
                return (Marshal.ReadByte(ptr, 1) | Marshal.ReadByte(ptr) << 8).ToString();
            return $"{Marshal.ReadByte(ptr, 2)},{Marshal.ReadByte(ptr, 1)},{Marshal.ReadByte(ptr, 0)}";
        }

        // 이미지 픽셀값 평균 리턴 (0~255)
        private int GetImagePixelValueAverage(int x, int y) {
            if (ImgBuf == IntPtr.Zero || x < 0 || x >= ImgBW || y < 0 || y >= ImgBH)
                return 0;
            IntPtr ptr = (IntPtr)(ImgBuf.ToInt64() + ((long)ImgBW * y + x) * ImgBytepp);
            if (ImgBytepp == 1)
                return Marshal.ReadByte(ptr);
            if (ImgBytepp == 2)
                return Marshal.ReadByte(ptr);
            return ((int)Marshal.ReadByte(ptr, 2) + (int)Marshal.ReadByte(ptr, 1) + (int)Marshal.ReadByte(ptr, 0)) / 3;
        }
    }
}