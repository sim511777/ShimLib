﻿using ShimLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PointD = System.Windows.Point;

namespace ImageBoxTest {
    public partial class FormMain : Form {
        static string[] extList = { ".bmp", ".jpg", ".png", ".hra", ".tif" };
        private IntPtr imgBuf;
        private int bw;
        private int bh;
        private int bytepp;

        public FormMain() {
            InitializeComponent();
            var exts = string.Join(";", extList.Select(ext => "*"+ext));
            dlgOpenFile.Filter = $"Image Files({exts})|{exts}";
        }

        private void LoadImageFile(string fileName) {
            if (imgBuf != IntPtr.Zero)
                Marshal.FreeHGlobal(imgBuf);
            
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".hra") {
                Util.LoadHraFile(fileName, ref imgBuf, ref bw, ref bh, ref bytepp);
            } else {
                var bmp = new Bitmap(fileName);
                Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);
                bmp.Dispose();
            }

            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }

        private void LoadClipboard() {
            var img = Clipboard.GetImage();
            if (img == null)
                return;

            if (imgBuf != IntPtr.Zero)
                Marshal.FreeHGlobal(imgBuf);

            var bmp = new Bitmap(img);
            Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);

            bmp.Dispose();
            img.Dispose();

            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
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

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e) {
            var ok = dlgOpenFile.ShowDialog(this);
            if (ok != DialogResult.OK)
                return;

            string filePath = dlgOpenFile.FileName;
            LoadImageFile(filePath);
        }

        private void pasteFromClipboardToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadClipboard();
        }

        private void zoomResetToolStripMenuItem_Click_1(object sender, EventArgs e) {
            pbxDraw.ZoomReset();
            pbxDraw.Invalidate();
        }

        private void zoomToImageToolStripMenuItem_Click_1(object sender, EventArgs e) {
            if (pbxDraw.ImgBW <= 0 || pbxDraw.ImgBH <= 0)
                pbxDraw.ZoomReset();
            else
                pbxDraw.ZoomToRect(0, 0, pbxDraw.ImgBW, pbxDraw.ImgBH);
            pbxDraw.Invalidate();
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

        private void aboutImageBoxToolStripMenuItem_Click(object sender, EventArgs e) {
            pbxDraw.ShowAbout();
        }

        private void LoadBitmap(Bitmap bmp) {
            if (imgBuf != IntPtr.Zero)
                Marshal.FreeHGlobal(imgBuf);
            Util.BitmapToImageBuffer(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }

        private void lennaToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Lenna);
        }

        private void chessToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Chess);
        }

        private void lenna4ToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Lenna4);
        }

        private void coinsToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadBitmap(Properties.Resources.Coins);
        }

        private void longImageToolStripMenuItem_Click(object sender, EventArgs e) {
            GenerateBitmap(256, 4000000);
        }

        private void wideImageToolStripMenuItem_Click(object sender, EventArgs e) {
            GenerateBitmap(4000000, 256);
        }

        private unsafe void GenerateBitmap(int bw, int bh) {
            if (imgBuf != IntPtr.Zero)
                Marshal.FreeHGlobal(imgBuf);
            long cb = (long)bw * bh;
            imgBuf = Marshal.AllocHGlobal((IntPtr)cb);
            for (long y = 0; y < bh; y++) {
                byte* ptr = (byte*)imgBuf + y * bw;
                for (long x = 0; x < bw; x++) {
                    ptr[x] = (byte)((x + y) % 256);
                }
            }
            pbxDraw.SetImgBuf(imgBuf, bw, bh, 1, true);
        }

        private void pbxDraw_Paint(object sender, PaintEventArgs e) {
            //ImageGraphics ig = new ImageGraphics(pbxDraw, e.Graphics);
            //ig.DrawLine(1, 1, 2, 2, Pens.Lime);
            //ig.DrawLine(1, 2, 2, 1, Pens.Red);
            //ig.DrawRectangle(2, 2, 3, 3, Pens.Red, true, Brushes.Lime);
            //ig.DrawRectangle(2, 2, 3, 3, Pens.Red, false, Brushes.Red);
            //ig.DrawEllipse(3, 3, 4, 4, Pens.Red, true, Brushes.Lime);
            //ig.DrawEllipse(3, 3, 4, 4, Pens.Red, false, Brushes.Red);
            //ig.DrawCross(10, 10, 20, Pens.Lime, false);
            //ig.DrawPlus(10, 10, 20, Pens.Red, true);
            
            if (retainedimmediateDrawTestToolStripMenuItem.Checked) {
                ImageGraphics ig = new ImageGraphics(pbxDraw, e.Graphics);
                for (int y = 0; y < 100; y++) {
                    for (int x = 0; x < 100; x++) {
                        ig.DrawEllipse(x, y, x + 1, y + 1, Pens.Red);
                    }
                }
            }
        }

        private void immediateDrawTestToolStripMenuItem_Click(object sender, EventArgs e) {
            using (Graphics g = pbxDraw.CreateGraphics()) {
                var st = Util.GetTimeMs();
                ImageGraphics ig = new ImageGraphics(pbxDraw, g);
                for (int y = 0; y < 100; y++) {
                    for (int x = 0; x < 100; x++) {
                        ig.DrawEllipse(x, y, x + 1, y + 1, Pens.Lime);
                    }
                }
                var dt = Util.GetTimeMs() - st;
                ig.DrawStringScreen(dt.ToString(), 200, 0, Brushes.Black, true, Brushes.White);
            }
        }

        private void retainedimmediateDrawTestToolStripMenuItem_Click(object sender, EventArgs e) {
            pbxDraw.Invalidate();
        }
    }
}
