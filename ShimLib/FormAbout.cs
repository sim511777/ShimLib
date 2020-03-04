﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void FormAbout_Load(object sender, EventArgs e) {
            tbxVersion.Text = ImageBox.VersionHistory;
            ImageBoxOption option = new ImageBoxOption();
            option.FromImageBox(pbx);
            grdOption.SelectedObject = option;
        }

        private void FormAbout_FormClosed(object sender, FormClosedEventArgs e) {
            if (this.DialogResult != DialogResult.OK)
                return;
            var option = grdOption.SelectedObject as ImageBoxOption;
            option.ToImageBox(pbx);
        }
    }

    public class ImageBoxOption {
        // 화면 표시 옵션
        public bool UseDrawPixelValue { get; set; }
        public bool UseDrawInfo { get; set; }
        public bool UseDrawCenterLine { get; set; }
        public bool UseDrawDrawTime { get; set; }
        public bool UseInterPorlation { get; set; }
        public bool UseParallel { get; set; }
        public int PixelValueDispZoomFactor { get; set; }
        public Font Font { get; set; }
        public Color BackColor { get; set; }
        // 마우스 동작 옵션
        public bool UseMouseMove { get; set; }
        public bool UseMouseWheelZoom { get; set; }
        // 줌 파라미터
        public int ZoomLevel { get; set; }
        // 패닝 파라미터
        public double PanX { get; set; }
        public double PanY { get; set; }

        public void FromImageBox(ImageBox pbx) {
            this.UseDrawPixelValue = pbx.UseDrawPixelValue;
            this.UseDrawInfo = pbx.UseDrawInfo;
            this.UseDrawCenterLine = pbx.UseDrawCenterLine;
            this.UseDrawDrawTime = pbx.UseDrawDrawTime;
            this.UseInterPorlation = pbx.UseInterPorlation;
            this.UseParallel = pbx.UseParallel;
            
            this.PixelValueDispZoomFactor = pbx.PixelValueDispZoomFactor;
            this.Font = pbx.Font;
            this.BackColor = pbx.BackColor;

            this.UseMouseMove = pbx.UseMouseMove;
            this.UseMouseWheelZoom = pbx.UseMouseWheelZoom;
            
            this.ZoomLevel = pbx.ZoomLevel;
            this.PanX = pbx.PanX;
            this.PanY = pbx.PanY;
        }

        public void ToImageBox(ImageBox pbx) {
            pbx.UseDrawPixelValue = this.UseDrawPixelValue;
            pbx.UseDrawInfo = this.UseDrawInfo;
            pbx.UseDrawCenterLine = this.UseDrawCenterLine;
            pbx.UseDrawDrawTime = this.UseDrawDrawTime;
            pbx.UseInterPorlation = this.UseInterPorlation;
            pbx.UseParallel = this.UseParallel;
            pbx.PixelValueDispZoomFactor = this.PixelValueDispZoomFactor;
            pbx.Font = this.Font;
            pbx.BackColor = this.BackColor;
            pbx.UseMouseMove = this.UseMouseMove;
            pbx.UseMouseWheelZoom = this.UseMouseWheelZoom;
            pbx.ZoomLevel = this.ZoomLevel;
            pbx.PanX = this.PanX;
            pbx.PanY = this.PanY;
        }
    }
}
