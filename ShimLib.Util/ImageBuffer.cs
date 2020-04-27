using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class ImageBuffer : IDisposable {
        public IntPtr buf;
        public int bw;
        public int bh;
        public int bytepp;
        private bool allocated;

        private bool disposed = false;

        public ImageBuffer(Bitmap bmp) {
            Util.BitmapToImageBuffer(bmp, ref buf, ref bw, ref bh, ref bytepp);
            allocated = true;
        }

        public ImageBuffer(string filePath) {
            using (var bmp = new Bitmap(filePath)) {
                Util.BitmapToImageBuffer(bmp, ref buf, ref bw, ref bh, ref bytepp);
                allocated = true;
            }
        }

        public ImageBuffer(IntPtr _buf, int _bw, int _bh, int _bytepp, bool _allocated) {
            buf = _buf;
            bw = _bw;
            bh = _bh;
            bytepp = _bytepp;
            allocated = _allocated;
        }

        ~ImageBuffer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;

            // managed 리소스 해제
            if (disposing) {
            }

            // unmanaged 리소스 해제
            if (allocated) {
                Util.FreeBuffer(ref buf);
            }

            disposed = true;
        }
    }
}
