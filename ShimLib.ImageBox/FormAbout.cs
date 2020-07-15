using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimLib {
    public partial class FormAbout : Form {
        private ImageBox pbx;
        public FormAbout(ImageBox pbx) {
            this.pbx = pbx;
            InitializeComponent();
        }

        ImageBoxOption optBackup;

        private void FormAbout_Load(object sender, EventArgs e) {
            tbxVersion.Text = ImageBox.VersionHistory;
            optBackup = new ImageBoxOption();
            optBackup.FromImageBox(pbx);
            ImageBoxOption option = new ImageBoxOption();
            option.FromImageBox(pbx);
            grdOption.SelectedObject = option;
        }

        private void FormAbout_FormClosed(object sender, FormClosedEventArgs e) {
            if (this.DialogResult == DialogResult.Cancel) {
                optBackup.ToImageBox(pbx);
                pbx.Redraw();
                return;
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            if (pbx.ImgBuf == IntPtr.Zero) {
                MessageBox.Show(this, "pbx.ImgBuf == IntPtr.Zero");
                return;
            }

            if (pbx.BufIsFloat) {
                MessageBox.Show(this, "Floating point image buffer can not be converted to Bitmap object.");
                return;
            }

            var ok = dlgSaveFile.ShowDialog(this);
            if (ok != DialogResult.OK)
                return;

            ImageUtil.SaveBmpFile(dlgSaveFile.FileName, pbx.ImgBuf, pbx.ImgBW, pbx.ImgBH, pbx.ImgBytepp);
        }

        private void btnCopy_Click(object sender, EventArgs e) {
            if (pbx.ImgBuf == IntPtr.Zero) {
                return;
            }

            if (pbx.BufIsFloat) {
                return;
            }

            var bmp = ImageUtil.ImageBufferToBitmap(pbx.ImgBuf, pbx.ImgBW, pbx.ImgBH, pbx.ImgBytepp);
            Clipboard.SetImage(bmp);
            bmp.Dispose();
        }

        private void grdOption_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            var option = grdOption.SelectedObject as ImageBoxOption;
            option.ToImageBox(pbx);
            pbx.Refresh();
        }
    }

    public class ImageBoxOption {
        // 화면 표시 옵션
        public bool UseDrawPixelValue { get; set; }
        public PixelValueRenderer DrawPixelValueMode { get; set; }
        public bool UseDrawInfo { get; set; }
        public bool UseDrawCenterLine { get; set; }
        public bool UseDrawDrawTime { get; set; }
        // 표시 옵션
        public Color BackColor { get; set; }
        public Font Font { get; set; }
        public Font PixelValueDispFont { get; set; }
        public int PixelValueDispZoomFactorGray8 { get; set; }
        public int PixelValueDispZoomFactorGray16 { get; set; }
        public int PixelValueDispZoomFactorRgb { get; set; }
        public int PixelValueDispZoomFactorFloat { get; set; }
        // 마우스 동작 옵션
        public bool UseMouseMove { get; set; }
        public bool UseMouseWheelZoom { get; set; }
        // 줌 파라미터
        public int ZoomLevel { get; set; }
        public int ZoomLevelMin { get; set; }
        public int ZoomLevelMax { get; set; }
        // 패닝 파라미터
        public int PanX { get; set; }
        public int PanY { get; set; }
        public bool UseMousePanClamp { get; set; }

        public void FromImageBox(ImageBox pbx) {
            this.UseDrawPixelValue = pbx.UseDrawPixelValue;
            this.DrawPixelValueMode = pbx.DrawPixelValueMode;
            this.UseDrawInfo = pbx.UseDrawInfo;
            this.UseDrawCenterLine = pbx.UseDrawCenterLine;
            this.UseDrawDrawTime = pbx.UseDrawDrawTime;

            this.BackColor = pbx.BackColor;
            this.Font = pbx.Font;
            this.PixelValueDispFont = pbx.PixelValueDispFont;
            this.PixelValueDispZoomFactorGray8 = pbx.PixelValueDispZoomFactorGray8;
            this.PixelValueDispZoomFactorGray16 = pbx.PixelValueDispZoomFactorGray16;
            this.PixelValueDispZoomFactorRgb = pbx.PixelValueDispZoomFactorRgb;
            this.PixelValueDispZoomFactorFloat = pbx.PixelValueDispZoomFactorFloat;

            this.UseMouseMove = pbx.UseMouseMove;
            this.UseMouseWheelZoom = pbx.UseMouseWheelZoom;

            this.ZoomLevel = pbx.ZoomLevel;
            this.ZoomLevelMin = pbx.ZoomLevelMin;
            this.ZoomLevelMax = pbx.ZoomLevelMax;

            this.PanX = pbx.PanX;
            this.PanY = pbx.PanY;
            this.UseMousePanClamp = pbx.UseMousePanClamp;
        }

        public void ToImageBox(ImageBox pbx) {
            pbx.UseDrawPixelValue = this.UseDrawPixelValue;
            pbx.DrawPixelValueMode = this.DrawPixelValueMode;
            pbx.UseDrawInfo = this.UseDrawInfo;
            pbx.UseDrawCenterLine = this.UseDrawCenterLine;
            pbx.UseDrawDrawTime = this.UseDrawDrawTime;

            pbx.BackColor = this.BackColor;
            pbx.Font = this.Font;
            pbx.PixelValueDispFont = this.PixelValueDispFont;
            pbx.PixelValueDispZoomFactorGray8 = this.PixelValueDispZoomFactorGray8;
            pbx.PixelValueDispZoomFactorGray16 = this.PixelValueDispZoomFactorGray16;
            pbx.PixelValueDispZoomFactorRgb = this.PixelValueDispZoomFactorRgb;
            pbx.PixelValueDispZoomFactorFloat = this.PixelValueDispZoomFactorFloat;

            pbx.UseMouseMove = this.UseMouseMove;
            pbx.UseMouseWheelZoom = this.UseMouseWheelZoom;

            pbx.ZoomLevel = this.ZoomLevel;
            pbx.ZoomLevelMin = this.ZoomLevelMin;
            pbx.ZoomLevelMax = this.ZoomLevelMax;

            pbx.PanX = this.PanX;
            pbx.PanY = this.PanY;
            pbx.UseMousePanClamp = this.UseMousePanClamp;
        }
    }
}
