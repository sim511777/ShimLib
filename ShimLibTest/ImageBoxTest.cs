using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShimLib;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace ShimLibTest {
    public partial class ImageBoxTest : UserControl {
        static string[] extList = { ".bmp", ".jpg", ".png", ".hra", ".tif" };
        private IntPtr imgBuf;
        private int bw;
        private int bh;
        private int bytepp;

        public ImageBoxTest() {
            InitializeComponent();
        }

        public void LoadCommandLine(string[] args) {
            var exts = string.Join(";", extList.Select(ext => "*" + ext));
            dlgOpenFile.Filter = $"Image Files({exts})|{exts}";
            if (args.Length > 0) {
                LoadImageFile(args[0]);
            }
        }

        private void LoadImageFile(string fileName) {
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".hra") {
                if (imgBuf != IntPtr.Zero)
                    Util.FreeBuffer(ref imgBuf);
                Util.LoadHraFile(fileName, ref imgBuf, ref bw, ref bh, ref bytepp);
                pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
            } else {
                var bmp = new Bitmap(fileName);
                LoadBitmap(bmp);
                bmp.Dispose();
            }
        }

        private void SaveImageFile(string fileName) {
            var bmp = Util.ImageBufferToBitmap(imgBuf, bw, bh, bytepp);
            bmp.Save(fileName, ImageFormat.Bmp);
            bmp.Dispose();
        }

        private void PasteFromClipboard() {
            var img = Clipboard.GetImage();
            if (img == null)
                return;

            var bmp = new Bitmap(img);

            LoadBitmap(bmp);

            bmp.Dispose();
            img.Dispose();
        }

        private void CopyToClipboard() {
            if (imgBuf == null)
                return;
            
            var bmp = Util.ImageBufferToBitmap(imgBuf, bw, bh, bytepp);
            Clipboard.SetImage(bmp);
            bmp.Dispose();
        }

        private string GetDragDataImageFile(IDataObject data) {
            string[] files = (string[])data.GetData(DataFormats.FileDrop);
            if (files.Length != 1)
                return null;
            
            string file = files[0];
            FileAttributes attr = File.GetAttributes(file);
            if (attr.HasFlag(FileAttributes.Directory))
                return null;

            string ext = Path.GetExtension(file).ToLower();
            if (extList.Contains(ext) == false)
                return null;

            return file;
        }

        private void pbxDraw_DragEnter(object sender, DragEventArgs e) {
            string imageFile = GetDragDataImageFile(e.Data);
            if (imageFile == null) {
                e.Effect = DragDropEffects.None;
                return;
            }
            e.Effect = DragDropEffects.Copy;
        }

        private void pbxDraw_DragDrop(object sender, DragEventArgs e) {
            string filePath = GetDragDataImageFile(e.Data);
            if (filePath == null)
                return;

            LoadImageFile(filePath);
        }

        private void LoadBitmap(Bitmap bmp) {
            if (imgBuf != IntPtr.Zero)
                Util.FreeBuffer(ref imgBuf);
            Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }

        private unsafe void LoadBitmapFloat(Bitmap bmp) {
            if (imgBuf != IntPtr.Zero)
                Util.FreeBuffer(ref imgBuf);
            Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);

            // byte -> float convert
            IntPtr floatBuf = Util.AllocBuffer(bw * bh * sizeof(float));
            for (int y = 0; y < bh; y++) {
                byte* src = (byte*)imgBuf + bw * y;
                float* dst = (float*)floatBuf + bw * y;
                for (int x = 0; x < bw; x++, src++, dst++) {
                    *dst = *src;
                }
            }
            Util.FreeBuffer(ref imgBuf);
            imgBuf = floatBuf;
            bytepp = sizeof(float);

            // SetFloatBuf
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true, true);
        }

        private unsafe void LoadBitmapDouble(Bitmap bmp) {
            if (imgBuf != IntPtr.Zero)
                Util.FreeBuffer(ref imgBuf);
            Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);

            // byte -> double convert
            IntPtr dlbBuf = Util.AllocBuffer(bw * bh * sizeof(double));
            for (int y = 0; y < bh; y++) {
                byte* src = (byte*)imgBuf + bw * y;
                double* dst = (double*)dlbBuf + bw * y;
                for (int x = 0; x < bw; x++, src++, dst++) {
                    *dst = *src;
                }
            }
            Util.FreeBuffer(ref imgBuf);
            imgBuf = dlbBuf;
            bytepp = sizeof(double);

            // SetFloatBuf
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true, true);
        }

        private unsafe void GenerateBitmap(int bw, int bh) {
            if (imgBuf != IntPtr.Zero)
                Util.FreeBuffer(ref imgBuf);
            long cb = (long)bw * bh;
            imgBuf = Util.AllocBuffer(cb);
            for (long y = 0; y < bh; y++) {
                byte* ptr = (byte*)imgBuf + y * bw;
                for (long x = 0; x < bw; x++) {
                    ptr[x] = (byte)((x + y) % 256);
                }
            }
            pbxDraw.SetImgBuf(imgBuf, bw, bh, 1, true);
        }

        private void pbxDraw_Paint(object sender, PaintEventArgs e) {
            if (chkRetainedDraw.Checked)
                UserDrawTest(e.Graphics);
        }

        private void UserDrawTest(Graphics g) {
            var st = Stopwatch.GetTimestamp();

            Random Rnd = new Random();
            int step = 20;
            if (rlbxRetainedDraw.SelectedIndex == 0) {
                Pen pen = new Pen(Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256)));
                for (int y = 0; y < 1000; y += step) {
                    for (int x = 0; x < 1000; x += step) {
                        g.DrawEllipse(pen, x, y, step, step);
                    }
                }
                pen.Dispose();
            } else if (rlbxRetainedDraw.SelectedIndex == 1) {
                Brush br = new SolidBrush(Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256)));
                for (int y = 0; y < 1000; y += step) {
                    for (int x = 0; x < 1000; x += step) {
                        g.FillEllipse(br, x, y, step, step);
                    }
                }
                br.Dispose();
            } else if (rlbxRetainedDraw.SelectedIndex == 2) {
                Brush br = new SolidBrush(Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256)));
                for (int y = 0; y < 1000; y += step) {
                    for (int x = 0; x < 1000; x += step) {
                        g.DrawString("128", this.Font, br, x, y);
                    }
                }
                br.Dispose();
            } else if (rlbxRetainedDraw.SelectedIndex == 3) {
                ImageGraphics ig = new ImageGraphics(pbxDraw, g);
                ig.DrawLine(1, 1, 2, 2, Pens.Lime);
                ig.DrawLine(1, 2, 2, 1, Pens.Red);
                ig.DrawRectangle(2, 2, 3, 3, Pens.Red, true, Brushes.Lime);
                ig.DrawRectangle(2, 2, 3, 3, Pens.Red, false, Brushes.Red);
                ig.DrawEllipse(3, 3, 4, 4, Pens.Red, true, Brushes.Lime);
                ig.DrawEllipse(3, 3, 4, 4, Pens.Red, false, Brushes.Red);
                ig.DrawCross(10, 10, 20, Pens.Lime, false);
                ig.DrawPlus(10, 10, 20, Pens.Red, true);
            } else if (rlbxRetainedDraw.SelectedIndex == 4) {
                ImageGraphics ig = new ImageGraphics(pbxDraw, g);
                Pen pen = new Pen(Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256)));
                for (int y = 0; y < 100; y++) {
                    for (int x = 0; x < 100; x++) {
                        ig.DrawEllipse(x, y, x + 1, y + 1, pen);
                    }
                }
                pen.Dispose();
            }

            var et = Stopwatch.GetTimestamp();
            var ms = (et - st) * 1000.0 / Stopwatch.Frequency;
            var text = $"DrawTime : {ms:0.00}";
            var size = g.MeasureString(text, this.Font);
            g.FillRectangle(Brushes.White, 255, 2, size.Width, size.Height);
            g.DrawString(text, pbxDraw.Font, Brushes.Black, 255, 2);
        }

        private void btnOpenFile_Click(object sender, EventArgs e) {
            var ok = dlgOpenFile.ShowDialog(this);
            if (ok != DialogResult.OK)
                return;

            string filePath = dlgOpenFile.FileName;
            LoadImageFile(filePath);
        }

        private void btnSaveFile_Click(object sender, EventArgs e) {
            if (imgBuf == IntPtr.Zero) {
                MessageBox.Show(this, "imgBuf == IntPtr.Zero");
                return;
            }

            var ok = dlgSaveFile.ShowDialog(this);
            if (ok != DialogResult.OK)
                return;

            string filePath = dlgSaveFile.FileName;
            SaveImageFile(filePath);
        }

        private void btnCopy_Click(object sender, EventArgs e) {
            CopyToClipboard();
        }

        private void btnPaste_Click(object sender, EventArgs e) {
            PasteFromClipboard();
        }

        private void btnLenna_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Lenna);
        }

        private void btnLenna4_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Lenna4);
        }

        private void btnCoins_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Coins);
        }

        private void btnCoinsFloat_Click(object sender, EventArgs e) {
            LoadBitmapFloat(Properties.Resources.Coins);
        }

        private void btnCoinsDouble_Click(object sender, EventArgs e) {
            LoadBitmapDouble(Properties.Resources.Coins);
        }

        private void btnChess_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Chess);
        }

        private void btnGradient_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.gardient_8bit);
        }

        private void bntLongImage_Click(object sender, EventArgs e) {
            GenerateBitmap(256, 400000);
        }

        private void btnWideImage_Click(object sender, EventArgs e) {
            GenerateBitmap(400000, 256);
        }

        private void btnZoomReset_Click(object sender, EventArgs e) {
            pbxDraw.ZoomReset();
            pbxDraw.Invalidate();
        }

        private void btnZoomToImage_Click(object sender, EventArgs e) {
            if (pbxDraw.ImgBW <= 0 || pbxDraw.ImgBH <= 0)
                pbxDraw.ZoomReset();
            else
                pbxDraw.ZoomToRect(0, 0, pbxDraw.ImgBW, pbxDraw.ImgBH);
            pbxDraw.Invalidate();
        }

        private void btnImmediateDraw_Click(object sender, EventArgs e) {
            using (Graphics g = pbxDraw.CreateGraphics()) {
                UserDrawTest(g);
            }
        }

        private void chkRetainedDraw_CheckedChanged(object sender, EventArgs e) {
            pbxDraw.Invalidate();
        }

        private void rlbxRetainedDraw_SelectedIndexChanged(object sender, EventArgs e) {
            pbxDraw.Invalidate();
        }

        private void btnAboutImageBox_Click(object sender, EventArgs e) {
            pbxDraw.ShowAbout();
        }

        private void pbxDraw_PaintBackBuffer(object sender, IntPtr buf, int bw, int bh) {
            var iCol = Color.Red.ToArgb();
            
            int numLine = (int)numLineNum.Value;
            int lineType = rlbxLineType.SelectedIndex;

            Random rnd = new Random(0);
            if (lineType == 0) {
                for (int i = 0; i < numLine; i++) {
                    Drawing.DrawLineEquation(buf, bw, bh, rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), iCol);
                }
            } else if (lineType == 1) {
                for (int i = 0; i < numLine; i++) {
                    Drawing.DrawLineDda(buf, bw, bh, rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), iCol);
                }
            } else if (lineType == 2) {
                for (int i = 0; i < numLine; i++) {
                    Drawing.DrawLineBresenham(buf, bw, bh, rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), rnd.Next(0, 499), iCol);
                }
            } else if (lineType == 3) {
                for (int i = 0; i < numLine; i++) {
                    Drawing.DrawCircle(buf, bw, bh, rnd.Next(125, 375), rnd.Next(125, 375), rnd.Next(0, 125), iCol);
                }
            }
        }

        private void rlbxLineType_SelectedIndexChanged(object sender, EventArgs e) {
            pbxDraw.Invalidate();
        }

        private void numLineNum_ValueChanged(object sender, EventArgs e) {
            pbxDraw.Invalidate();
        }
    }
}
