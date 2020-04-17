namespace ShimLibTest {
    partial class ImageBoxTest {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent() {
            this.pbxDraw = new ShimLib.ImageBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnAboutImageBox = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rlbxRetainedDraw = new ShimLib.RadioListBox();
            this.chkRetainedDraw = new System.Windows.Forms.CheckBox();
            this.btnImmediateDraw = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnZoomToImage = new System.Windows.Forms.Button();
            this.btnZoomReset = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnWideImage = new System.Windows.Forms.Button();
            this.bntLongImage = new System.Windows.Forms.Button();
            this.btnGradient = new System.Windows.Forms.Button();
            this.btnChess = new System.Windows.Forms.Button();
            this.btnCoinsDouble = new System.Windows.Forms.Button();
            this.btnCoinsFloat = new System.Windows.Forms.Button();
            this.btnCoins = new System.Windows.Forms.Button();
            this.btnLenna4 = new System.Windows.Forms.Button();
            this.btnLenna = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.dlgSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.panel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbxDraw
            // 
            this.pbxDraw.AllowDrop = true;
            this.pbxDraw.BackColor = System.Drawing.Color.Gray;
            this.pbxDraw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxDraw.Location = new System.Drawing.Point(0, 0);
            this.pbxDraw.Name = "pbxDraw";
            this.pbxDraw.PanX = 0D;
            this.pbxDraw.PanY = 0D;
            this.pbxDraw.Size = new System.Drawing.Size(712, 609);
            this.pbxDraw.TabIndex = 3;
            this.pbxDraw.Text = "imageBox1";
            this.pbxDraw.UseDrawCenterLine = true;
            this.pbxDraw.UseDrawDrawTime = true;
            this.pbxDraw.UseDrawInfo = true;
            this.pbxDraw.UseDrawPixelValue = true;
            this.pbxDraw.UseInterPorlation = false;
            this.pbxDraw.UseMouseMove = true;
            this.pbxDraw.UseMousePanClamp = true;
            this.pbxDraw.UseMouseWheelZoom = true;
            this.pbxDraw.UseParallel = false;
            this.pbxDraw.ZoomLevel = 0;
            this.pbxDraw.DragDrop += new System.Windows.Forms.DragEventHandler(this.pbxDraw_DragDrop);
            this.pbxDraw.DragEnter += new System.Windows.Forms.DragEventHandler(this.pbxDraw_DragEnter);
            this.pbxDraw.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxDraw_Paint);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(712, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(257, 609);
            this.panel1.TabIndex = 4;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnAboutImageBox);
            this.groupBox4.Location = new System.Drawing.Point(130, 255);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(118, 52);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Help";
            // 
            // btnAboutImageBox
            // 
            this.btnAboutImageBox.Location = new System.Drawing.Point(6, 20);
            this.btnAboutImageBox.Name = "btnAboutImageBox";
            this.btnAboutImageBox.Size = new System.Drawing.Size(106, 23);
            this.btnAboutImageBox.TabIndex = 0;
            this.btnAboutImageBox.Text = "About ImageBox";
            this.btnAboutImageBox.UseVisualStyleBackColor = true;
            this.btnAboutImageBox.Click += new System.EventHandler(this.btnAboutImageBox_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rlbxRetainedDraw);
            this.groupBox3.Controls.Add(this.chkRetainedDraw);
            this.groupBox3.Controls.Add(this.btnImmediateDraw);
            this.groupBox3.Location = new System.Drawing.Point(130, 90);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(118, 159);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Test";
            // 
            // rlbxRetainedDraw
            // 
            this.rlbxRetainedDraw.BackColor = System.Drawing.SystemColors.Window;
            this.rlbxRetainedDraw.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.rlbxRetainedDraw.FormattingEnabled = true;
            this.rlbxRetainedDraw.ItemHeight = 14;
            this.rlbxRetainedDraw.Items.AddRange(new object[] {
            "Draw Ellipse",
            "Fill Ellipse",
            "String",
            "Shapes",
            "Pixel Circles"});
            this.rlbxRetainedDraw.Location = new System.Drawing.Point(6, 78);
            this.rlbxRetainedDraw.Name = "rlbxRetainedDraw";
            this.rlbxRetainedDraw.Size = new System.Drawing.Size(106, 74);
            this.rlbxRetainedDraw.TabIndex = 2;
            this.rlbxRetainedDraw.SelectedIndexChanged += new System.EventHandler(this.rlbxRetainedDraw_SelectedIndexChanged);
            // 
            // chkRetainedDraw
            // 
            this.chkRetainedDraw.AutoSize = true;
            this.chkRetainedDraw.Location = new System.Drawing.Point(6, 53);
            this.chkRetainedDraw.Name = "chkRetainedDraw";
            this.chkRetainedDraw.Size = new System.Drawing.Size(106, 16);
            this.chkRetainedDraw.TabIndex = 1;
            this.chkRetainedDraw.Text = "Retained Draw";
            this.chkRetainedDraw.UseVisualStyleBackColor = true;
            this.chkRetainedDraw.CheckedChanged += new System.EventHandler(this.chkRetainedDraw_CheckedChanged);
            // 
            // btnImmediateDraw
            // 
            this.btnImmediateDraw.Location = new System.Drawing.Point(6, 20);
            this.btnImmediateDraw.Name = "btnImmediateDraw";
            this.btnImmediateDraw.Size = new System.Drawing.Size(106, 23);
            this.btnImmediateDraw.TabIndex = 0;
            this.btnImmediateDraw.Text = "Immediate Draw";
            this.btnImmediateDraw.UseVisualStyleBackColor = true;
            this.btnImmediateDraw.Click += new System.EventHandler(this.btnImmediateDraw_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnZoomToImage);
            this.groupBox2.Controls.Add(this.btnZoomReset);
            this.groupBox2.Location = new System.Drawing.Point(130, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(118, 81);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "File";
            // 
            // btnZoomToImage
            // 
            this.btnZoomToImage.Location = new System.Drawing.Point(6, 49);
            this.btnZoomToImage.Name = "btnZoomToImage";
            this.btnZoomToImage.Size = new System.Drawing.Size(106, 23);
            this.btnZoomToImage.TabIndex = 1;
            this.btnZoomToImage.Text = "Zoom to Image";
            this.btnZoomToImage.UseVisualStyleBackColor = true;
            this.btnZoomToImage.Click += new System.EventHandler(this.btnZoomToImage_Click);
            // 
            // btnZoomReset
            // 
            this.btnZoomReset.Location = new System.Drawing.Point(6, 20);
            this.btnZoomReset.Name = "btnZoomReset";
            this.btnZoomReset.Size = new System.Drawing.Size(106, 23);
            this.btnZoomReset.TabIndex = 0;
            this.btnZoomReset.Text = "Zoom Reset";
            this.btnZoomReset.UseVisualStyleBackColor = true;
            this.btnZoomReset.Click += new System.EventHandler(this.btnZoomReset_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnWideImage);
            this.groupBox1.Controls.Add(this.bntLongImage);
            this.groupBox1.Controls.Add(this.btnGradient);
            this.groupBox1.Controls.Add(this.btnChess);
            this.groupBox1.Controls.Add(this.btnCoinsDouble);
            this.groupBox1.Controls.Add(this.btnCoinsFloat);
            this.groupBox1.Controls.Add(this.btnCoins);
            this.groupBox1.Controls.Add(this.btnLenna4);
            this.groupBox1.Controls.Add(this.btnLenna);
            this.groupBox1.Controls.Add(this.btnPaste);
            this.groupBox1.Controls.Add(this.btnCopy);
            this.groupBox1.Controls.Add(this.btnSaveFile);
            this.groupBox1.Controls.Add(this.btnOpenFile);
            this.groupBox1.Location = new System.Drawing.Point(6, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(118, 400);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File";
            // 
            // btnWideImage
            // 
            this.btnWideImage.Location = new System.Drawing.Point(6, 368);
            this.btnWideImage.Name = "btnWideImage";
            this.btnWideImage.Size = new System.Drawing.Size(106, 23);
            this.btnWideImage.TabIndex = 12;
            this.btnWideImage.Text = "Wide Image";
            this.btnWideImage.UseVisualStyleBackColor = true;
            this.btnWideImage.Click += new System.EventHandler(this.btnWideImage_Click);
            // 
            // bntLongImage
            // 
            this.bntLongImage.Location = new System.Drawing.Point(6, 339);
            this.bntLongImage.Name = "bntLongImage";
            this.bntLongImage.Size = new System.Drawing.Size(106, 23);
            this.bntLongImage.TabIndex = 11;
            this.bntLongImage.Text = "Long Image";
            this.bntLongImage.UseVisualStyleBackColor = true;
            this.bntLongImage.Click += new System.EventHandler(this.bntLongImage_Click);
            // 
            // btnGradient
            // 
            this.btnGradient.Location = new System.Drawing.Point(6, 310);
            this.btnGradient.Name = "btnGradient";
            this.btnGradient.Size = new System.Drawing.Size(106, 23);
            this.btnGradient.TabIndex = 10;
            this.btnGradient.Text = "Gradient";
            this.btnGradient.UseVisualStyleBackColor = true;
            this.btnGradient.Click += new System.EventHandler(this.btnGradient_Click);
            // 
            // btnChess
            // 
            this.btnChess.Location = new System.Drawing.Point(6, 281);
            this.btnChess.Name = "btnChess";
            this.btnChess.Size = new System.Drawing.Size(106, 23);
            this.btnChess.TabIndex = 9;
            this.btnChess.Text = "Chess";
            this.btnChess.UseVisualStyleBackColor = true;
            this.btnChess.Click += new System.EventHandler(this.btnChess_Click);
            // 
            // btnCoinsDouble
            // 
            this.btnCoinsDouble.Location = new System.Drawing.Point(6, 252);
            this.btnCoinsDouble.Name = "btnCoinsDouble";
            this.btnCoinsDouble.Size = new System.Drawing.Size(106, 23);
            this.btnCoinsDouble.TabIndex = 8;
            this.btnCoinsDouble.Text = "Coins Double";
            this.btnCoinsDouble.UseVisualStyleBackColor = true;
            this.btnCoinsDouble.Click += new System.EventHandler(this.btnCoinsDouble_Click);
            // 
            // btnCoinsFloat
            // 
            this.btnCoinsFloat.Location = new System.Drawing.Point(6, 223);
            this.btnCoinsFloat.Name = "btnCoinsFloat";
            this.btnCoinsFloat.Size = new System.Drawing.Size(106, 23);
            this.btnCoinsFloat.TabIndex = 7;
            this.btnCoinsFloat.Text = "Coins Float";
            this.btnCoinsFloat.UseVisualStyleBackColor = true;
            this.btnCoinsFloat.Click += new System.EventHandler(this.btnCoinsFloat_Click);
            // 
            // btnCoins
            // 
            this.btnCoins.Location = new System.Drawing.Point(6, 194);
            this.btnCoins.Name = "btnCoins";
            this.btnCoins.Size = new System.Drawing.Size(106, 23);
            this.btnCoins.TabIndex = 6;
            this.btnCoins.Text = "Coins";
            this.btnCoins.UseVisualStyleBackColor = true;
            this.btnCoins.Click += new System.EventHandler(this.btnCoins_Click);
            // 
            // btnLenna4
            // 
            this.btnLenna4.Location = new System.Drawing.Point(6, 165);
            this.btnLenna4.Name = "btnLenna4";
            this.btnLenna4.Size = new System.Drawing.Size(106, 23);
            this.btnLenna4.TabIndex = 5;
            this.btnLenna4.Text = "Lenna4";
            this.btnLenna4.UseVisualStyleBackColor = true;
            this.btnLenna4.Click += new System.EventHandler(this.btnLenna4_Click);
            // 
            // btnLenna
            // 
            this.btnLenna.Location = new System.Drawing.Point(6, 136);
            this.btnLenna.Name = "btnLenna";
            this.btnLenna.Size = new System.Drawing.Size(106, 23);
            this.btnLenna.TabIndex = 4;
            this.btnLenna.Text = "Lenna";
            this.btnLenna.UseVisualStyleBackColor = true;
            this.btnLenna.Click += new System.EventHandler(this.btnLenna_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Location = new System.Drawing.Point(6, 107);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(106, 23);
            this.btnPaste.TabIndex = 3;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(6, 78);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(106, 23);
            this.btnCopy.TabIndex = 2;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(6, 49);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(106, 23);
            this.btnSaveFile.TabIndex = 1;
            this.btnSaveFile.Text = "Save File";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(6, 20);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(106, 23);
            this.btnOpenFile.TabIndex = 0;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // dlgSaveFile
            // 
            this.dlgSaveFile.Filter = "Bmp File(*.bmp)|*.bmp";
            // 
            // ImageBoxTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbxDraw);
            this.Controls.Add(this.panel1);
            this.Name = "ImageBoxTest";
            this.Size = new System.Drawing.Size(969, 609);
            this.panel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ShimLib.ImageBox pbxDraw;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnAboutImageBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private ShimLib.RadioListBox rlbxRetainedDraw;
        private System.Windows.Forms.CheckBox chkRetainedDraw;
        private System.Windows.Forms.Button btnImmediateDraw;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnZoomToImage;
        private System.Windows.Forms.Button btnZoomReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnWideImage;
        private System.Windows.Forms.Button bntLongImage;
        private System.Windows.Forms.Button btnGradient;
        private System.Windows.Forms.Button btnChess;
        private System.Windows.Forms.Button btnCoinsDouble;
        private System.Windows.Forms.Button btnCoinsFloat;
        private System.Windows.Forms.Button btnCoins;
        private System.Windows.Forms.Button btnLenna4;
        private System.Windows.Forms.Button btnLenna;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.SaveFileDialog dlgSaveFile;
    }
}
