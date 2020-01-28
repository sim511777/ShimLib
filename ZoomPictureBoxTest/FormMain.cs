using ShimLib;
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

namespace ZoomPictureBoxTest {
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

        private void showPixelValueToolStripMenuItem_CheckedChanged(object sender, EventArgs e) {
            pbxDraw.UseDrawPixelValue = showPixelValueToolStripMenuItem.Checked;
            pbxDraw.UseDrawInfo = showCursorInfoToolStripMenuItem.Checked;
            pbxDraw.UseDrawCenterLine = showDrawCenterLineToolStripMenuItem.Checked;
            pbxDraw.UseDrawDrawTime = showDrawTimeToolStripMenuItem.Checked;
            pbxDraw.UseMouseMove = mousePanningToolStripMenuItem.Checked;
            pbxDraw.UseMouseWheelZoom = wheelZoomToolStripMenuItem.Checked;
            pbxDraw.UseInterPorlation = useInterpolationToolStripMenuItem.Checked;
            pbxDraw.UseParallel = userParallelToolStripMenuItem.Checked;
            pbxDraw.UseNative = useNativeToolStripMenuItem.Checked;
            pbxDraw.Invalidate();
        }

        private void aboutZoomPictureBoxToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show(this, ZoomPictureBox.VersionHistory, "About ZoomPictureBox");
        }

        private void lennaToolStripMenuItem_Click(object sender, EventArgs e) {
            Util.BitmapToImageBuffer(Properties.Resources.Lenna, ref imgBuf, ref bw, ref bh, ref bytepp);
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }

        private void chessToolStripMenuItem_Click(object sender, EventArgs e) {
            Util.BitmapToImageBuffer(Properties.Resources.Chess, ref imgBuf, ref bw, ref bh, ref bytepp);
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }

        private void lenna4ToolStripMenuItem_Click(object sender, EventArgs e) {
            Util.BitmapToImageBuffer(Properties.Resources.Lenna4, ref imgBuf, ref bw, ref bh, ref bytepp);
            pbxDraw.SetImgBuf(imgBuf, bw, bh, bytepp, true);
        }
    }
}
