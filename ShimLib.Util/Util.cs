using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct BITMAPFILEHEADER {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    public class Util {
        public const string VersionHistory =
@"ShimLib Util for .NET

v20200330
1. Version정보 추가";

        // 시간 측정 함수
        public static double GetTime() {
            return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        }

        public static double GetTimeMs() {
            return GetTime() * 1000.0;
        }

        public static void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a = b;
            b = temp;
        }

        // 범위 제한 함수
        public static T Clamp<T>(T value, T min, T max) where T : IComparable {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        public static unsafe IntPtr Memcpy(IntPtr _Dst, IntPtr _Src, Int64 _Size) {
            Int64 size4 = _Size / 4;
            Int64 size1 = _Size % 4;

            int* pdst4 = (int*)_Dst;
            int* psrc4 = (int*)_Src;
            while (size4-- > 0)
                *pdst4++ = *psrc4++;

            byte* pdst1 = (byte*)pdst4;
            byte* psrc1 = (byte*)psrc4;
            while (size1-- > 0)
                *pdst1++ = *psrc1++;

            return _Dst;
        }

        public static unsafe IntPtr Memset(IntPtr _Dst, int _Val, Int64 _Size) {
            Int64 size4 = _Size / 4;
            Int64 size1 = _Size % 4;

            int val4 = _Val | _Val << 8 | _Val << 16 | _Val << 24;
            byte val1 = (byte)_Val;

            int* pdst4 = (int*)_Dst;
            while (size4-- > 0)
                *pdst4++ = val4;

            byte* pdst1 = (byte*)pdst4;
            while (size1-- > 0)
                *pdst1++ = val1;

            return _Dst;
        }

        // free and set null
        public static void FreeBuffer(ref IntPtr buf) {
            if (buf != IntPtr.Zero) {
                Marshal.FreeHGlobal(buf);
                buf = IntPtr.Zero;
            }
        }

        public static IntPtr AllocBuffer(Int64 size) {
            IntPtr buf = Marshal.AllocHGlobal((IntPtr)size);
            Util.Memset(buf, 0, size);
            return buf;
        }

        // float(32bit) or double(64bit) -> gray(8bit);
        public unsafe static void FloatBufToByte(IntPtr floatBuf, int bw, int bh, int bytepp, IntPtr byteBuf) {
            for (int y = 0; y < bh; y++) {
                byte* psrc = (byte*)floatBuf + (bw * y) * bytepp;
                byte* pdst = (byte*)byteBuf + bw * y;
                for (int x = 0; x < bw; x++, pdst++, psrc += bytepp) {
                    if (bytepp == 4)
                        *pdst = (byte)Util.Clamp(*(float*)psrc, 0, 255);
                    else if (bytepp == 8)
                        *pdst = (byte)Util.Clamp(*(double*)psrc, 0, 255);
                }
            }
        }

        // 8bit bmp 파일 버퍼에 로드
        public unsafe static T StreamReadStructure<T>(Stream sr) {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];
            sr.Read(buf, 0, size);
            fixed (byte* ptr = buf) {
                T obj = Marshal.PtrToStructure<T>((IntPtr)ptr);
                return obj;
            }
        }
        public static bool Load8BitBmp(IntPtr buf, int bw, int bh, string filePath) {
            // 파일 오픈
            FileStream hFile;
            try {
                hFile = File.OpenRead(filePath);
            } catch {
                return false;
            }

            // 파일 헤더
            BITMAPFILEHEADER fh = StreamReadStructure<BITMAPFILEHEADER>(hFile);

            // 정보 헤더
            BITMAPINFOHEADER ih = StreamReadStructure<BITMAPINFOHEADER>(hFile);
            if (ih.biBitCount != 8) {   // 컬러비트 체크
                hFile.Dispose();
                return false;
            }

            hFile.Seek(fh.bfOffBits, SeekOrigin.Begin);

            int fbw = ih.biWidth;
            int fbh = ih.biHeight;

            // bmp파일은 파일 저장시 라인당 4byte padding을 한다.
            // bw가 4로 나눠 떨어지지 않을경우 padding처리 해야 함
            // int stride = (bw+3)/4*4;buf + y * bw
            int fstep = (fbw + 3) / 4 * 4;

            byte[] fbuf = new byte[fbh * fstep];
            hFile.Read(fbuf, 0, fbh * fstep);

            // 대상버퍼 width/height 소스버퍼 width/height 중 작은거 만큼 카피
            int minh = Math.Min(bh, fbh);
            int minw = Math.Min(bw, fbw);

            // bmp파일은 위아래가 뒤집혀 있으므로 파일에서 아래 라인부터 읽어서 버퍼에 쓴다
            for (int y = 0; y < minh; y++) {
                Marshal.Copy(fbuf, (fbh - y - 1) * fstep, buf + y * bw, minw);
            }

            hFile.Dispose();
            return true;
        }

        // 8bit 버퍼 bmp 파일에 저장
        static readonly byte[] grayPalette = Enumerable.Range(0, 1024).Select(i => i % 4 == 3 ? (byte)0xff : (byte)(i / 4)).ToArray();
        public unsafe static void StreamWriteStructure<T>(Stream sr, T obj) {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];
            fixed (byte* ptr = buf) {
                Marshal.StructureToPtr<T>(obj, (IntPtr)ptr, false);
            }
            sr.Write(buf, 0, size);
        }
        public static bool Save8BitBmp(IntPtr buf, int bw, int bh, string filePath) {
            // 파일 오픈
            FileStream hFile;
            try {
                hFile = File.OpenWrite(filePath);
            } catch {
                return false;
            }

            int fstep = (bw + 3) / 4 * 4;

            // 파일 헤더
            BITMAPFILEHEADER fh;
            fh.bfType = 0x4d42;  // Magic NUMBER "BM"
            fh.bfOffBits = (uint)(Marshal.SizeOf(typeof(BITMAPFILEHEADER)) + Marshal.SizeOf(typeof(BITMAPINFOHEADER)) + grayPalette.Length);   // offset to bitmap buffer from start
            fh.bfSize = fh.bfOffBits + (uint)(fstep * bh);  // file size
            fh.bfReserved1 = 0;
            fh.bfReserved2 = 0;
            StreamWriteStructure(hFile, fh);

            // 정보 헤더
            BITMAPINFOHEADER ih;
            ih.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));   // struct of BITMAPINFOHEADER
            ih.biWidth = bw; // widht
            ih.biHeight = bh; // height
            ih.biPlanes = 1;
            ih.biBitCount = 8;  // 8bit
            ih.biCompression = 0;
            ih.biSizeImage = 0;
            ih.biXPelsPerMeter = 3780;  // pixels-per-meter
            ih.biYPelsPerMeter = 3780;  // pixels-per-meter
            ih.biClrUsed = 256;   // grayPalette count
            ih.biClrImportant = 256;   // grayPalette count
            StreamWriteStructure(hFile, ih);

            // RGB Palette
            hFile.Write(grayPalette, 0, grayPalette.Length);

            // bmp파일은 파일 저장시 라인당 4byte padding을 한다.
            // bw가 4로 나눠 떨어지지 않을경우 padding처리 해야 함
            int paddingSize = fstep - bw;
            byte[] paddingBuf = { 0, 0, 0, 0 };

            byte[] fbuf = new byte[bh * fstep];
            // bmp파일은 위아래가 뒤집혀 있으므로 버퍼 아래라인 부터 읽어서 파일에 쓴다
            for (int y = bh - 1; y >= 0; y--) {
                Marshal.Copy(buf + y * bw, fbuf, (bh - y - 1) * fstep, bw);
                if (paddingSize > 0)
                    Array.Copy(paddingBuf, 0, fbuf, (bh - y - 1) * fstep + bw, paddingSize);
            }
            hFile.Write(fbuf, 0, bh * fstep);

            hFile.Dispose();
            return true;
        }

        // Load Bitmap to buffer
        public unsafe static void BitmapToImageBuffer(Bitmap bmp, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            if (bmp.PixelFormat == PixelFormat.Format1bppIndexed) {
                BitmapToImageBuffer1Bit(bmp, ref imgBuf, ref bw, ref bh, ref bytepp);
                return;
            }

            bw = bmp.Width;
            bh = bmp.Height;
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                bytepp = 1;
            else if (bmp.PixelFormat == PixelFormat.Format16bppGrayScale)
                bytepp = 2;
            else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                bytepp = 3;
            else if (bmp.PixelFormat == PixelFormat.Format32bppRgb || bmp.PixelFormat == PixelFormat.Format32bppArgb || bmp.PixelFormat == PixelFormat.Format32bppPArgb)
                bytepp = 4;
            long bufSize = (long)bw * bh * bytepp;
            imgBuf = Util.AllocBuffer(bufSize);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int copySize = bw * bytepp;
            for (int y = 0; y < bh; y++) {
                IntPtr dstPtr = imgBuf + bw * y * bytepp;
                IntPtr srcPtr = bmpData.Scan0 + bmpData.Stride * y;
                Util.Memcpy(dstPtr, srcPtr, copySize);
            }

            bmp.UnlockBits(bmpData);
        }

        private unsafe static void BitmapToImageBuffer1Bit(Bitmap bmp, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            bw = bmp.Width;
            bh = bmp.Height;
            bytepp = 1;
            long bufSize = (long)bw * bh * bytepp;
            imgBuf = Util.AllocBuffer(bufSize);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int w8 = bw / 8;
            for (int y = 0; y < bh; y++) {
                byte* sptr = (byte*)bmpData.Scan0 + bmpData.Stride * y;
                byte* dptr = (byte*)imgBuf + bw * y;
                for (int x8 = 0; x8 < w8; x8++, sptr++) {
                    byte sp = *sptr;
                    for (int x = 0; x < 8; x++, dptr++) {
                        if (((sp << x) & 0x80) != 0)
                            *dptr = 255;
                        else
                            *dptr = 0;
                    }
                }
            }

            bmp.UnlockBits(bmpData);
        }

        // Save Bitmap from Buffer
        public unsafe static Bitmap ImageBufferToBitmap(IntPtr imgBuf, int bw, int bh, int bytepp) {
            if (bytepp == 2) {
                return HraToBmp24(imgBuf, bw, bh, bytepp);
            }
            PixelFormat pixelFormat;
            if (bytepp == 1)
                pixelFormat = PixelFormat.Format8bppIndexed;
            else if (bytepp == 3)
                pixelFormat = PixelFormat.Format24bppRgb;
            else if (bytepp == 4)
                pixelFormat = PixelFormat.Format32bppRgb;
            else
                return null;

            Bitmap bmp = new Bitmap(bw, bh, pixelFormat);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.WriteOnly, bmp.PixelFormat);
            int copySize = bw * bytepp;
            int paddingSize = bmpData.Stride - copySize;
            byte[] paddingBuf = { 0, 0, 0, 0 };
            for (int y = 0; y < bh; y++) {
                IntPtr srcPtr = imgBuf + bw * y * bytepp;
                IntPtr dstPtr = bmpData.Scan0 + bmpData.Stride * y;
                Util.Memcpy(dstPtr, srcPtr, copySize);
                if (paddingSize > 0)
                    Marshal.Copy(paddingBuf, 0, dstPtr + copySize, paddingSize);
            }
            bmp.UnlockBits(bmpData);
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed) {
                var pal = bmp.Palette;
                for (int i = 0; i < pal.Entries.Length; i++) {
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                bmp.Palette = pal;
            }
            return bmp;
        }

        // hra to bmp24
        private unsafe static Bitmap HraToBmp24(IntPtr imgBuf, int bw, int bh, int bytepp) {
            Bitmap bmp = new Bitmap(bw, bh, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.WriteOnly, bmp.PixelFormat);
            int paddingSize = bmpData.Stride - bw * 3;
            byte[] paddingBuf = { 0, 0, 0, 0 };
            for (int y = 0; y < bh; y++) {
                byte* srcPtr = (byte*)imgBuf + bw * bytepp * y;
                byte* dstPtr = (byte*)bmpData.Scan0 + bmpData.Stride * y;
                for (int x = 0; x < bw; x++, srcPtr += bytepp, dstPtr += 3) {
                    byte gray = srcPtr[0];
                    dstPtr[0] = gray;
                    dstPtr[1] = gray;
                    dstPtr[2] = gray;
                }

                if (paddingSize > 0)
                    Marshal.Copy(paddingBuf, 0, (IntPtr)dstPtr, paddingSize);
            }
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        // hra Lolad
        public unsafe static void LoadHraFile(string fileName, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            Stream sr = null;
            try {
                sr = File.OpenRead(fileName);
                using (var br = new BinaryReader(sr)) {
                    sr = null;
                    br.BaseStream.Position = 252;
                    bytepp = br.ReadInt32();
                    bw = br.ReadInt32();
                    bh = br.ReadInt32();

                    int bufSize = bw * bh * bytepp;
                    imgBuf = Util.AllocBuffer(bufSize);

                    byte[] fbuf = br.ReadBytes(bufSize);
                    for (int y = 0; y < bh; y++) {
                        byte* dp = (byte*)imgBuf.ToPointer() + bw * bytepp * y;
                        int idx = bw * bytepp * y;
                        for (int x = 0; x < bw; x++, dp += bytepp, idx += bytepp) {
                            if (bytepp == 1)
                                dp[0] = fbuf[idx];
                            else if (bytepp == 2) {
                                dp[0] = fbuf[idx];
                                dp[1] = fbuf[idx + 1];
                            }
                        }
                    }
                }
            } finally {
                sr?.Dispose();
            }
        }

        // hra save
        public unsafe static void SaveHraFile(string fileName, IntPtr imgBuf, int bw, int bh, int bytepp) {
            Stream sr = null;
            try {
                sr = File.OpenWrite(fileName);
                using (var bwr = new BinaryWriter(sr)) {
                    sr = null;
                    for (int i = 0; i < 252; i++)
                        bwr.Write((byte)0);
                    bwr.Write(bytepp);
                    bwr.Write(bw);
                    bwr.Write(bh);

                    int bufSize = bw * bh * bytepp;
                    byte[] fbuf = new byte[bufSize];

                    for (int y = 0; y < bh; y++) {
                        byte* sp = (byte*)imgBuf.ToPointer() + bw * bytepp * y;
                        int idx = bw * bytepp * y;
                        for (int x = 0; x < bw; x++, sp += bytepp, idx += bytepp) {
                            if (bytepp == 1)
                                fbuf[idx] = sp[0];
                            else if (bytepp == 2) {
                                fbuf[idx] = sp[0];
                                fbuf[idx + 1] = sp[1];
                            }
                        }
                    }
                    bwr.Write(fbuf);
                }
            } finally {
                sr?.Dispose();
            }
        }
    }
}
