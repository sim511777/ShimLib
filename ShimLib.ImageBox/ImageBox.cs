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
using PointD = System.Windows.Point;

namespace ShimLib {
    public class ImageBox : Control {
        public const string VersionHistory =
@"ImageBox for .NET

v1.0.0.14 - 20200325
1. ZoomLevelMin, ZoomLevelMax 속성 추가
2. UseMousePanClamp 속성 추가

v1.0.0.13 - 20200320
1. 16비트 hra버퍼 저장 시 24비트 bmp로 저장 되도록 수정

v1.0.0.12 - 20200316
1. float, double buffer 표시 기능 추가
2. float, double buffer 전처리 해서 표시 기능 추가
3. 버파 파일저장 시 8bit png 포멧으로 저장되는 버그 수정

v1.0.0.11 - 20200315
1. 옵션창에서 버퍼 파일저장, 버퍼 클립보드 복사 추가

v1.0.0.10 - 20200308
1. 개별 픽셀 표시 폰트와 일반 표시 폰트 두가지 따로 갈것
2. pseudo 기본 컬러 더 잘보이는 것으로 수정

v1.0.0.9 - 20200305
1. BufferedGraphics 안쓰고 DoubleBuffered = true; 사용
2. MouseMove 오동작 수정

v1.0.0.8 - 20200304
1. 버전정보 창에 속성 변경기능 추가
2. 쿼드클릭 대신 ctrl + 더블클릭 누를때 버전정보 창 띄움
3. ShowAbout() 함수 추가

v1.0.0.7 - 20200217
1. DrawInfo() 깜빡이 않게 더블버퍼 처리

v1.0.0.6 - 20200214
1. ImageBox로 이름 변경
2. ImgToDisp(), DIspToImg() 함수 PointD 타입으로 변경
3. ImageGraphics 클래스 추가 및 테스트
4. immediate 드로잉 마우스 Move시에 안지워지도록 수정
5. PixelValueDispZoomFactor 속성 추가

v1.0.0.5 - 20200131
1. Quadruple 클릭시 버전정보창 띄움 (마우스 다운이 아닌 마우스 업에서 처리)
2. PtPanning => double PanX, PanY 로 변경
3. 더 큰 이미지 (2000000width) 표시시 CenterLine 오버플로우 다운 수정
4. 더 큰 이미지 (2000000width) 표시시 PanX, PanY int타입 오버플로우로 발생하는 계산 에러 수정
5. 더 큰 이미지 (2000000width) 표시를 위해서 zoom레벨 (1/10000000x) ~ (1000000x)로 수정
6. DrawString()시 Control.Font 사용

v1.0.0.4 - 20200129
1. 필터링시 +0.5 offset 추가
2. panning 좌표 FloatF 로 변경 하여 축소 확대시 위치 안벗어남
3. Native.dll 불필요 해서 지움
4. Quadruple 클릭시 버전정보창 띄움
5. PtPanning 속성 숨김

v1.0.0.3 - 20200127
1. 필터링시 가장자리 0.5픽셀 처리 안하던것 처리하도록 수정
2. SetImgBuf 함수에 bInvalidate 파라미터 추가

v1.0.0.2 - 20200119
1. 레스터라이즈 Parallel 라이브러리 사용해서멀티쓰레드 처리
2. C++ dll 프로젝트 추가
3. Draw Time 표시에 추가 정보 포함(이미지버퍼, 드로우옵션, 마우스옵션) 

v1.0.0.1 - 20200116
1. 확대시 선형보간 기능 추가
2. DispToImg(Rectangle rect) 버그 수정 - Floor 하지 말아야 함

v1.0.0.0 - 20200115
1. 기본 기능 완성
2. 버전정보 추가

v0.1.0.1 - 20191201
1. 기본 알고리즘 구현

v0.0.0.0 - 20191001
1. 설계
  - Native 이미지 버퍼 표시 기능
  - 아주 큰 이미지도 표시 가능
  - 마우스 줌, 마우스 패닝 기능
  - 확대 되었을때 픽셀값 표시
  - 마우스 이동시 커서 위치의 픽셀좌표와 픽셀값 표시
  - C, C# 모두 구현하여 속도 비교 테스트
  - 닷넷 컨트롤로 구현하여 폼디자이너에서 사용하기 쉽게 만듦
";

        // 디스플레이용 버퍼
        private int dispBW;
        private int dispBH;
        private IntPtr dispBuf;
        private Bitmap dispBmp;

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
        public bool FloatPreprocessed { get; private set; } = false;

        // Float 버퍼용 전처리 버퍼
        private IntPtr grayBuf = IntPtr.Zero;

        // 생성자
        public ImageBox() {
            DoubleBuffered = true;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            FreeDispBuf();
            Util.FreeBuffer(ref grayBuf);
        }

        // 화면 표시 옵션
        public bool UseDrawPixelValue { get; set; } = true;
        public bool UseDrawInfo { get; set; } = true;
        public bool UseDrawCenterLine { get; set; } = true;
        public bool UseDrawDrawTime { get; set; } = false;
        public bool UseInterPorlation { get; set; } = false;
        public bool UseParallel { get; set; } = false;
        public Font PixelValueDispFont { get; set; } = SystemFonts.DefaultFont;
        public int PixelValueDispZoomFactor { get; set; } = 20;

        // 마우스 동작 옵션
        public bool UseMouseMove { get; set; } = true;
        public bool UseMouseWheelZoom { get; set; } = true;

        // 줌 파라미터
        // ZoomLevel = 0 => ZoomFactor = 1;
        // { 1/1024d,  3/2048d,  1/512d,  3/1024d,  1/256d,  3/512d,  1/128d,  3/256d,  1/64d,  3/128d,  1/32d,  3/64d,  1/16d,  3/32d,  1/8d,  3/16d,  1/4d,  3/8d,  1/2d,  3/4d,  1d,  3/2d,  2d,  3d,  4d,  6d,  8d,  12d,  16d,  24d,  32d,  48d,  64d,  96d,  128d,  192d,  256d,  384d,  512d,  768d,  1024d, };
        private int zoomLevel = 0;
        public int ZoomLevel {
            get {
                return zoomLevel;
            }
            set {
                zoomLevel = Util.Clamp(value, ZoomLevelMin, ZoomLevelMax);
            }
        }
        public int ZoomLevelMin { get; set; } = -12;
        public int ZoomLevelMax { get; set; } = 12; 
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
        private double panX = 0;
        private double panY = 0;
        public double PanX {
            get {
                return panX;
            }
            set {
                if (UseMousePanClamp) {
                    if (ImgBuf == IntPtr.Zero)
                        panX = 0;
                    else
                        panX = Util.Clamp(value, -ImgBW * GetZoomFactor(), 0);
                } else {
                    panX = value;
                }
            }
        }
        public double PanY {
            get {
                return panY;
            }
            set {
                if (UseMousePanClamp) {
                    if (ImgBuf == IntPtr.Zero)
                        panY = 0;
                    else
                        panY = Util.Clamp(value, -ImgBH * GetZoomFactor(), 0); 
                } else {
                    panY = value;
                }
            }
        }
        public bool UseMousePanClamp { get; set; } = true;

        // 이미지 버퍼 세팅
        public void SetImgBuf(IntPtr buf, int bw, int bh, int bytepp, bool bInvalidate) {
            ImgBuf = buf;
            ImgBW = bw;
            ImgBH = bh;
            ImgBytepp = bytepp;
            
            BufIsFloat = false;
            FloatPreprocessed = false;
            Util.FreeBuffer(ref grayBuf);
            
            if (bInvalidate)
                Invalidate();
        }

        // float, double 버퍼 세팅
        public void SetFloatBuf(IntPtr buf, int bw, int bh, int bytepp, bool preprocess, bool bInvalidate) {
            ImgBuf = buf;
            ImgBW = bw;
            ImgBH = bh;
            ImgBytepp = bytepp;
            
            BufIsFloat = true;
            FloatPreprocessed = preprocess;
            Util.FreeBuffer(ref grayBuf);
            if (preprocess) {
                grayBuf = Util.AllocBuffer(bw * bh);
                Util.FloatBufToByte(buf, bw, bh, bytepp, grayBuf);
            }

            if (bInvalidate)
                Invalidate();
        }

        // 사각형을 피팅 되도록 줌 변경
        public void ZoomToRect(int x, int y, int width, int height) {
            double scale1 = (double)ClientRectangle.Width / width;
            double scale2 = (double)ClientRectangle.Height / height;
            double wantedZoomFactor = Math.Min(scale1, scale2);
            ZoomLevel = (int)Math.Floor(Math.Log(wantedZoomFactor) / Math.Log(Math.Sqrt(2)));
            double ZoomFactor = GetZoomFactor();
            PanX = (ClientRectangle.Width - width * ZoomFactor) / 2 - x * ZoomFactor;
            PanY = (ClientRectangle.Height - height * ZoomFactor) / 2 - y * ZoomFactor;
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
            Invalidate();
        }

        // 페인트 할때
        protected override void OnPaint(PaintEventArgs e) {
            var t0 = Util.GetTimeMs();

            if (UseInterPorlation) {
                if (BufIsFloat) {
                    if (FloatPreprocessed)
                        Util.CopyImageBufferZoomIpl(grayBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), 1, this.BackColor.ToArgb(), UseParallel);
                    else
                        Util.CopyImageBufferZoomIplFloat(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb(), UseParallel);
                } else {
                    Util.CopyImageBufferZoomIpl(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb(), UseParallel);
                }
            } else {
                if (BufIsFloat) {
                    if (FloatPreprocessed)
                        Util.CopyImageBufferZoom(grayBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), 1, this.BackColor.ToArgb(), UseParallel);
                    else
                        Util.CopyImageBufferZoomFloat(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb(), UseParallel);
                } else {
                    Util.CopyImageBufferZoom(ImgBuf, ImgBW, ImgBH, dispBuf, dispBW, dispBH, (Int64)PanX, (Int64)PanY, GetZoomFactor(), ImgBytepp, this.BackColor.ToArgb(), UseParallel);
                }
            }
            var t1 = Util.GetTimeMs();

            e.Graphics.DrawImageUnscaledAndClipped(dispBmp, new Rectangle(0, 0, dispBW, dispBH));
            var t2 = Util.GetTimeMs();

            var ig = new ImageGraphics(this, e.Graphics);
            if (UseDrawPixelValue)
                DrawPixelValue(ig);
            var t3 = Util.GetTimeMs();

            if (UseDrawCenterLine)
                DrawCenterLine(ig);
            var t4 = Util.GetTimeMs();

            base.OnPaint(e);
            var t5 = Util.GetTimeMs();

            if (UseDrawInfo)
                DrawInfo(ig);
            var t6 = Util.GetTimeMs();

            if (UseDrawDrawTime) {
                string info =
$@"== Image ==
{(ImgBuf == IntPtr.Zero ? "X" : $"{ImgBW}*{ImgBH}*{ImgBytepp * 8}bpp({(BufIsFloat?"float":"byte")})")}

== Draw option ==
DrawPixelValue : {(UseDrawPixelValue ? "O" : "X")}
DrawInfo : {(UseDrawInfo ? "O" : "X")}
DrawCenterLine : {(UseDrawCenterLine ? "O" : "X")}
DrawDrawTime : {(UseDrawDrawTime ? "O" : "X")}
Interpolation : {(UseInterPorlation ? "O" : "X")}
Parallel : {(UseParallel ? "O" : "X")}

== Mouse option ==
MouseMove : {(UseMouseMove ? "O" : "X")}
MouseWheelZoom : {(UseMouseWheelZoom ? "O" : "X")}
UseMousePanClamp : {(UseMousePanClamp ? "O" : "X")}
Pan : ({PanX},{PanY})
ZoomLevel : {ZoomLevel}
ZoomLevelMin : {ZoomLevelMin}
ZoomLevelMax : {ZoomLevelMax}

== Draw time ==
CopyImage : {t1 - t0:0.0}ms
DrawImage : {t2 - t1:0.0}ms
PixelValue : {t3 - t2:0.0}ms
CenterLine : {t4 - t3:0.0}ms
OnPaint : {t5 - t4:0.0}ms
CursorInfo : {t6 - t5:0.0}ms
Total : {t6 - t0:0.0}ms
";
                DrawDrawTime(ig, info);
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
            PanX = ptX;
            PanY = ptY;
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
                Invalidate();
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
            var dr = frmAbout.ShowDialog(this);
            if (dr == DialogResult.OK)
                this.Invalidate();
        }

        // 표시 버퍼 생성
        private void AllocDispBuf() {
            FreeDispBuf();

            dispBW = Math.Max(ClientSize.Width, 64);
            dispBH = Math.Max(ClientSize.Height, 64);
            dispBuf = Util.AllocBuffer(dispBW * dispBH * 4);
            dispBmp = new Bitmap(dispBW, dispBH, dispBW * 4, PixelFormat.Format32bppPArgb, dispBuf);
        }

        // 표시 버퍼 해제
        private void FreeDispBuf() {
            if (dispBmp != null)
                dispBmp.Dispose();
            if (dispBuf != IntPtr.Zero)
                Util.FreeBuffer(ref dispBuf);
        }

        // 중심선 표시
        private void DrawCenterLine(ImageGraphics ig) {
            if (ImgBuf == IntPtr.Zero)
                return;

            Pen pen = new Pen(Color.Yellow) {
                DashStyle = DashStyle.Dot
            };

            PointD ptLeft = new PointD(0, ImgBH / 2);
            PointD ptRight = new PointD(ImgBW, ImgBH / 2);
            PointD ptdLeft = ImgToDisp(ptLeft);
            PointD ptdRight = ImgToDisp(ptRight);

            int ClientWidth = ClientSize.Width;
            int ClientHeight = ClientSize.Height;
            if (ptdLeft.Y >= 0 && ptdLeft.Y < ClientHeight && ptdRight.X >= 0 && ptdLeft.X < ClientWidth) {
                ptdLeft.X = Util.Clamp(ptdLeft.X, 0, ClientWidth);
                ptdRight.X = Util.Clamp(ptdRight.X, 0, ClientWidth);
                ptLeft = DispToImg(ptdLeft);
                ptRight = DispToImg(ptdRight);
                ig.DrawLine(ptLeft, ptRight, pen);
            }

            PointD ptTop = new PointD(ImgBW / 2, 0);
            PointD ptBottom = new PointD(ImgBW / 2, ImgBH);
            PointD ptdTop = ImgToDisp(ptTop);
            PointD ptdBottom = ImgToDisp(ptBottom);

            if (ptdTop.X >= 0 && ptdTop.X < ClientWidth && ptdBottom.Y >= 0 && ptTop.Y < ClientHeight) {
                ptdTop.Y = Util.Clamp(ptdTop.Y, 0, ClientHeight);
                ptdBottom.Y = Util.Clamp(ptdBottom.Y, 0, ClientHeight);
                ptTop = DispToImg(ptdTop);
                ptBottom = DispToImg(ptdBottom);
                ig.DrawLine(ptTop, ptBottom, pen);
            }
        }

        // 이미지 픽셀값 표시
        private static readonly Brush[] pseudo = {
            Brushes.White,      // 0~31
            Brushes.Cyan,       // 32~63
            Brushes.DodgerBlue, // 63~95
            Brushes.Yellow,     // 96~127
            Brushes.Brown,      // 128~159
            Brushes.DarkViolet, // 160~191
            Brushes.Red    ,    // 192~223
            Brushes.Black,      // 224~255
        };

        private void DrawPixelValue(ImageGraphics ig) {
            double ZoomFactor = GetZoomFactor();
            double pixeValFactor = Util.Clamp(ImgBytepp, 1, 3);
            if (BufIsFloat)
                pixeValFactor *= 0.6;
            if (ZoomFactor < PixelValueDispZoomFactor * pixeValFactor)
                return;

            var ptDisp1 = new PointD(0, 0);
            var ptDisp2 = new PointD(ClientSize.Width, ClientSize.Height);
            var ptImg1 = DispToImg(ptDisp1);
            var ptImg2 = DispToImg(ptDisp2);
            int imgX1 = Util.Clamp((int)Math.Floor(ptImg1.X), 0, ImgBW - 1);
            int imgY1 = Util.Clamp((int)Math.Floor(ptImg1.Y), 0, ImgBH - 1);
            int imgX2 = Util.Clamp((int)Math.Floor(ptImg2.X), 0, ImgBW - 1);
            int imgY2 = Util.Clamp((int)Math.Floor(ptImg2.Y), 0, ImgBH - 1);

            for (int imgY = imgY1; imgY <= imgY2; imgY++) {
                for (int imgX = imgX1; imgX <= imgX2; imgX++) {
                    string pixelValText = GetImagePixelValueText(imgX, imgY);
                    int pixelVal = GetImagePixelValueAverage(imgX, imgY);
                    var brush = pseudo[pixelVal / 32];
                    ig.DrawString(pixelValText, PixelValueDispFont, new PointD(imgX, imgY), brush);
                }
            }
        }

        // 좌상단 정보 표시
        private void DrawInfo(ImageGraphics ig) {
            Point ptCur = ptMouseLast;
            PointD ptImg = DispToImg(ptCur.ToDouble());
            int imgX = (int)Math.Floor(ptImg.X);
            int imgY = (int)Math.Floor(ptImg.Y);
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
            ig.DrawStringScreen(info, ClientSize.Width - 150, 0, Brushes.Black, true, Brushes.White);
        }

        // 표시 픽셀 좌표를 이미지 좌표로 변환
        public PointD DispToImg(PointD pt) {
            double ZoomFactor = GetZoomFactor();
            double x = (pt.X - PanX) / ZoomFactor;
            double y = (pt.Y - PanY) / ZoomFactor;
            return new PointD(x, y);
        }

        // 이미지 좌표를 표시 픽셀 좌표로 변환
        public PointD ImgToDisp(PointD pt) {
            double ZoomFactor = GetZoomFactor();
            double x = Math.Floor(pt.X * ZoomFactor + PanX);
            double y = Math.Floor(pt.Y * ZoomFactor + PanY);
            return new PointD(x, y);
        }

        // 이미지 픽셀값 문자열 리턴
        private unsafe string GetImagePixelValueText(int x, int y) {
            if (ImgBuf == IntPtr.Zero || x < 0 || x >= ImgBW || y < 0 || y >= ImgBH)
                return "";

            IntPtr ptr = (IntPtr)(ImgBuf.ToInt64() + ((long)ImgBW * y + x) * ImgBytepp);
            
            if (!BufIsFloat) {
                if (ImgBytepp == 1)
                    return Marshal.ReadByte(ptr).ToString();
                if (ImgBytepp == 2)
                    return (Marshal.ReadByte(ptr, 1) | Marshal.ReadByte(ptr) << 8).ToString();
                else
                    return $"{Marshal.ReadByte(ptr, 2)},{Marshal.ReadByte(ptr, 1)},{Marshal.ReadByte(ptr, 0)}";
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

    public static class PointDExtension {
        public static PointD ToDouble(this PointF pt) {
            return new PointD(pt.X, pt.Y);
        }

        public static PointD ToDouble(this Point pt) {
            return new PointD(pt.X, pt.Y);
        }

        public static PointF ToFloat(this PointD pt) {
            return new PointF((float)pt.X, (float)pt.Y);
        }

        public static Point ToInt(this PointD pt) {
            return new Point((int)pt.X, (int)pt.Y);
        }
    }
}