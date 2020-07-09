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
using ShimLib.Properties;

namespace ShimLib {
    public delegate void PaintBackbufferEventHandler(object sender, IntPtr buf, int bw, int bh);

    public enum PixelValueRenderer {
        FontAscii_4x6,
        FontAscii_5x8,
    }

    public partial class ImageBox : Control {
        // 백버퍼 그리기
        public event PaintBackbufferEventHandler PaintBackBuffer;
        protected void OnPaintBackBuffer(IntPtr buf, int bw, int bh) {
            if (PaintBackBuffer != null)
                PaintBackBuffer(this, buf, bw, bh);
        }

        // 디스플레이용 버퍼
        private int dispBW;
        private int dispBH;
        private IntPtr dispBuf;
        private Bitmap dispBmp;
        private BufferedGraphics buffGfx;
        private FontRenderer fontAscii4x6;
        private FontRenderer fontAscii5x8;
        public FontRenderer FontRender {
            get {
                if (DrawPixelValueMode == PixelValueRenderer.FontAscii_4x6)
                    return fontAscii4x6;
                return fontAscii5x8;
            }
        }

        // 이미지용 버퍼
        [Browsable(false)]
        public int ImgBW { get; private set; } = 0;
        [Browsable(false)]
        public int ImgBH { get; private set; } = 0;
        [Browsable(false)]
        public IntPtr ImgBuf { get; private set; } = IntPtr.Zero;
        [Browsable(false)]
        public int ImgBytepp { get; private set; } = 1;
        [Browsable(false)]
        public bool BufIsFloat { get; private set; } = false;
        [Browsable(false)]

        // 생성자
        public ImageBox() {
            //DoubleBuffered = true;
            fontAscii4x6 = new FontRenderer(Resources.FontAscii_4x6, 4, 6);
            fontAscii5x8 = new FontRenderer(Resources.FontAscii_5x8, 5, 8);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            FreeDispBuf();
        }

        // 화면 표시 옵션
        public bool UseDrawPixelValue { get; set; } = true;
        public PixelValueRenderer DrawPixelValueMode { get; set; } = PixelValueRenderer.FontAscii_5x8;
        public bool UseDrawInfo { get; set; } = true;
        public bool UseDrawCenterLine { get; set; } = true;
        public bool UseDrawDrawTime { get; set; } = false;
        public Font PixelValueDispFont { get; set; } = new Font("Arial", 6);
        public int PixelValueDispZoomFactorGray8 { get; set; } = 16;
        public int PixelValueDispZoomFactorGray16 { get; set; } = 32;
        public int PixelValueDispZoomFactorRgb { get; set; } = 32;
        public int PixelValueDispZoomFactorFloat { get; set; } = 48;

        // 마우스 동작 옵션
        public bool UseMouseMove { get; set; } = true;
        public bool UseMouseWheelZoom { get; set; } = true;

        // 줌 파라미터
        // ZoomLevel = 0 => ZoomFactor = 1;
        // { 1/1024d,  3/2048d,  1/512d,  3/1024d,  1/256d,  3/512d,  1/128d,  3/256d,  1/64d,  3/128d,  1/32d,  3/64d,  1/16d,  3/32d,  1/8d,  3/16d,  1/4d,  3/8d,  1/2d,  3/4d,  1d,  3/2d,  2d,  3d,  4d,  6d,  8d,  12d,  16d,  24d,  32d,  48d,  64d,  96d,  128d,  192d,  256d,  384d,  512d,  768d,  1024d, };
        private int zoomLevel = 8;
        public int ZoomLevel {
            get {
                return zoomLevel;
            }
            set {
                zoomLevel = Util.Clamp(value, ZoomLevelMin, ZoomLevelMax);
            }
        }
        public int ZoomLevelMin { get; set; } = -16;
        public int ZoomLevelMax { get; set; } = 16;
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
        private int panX = 0;
        private int panY = 0;
        public int PanX {
            get {
                return panX;
            }
            set {
                if (UseMousePanClamp) {
                    if (ImgBuf == IntPtr.Zero)
                        panX = 0;
                    else
                        panX = Util.Clamp(value, (int)Math.Round(-ImgBW * GetZoomFactor()) + 2, 2);
                } else {
                    panX = value;
                }
            }
        }
        public int PanY {
            get {
                return panY;
            }
            set {
                if (UseMousePanClamp) {
                    if (ImgBuf == IntPtr.Zero)
                        panY = 0;
                    else
                        panY = Util.Clamp(value, (int)Math.Round(-ImgBH * GetZoomFactor()) + 2, 2);
                } else {
                    panY = value;
                }
            }
        }
        public bool UseMousePanClamp { get; set; } = true;

        // 이미지 버퍼 세팅
        public void SetImgBuf(IntPtr buf, int bw, int bh, int bytepp, bool bInvalidate, bool bufIsFloat = false) {
            ImgBuf = buf;
            ImgBW = bw;
            ImgBH = bh;
            ImgBytepp = bytepp;

            BufIsFloat = bufIsFloat;

            if (bInvalidate)
                Redraw();
        }

        // 사각형을 피팅 되도록 줌 변경
        public void ZoomToRect(int x, int y, int width, int height) {
            double scale1 = (double)ClientRectangle.Width / width;
            double scale2 = (double)ClientRectangle.Height / height;
            double wantedZoomFactor = Math.Min(scale1, scale2);
            ZoomLevel = (int)Math.Round(Math.Log(wantedZoomFactor) / Math.Log(Math.Sqrt(2)));
            double ZoomFactor = GetZoomFactor();
            PanX = (int)Math.Round((ClientRectangle.Width - width * ZoomFactor) / 2 - x * ZoomFactor);
            PanY = (int)Math.Round((ClientRectangle.Height - height * ZoomFactor) / 2 - y * ZoomFactor);
        }

        // 줌 리셋
        public void ZoomReset() {
            ZoomLevel = 0;
            PanX = 0;
            PanY = 0;
        }

        // 리사이즈 할때
        protected override void OnLayout(LayoutEventArgs e) {
            base.OnLayout(e);

            AllocDispBuf();
            Redraw();
        }

        public void Redraw() {
            var dispRect = new Rectangle(0, 0, dispBW, dispBH);

            var t0 = Util.GetTimeMs();

            CopyImageBufferZoom(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, PanX, PanY, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb(), BufIsFloat);
            var t1 = Util.GetTimeMs();

            var imgDraw = new ImageDrawing(this, dispBuf, dispBW, dispBH);

            if (UseDrawPixelValue) {
                DrawPixelValueBitmap(imgDraw);
            }
            if (UseDrawCenterLine)
                DrawCenterLine(imgDraw);
            var t2 = Util.GetTimeMs();

            OnPaintBackBuffer(dispBuf, dispBW, dispBH);
            var t3 = Util.GetTimeMs();

            var bmpG = Graphics.FromImage(dispBmp);
            base.OnPaint(new PaintEventArgs(bmpG, dispRect));
            var t4 = Util.GetTimeMs();

            bmpG.Dispose();
            buffGfx.Graphics.DrawImageUnscaledAndClipped(dispBmp, dispRect);
            var t5 = Util.GetTimeMs();

            var ig = new ImageGraphics(this, buffGfx.Graphics);
            if (UseDrawInfo)
                DrawInfo(ig);
            if (UseDrawDrawTime) {
                string info =
$@"== Image ==
{(ImgBuf == IntPtr.Zero ? "X" : $"{ImgBW}*{ImgBH}*{ImgBytepp * 8}bpp({(BufIsFloat ? "float" : "byte")})")}

== Draw option ==
DrawPixelValue : {(UseDrawPixelValue ? "O" : "X")}
 -> : {DrawPixelValueMode}
DrawInfo : {(UseDrawInfo ? "O" : "X")}
DrawCenterLine : {(UseDrawCenterLine ? "O" : "X")}
DrawDrawTime : {(UseDrawDrawTime ? "O" : "X")}

== Mouse option ==
MouseMove : {(UseMouseMove ? "O" : "X")}
MouseWheelZoom : {(UseMouseWheelZoom ? "O" : "X")}
UseMousePanClamp : {(UseMousePanClamp ? "O" : "X")}
Pan : ({PanX},{PanY})
ZoomLevel : {ZoomLevel}
ZoomLevelMin : {ZoomLevelMin}
ZoomLevelMax : {ZoomLevelMax}

== Draw time ==
ZoomImage : {t1 - t0:0.0}ms
DrawInfo : {t2 - t1:0.0}ms
OnPaintBuffer : {t3 - t2:0.0}ms
OnPaint : {t4 - t3:0.0}ms
DrawImage : {t5 - t4:0.0}ms
Total : {t5 - t0:0.0}ms
";
                DrawDrawTime(ig, info);
            }

            buffGfx.Render();
        }

        // 백그라운 처리에서 아무것도 안하도록 함
        protected override void OnPaintBackground(PaintEventArgs pevent) {
        }

        // 페인트 할때
        protected override void OnPaint(PaintEventArgs e) {
            Redraw();
        }

        // 이미지 버퍼를 디스플레이 버퍼에 복사
        private static unsafe void CopyImageBufferZoom(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, Int64 panx, Int64 pany, double zoom, int bytepp, int bgColor, bool bufIsFloat) {
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
            Redraw();
        }

        // 휠 스크롤
        private void WheelScroll(MouseEventArgs e, bool vertical) {
            int scroll = (e.Delta > 0) ? 128 : -128;
            if (vertical)
                PanY += scroll;
            else
                PanX += scroll;
        }

        // 휠 줌
        private void WheelZoom(MouseEventArgs e, bool fixPanning) {
            var zoomFactorOld = GetZoomFactor();
            ZoomLevel = (e.Delta > 0) ? ZoomLevel + 1 : ZoomLevel - 1;
            if (fixPanning)
                return;

            var zoomFactorNew = GetZoomFactor();

            var zoomFactorDelta = zoomFactorNew / zoomFactorOld;
            var ptX = (PanX - e.Location.X) * zoomFactorDelta + e.Location.X;
            var ptY = (PanY - e.Location.Y) * zoomFactorDelta + e.Location.Y;
            PanX = (int)Math.Round(ptX);
            PanY = (int)Math.Round(ptY);
        }

        // 마우스 다운
        bool mouseDown = false;
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left) {
                mouseDown = true;
            }
            ptMouseLast = e.Location;
        }

        // 마우스 무브
        Point ptMouseLast;
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (UseMouseMove && mouseDown) {
                PanX += e.Location.X - ptMouseLast.X;
                PanY += e.Location.Y - ptMouseLast.Y;
                Redraw();
            }
            ptMouseLast = e.Location;

            if (UseDrawInfo) {
                using (Graphics g = this.CreateGraphics()) {
                    ImageGraphics ig = new ImageGraphics(this, g);
                    DrawInfo(ig);
                }
            }
        }

        // 마우스 업
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
                mouseDown = false;
        }

        // ctrl + doubleclick 누를때 어바웃창 띄움
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control) && e.Button == MouseButtons.Left) {
                ShowAbout();
            } else {
                base.OnDoubleClick(e);
            }
        }

        // 어바웃 / 옵션 창 띄움
        public void ShowAbout() {
            var frmAbout = new FormAbout(this);
            frmAbout.ShowDialog(this);
        }

        // 표시 버퍼 생성
        private void AllocDispBuf() {
            FreeDispBuf();

            dispBW = Math.Max(ClientSize.Width, 64);
            dispBH = Math.Max(ClientSize.Height, 64);
            dispBuf = Util.AllocBuffer(dispBW * dispBH * 4);
            dispBmp = new Bitmap(dispBW, dispBH, dispBW * 4, PixelFormat.Format32bppPArgb, dispBuf);
            buffGfx = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), new Rectangle(0, 0, dispBW, dispBH));
        }

        // 표시 버퍼 해제
        private void FreeDispBuf() {
            if (dispBmp != null)
                dispBmp.Dispose();
            if (dispBuf != IntPtr.Zero)
                Util.FreeBuffer(ref dispBuf);
            if (buffGfx != null)
                buffGfx.Dispose();
        }

        // 중심선 표시
        private void DrawCenterLine(ImageDrawing id) {
            if (ImgBuf == IntPtr.Zero)
                return;

            Color color = Color.Yellow;
            id.DrawLine(-0.5f, ImgBH / 2.0f - 0.5f, ImgBW - 0.5f, ImgBH / 2.0f - 0.5f, color);
            id.DrawLine(ImgBW / 2.0f - 0.5f, -0.5f, ImgBW / 2.0f - 0.5f, ImgBH - 0.5f, color);
        }

        // 이미지 픽셀값 표시
        private static readonly Color[] pseudo = {
            Color.White,      // 0~31
            Color.Cyan,       // 32~63
            Color.DodgerBlue, // 63~95
            Color.Yellow,     // 96~127
            Color.Brown,      // 128~159
            Color.DarkViolet, // 160~191
            Color.Red    ,    // 192~223
            Color.Black,      // 224~255
        };

        private void DrawPixelValueBitmap(ImageDrawing imgDrw) {
            double ZoomFactor = GetZoomFactor();
            if (BufIsFloat) {
                if (ZoomFactor < PixelValueDispZoomFactorFloat)
                    return;
            } else {
                if (ImgBytepp == 1)
                if (ZoomFactor < PixelValueDispZoomFactorGray8)
                    return;
                if (ImgBytepp == 2)
                if (ZoomFactor < PixelValueDispZoomFactorGray16)
                    return;
                if (ImgBytepp == 3 || ImgBytepp == 4)
                if (ZoomFactor < PixelValueDispZoomFactorRgb)
                    return;
            }

            var ptDisp1 = new Point(0, 0);
            var ptDisp2 = new Point(ClientSize.Width, ClientSize.Height);
            var ptImg1 = DispToImg(ptDisp1);
            var ptImg2 = DispToImg(ptDisp2);
            int imgX1 = Util.Clamp((int)Math.Round(ptImg1.X), 0, ImgBW - 1);
            int imgY1 = Util.Clamp((int)Math.Round(ptImg1.Y), 0, ImgBH - 1);
            int imgX2 = Util.Clamp((int)Math.Round(ptImg2.X), 0, ImgBW - 1);
            int imgY2 = Util.Clamp((int)Math.Round(ptImg2.Y), 0, ImgBH - 1);

            for (int imgY = imgY1; imgY <= imgY2; imgY++) {
                for (int imgX = imgX1; imgX <= imgX2; imgX++) {
                    string pixelValText = GetImagePixelValueText(imgX, imgY);
                    if (!BufIsFloat && ImgBytepp == 3 || ImgBytepp == 4)
                        pixelValText = pixelValText.Replace(",", "\r\n");

                    int pixelVal = GetImagePixelValueAverage(imgX, imgY);
                    PointF ptImg = new PointF(imgX - 0.5f, imgY - 0.5f);
                    var color = pseudo[pixelVal / 32];
                    imgDrw.DrawString(pixelValText, ptImg, color);
                }
            }
        }

        // 좌상단 정보 표시
        private void DrawInfo(ImageGraphics ig) {
            Point ptCur = ptMouseLast;
            PointF ptImg = DispToImg(ptCur);
            int imgX = (int)Math.Round(ptImg.X);
            int imgY = (int)Math.Round(ptImg.Y);
            string pixelVal = GetImagePixelValueText(imgX, imgY);
            string info = $"zoom={GetZoomText()} ({imgX},{imgY})={pixelVal}";

            var size = ig.g.MeasureString("0", ig.imgBox.Font);
            using (Bitmap bmp = new Bitmap(250, (int)size.Height)) {
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.FillRectangle(Brushes.Black, 0, 0, 250, size.Height);
                    g.DrawString(info, ig.imgBox.Font, Brushes.White, 0, 0);
                }
                ig.g.DrawImage(bmp, 2, 2);
            }
        }

        // 렌더링 시간 표시
        private void DrawDrawTime(ImageGraphics ig, string info) {
            ig.DrawStringWnd(info, new Point(ClientSize.Width - 150, 0), null, Brushes.Black, Brushes.White);
        }

        // 표시 픽셀 좌표를 이미지 좌표로 변환
        public PointF DispToImg(Point pt) {
            double ZoomFactor = GetZoomFactor();
            double x = (pt.X - PanX) / ZoomFactor - 0.5;
            double y = (pt.Y - PanY) / ZoomFactor - 0.5;
            return new PointF((float)x, (float)y);
        }

        // 이미지 좌표를 표시 픽셀 좌표로 변환
        public Point ImgToDisp(PointF pt) {
            double ZoomFactor = GetZoomFactor();
            double x = (pt.X + 0.5) * ZoomFactor + PanX;
            double y = (pt.Y + 0.5) * ZoomFactor + PanY;
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        // 이미지 픽셀값 문자열 리턴
        private unsafe string GetImagePixelValueText(int x, int y) {
            if (ImgBuf == IntPtr.Zero || x < 0 || x >= ImgBW || y < 0 || y >= ImgBH)
                return "";

            byte* ptr = (byte*)ImgBuf + ((long)ImgBW * y + x) * ImgBytepp;

            if (!BufIsFloat) {
                if (ImgBytepp == 1)
                    return $"{*ptr}";
                if (ImgBytepp == 2)
                    return (ptr[1] | ptr[0] << 8).ToString();
                else
                    return $"{ptr[2]},{ptr[1]},{ptr[0]}";
            } else {
                if (ImgBytepp == 4)
                    return $"{(*(float*)ptr):f2}";
                else
                    return $"{(*(double*)ptr):f2}";
            }
        }

        // 이미지 픽셀값 평균 리턴 (0~255)
        private unsafe int GetImagePixelValueAverage(int x, int y) {
            if (ImgBuf == IntPtr.Zero || x < 0 || x >= ImgBW || y < 0 || y >= ImgBH)
                return 0;

            IntPtr ptr = (IntPtr)(ImgBuf.ToInt64() + ((long)ImgBW * y + x) * ImgBytepp);

            if (!BufIsFloat) {
                if (ImgBytepp == 1)
                    return Marshal.ReadByte(ptr);
                if (ImgBytepp == 2)
                    return Marshal.ReadByte(ptr);
                else
                    return ((int)Marshal.ReadByte(ptr, 2) + (int)Marshal.ReadByte(ptr, 1) + (int)Marshal.ReadByte(ptr, 0)) / 3;
            } else {
                if (ImgBytepp == 4)
                    return Util.Clamp((int)*(float*)ptr, 0, 255);
                else
                    return Util.Clamp((int)*(double*)ptr, 0, 255);
            }
        }
    }
}