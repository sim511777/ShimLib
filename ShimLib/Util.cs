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
    [StructLayout(LayoutKind.Sequential, Pack=2)]
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
        // 시간 측정 함수
        public static double GetPastTimeMs(long timeStampStart) {
            long timeStampEnd = Stopwatch.GetTimestamp();
            return (timeStampEnd - timeStampStart) * 1000.0 / Stopwatch.Frequency;
        }

        // 범위 제한 함수
        public static int IntClamp(int value, int min, int max) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // memset
        public unsafe static IntPtr memset(IntPtr _Dst, int _Val, long _Size) {
            byte valByte = (byte)_Val;
            byte* pdst = (byte*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, pdst++) {
                *pdst = valByte;
            }
            return _Dst;
        }

        // memcpy
        public unsafe static IntPtr memcpy(IntPtr _Dst, IntPtr _Src, long _Size) {
            byte* psrc = (byte*)_Src.ToPointer();
            byte* pdst = (byte*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, psrc++, pdst++) {
                *pdst = *psrc;
            }
            return _Dst;
        }

        // memset 4byte
        public unsafe static IntPtr memset4(IntPtr _Dst, uint _Val, long _Size) {
            uint* pdst = (uint*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, pdst++) {
                *pdst = _Val;
            }
            return _Dst;
        }

        // memcpy 4byte
        public unsafe static IntPtr memcpy4(IntPtr _Dst, IntPtr _Src, long _Size) {
            uint* psrc = (uint*)_Src.ToPointer();
            uint* pdst = (uint*)_Dst.ToPointer();
            for (long i = 0; i < _Size; i++, psrc++, pdst++) {
                *pdst = *psrc;
            }
            return _Dst;
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
            uint bufSize = fh.bfSize - fh.bfOffBits;

            // 정보 헤더
            BITMAPINFOHEADER ih = StreamReadStructure<BITMAPINFOHEADER>(hFile);
            if (ih.biBitCount != 8) {   // 컬러비트 체크
                hFile.Dispose();
                return  false;
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
                Marshal.Copy(fbuf, (fbh-y-1) * fstep, buf + y * bw, minw); 
            }

            hFile.Dispose();
            return true;
        }

        // 8bit 버퍼 bmp 파일에 저장
        static byte[] grayPalette = Enumerable.Range(0, 1024).Select(i => i % 4 == 3 ? (byte)0xff : (byte)(i / 4)).ToArray();
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
            byte[] paddingBuf = {0,0,0,0};

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
            imgBuf = Marshal.AllocHGlobal(new IntPtr(bufSize));
                
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bw, bh), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int copySize = bw * bytepp;
            for (int y = 0; y < bh; y++) {
                IntPtr dstPtr = imgBuf + bw * y * bytepp;
                IntPtr srcPtr = bmpData.Scan0 + bmpData.Stride * y;
                Util.memcpy(dstPtr, srcPtr, copySize);
            }
                
            bmp.UnlockBits(bmpData);
        }

        // Save Bitmap from Buffer
        public unsafe static Bitmap ImageBufferToBitmap(IntPtr imgBuf, int bw, int bh, int bytepp) {
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
            int paddingSize = bmpData.Stride - bw * bytepp;
            byte[] paddingBuf = Enumerable.Repeat((byte)0, 4).ToArray();
            for (int y = 0; y < bh; y++) {
                IntPtr srcPtr = imgBuf + bw * y * bytepp;
                IntPtr dstPtr = bmpData.Scan0 + bmpData.Stride * y;
                Util.memcpy(dstPtr, srcPtr, copySize);
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
        
        // hra Lolad
        public unsafe static void LoadHraFile(string fileName, ref IntPtr imgBuf, ref int bw, ref int bh, ref int bytepp) {
            using (var sr = File.OpenRead(fileName))
            using (var br = new BinaryReader(sr)) {
                sr.Position = 252;
                bytepp = br.ReadInt32();
                bw = br.ReadInt32();
                bh = br.ReadInt32();

                int bufSize = bw * bh * bytepp;
                imgBuf = Marshal.AllocHGlobal(bufSize);

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
        }

        // hra save
        public unsafe static void SaveHraFile(string fileName, IntPtr imgBuf, int bw, int bh, int bytepp) {
            using (var sr = File.OpenWrite(fileName))
            using (var bwr = new BinaryWriter(sr)) {
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
        }

        // 이미지 버퍼를 디스플레이 버퍼에 복사
        public unsafe static void CopyImageBufferZoom(IntPtr sbuf, int sbw, int sbh, IntPtr dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor) {
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

            // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
            for (int y = 0; y < dbh; y++) {
                int siy = siys[y];
                byte* sptr = (byte*)sbuf.ToPointer() + (Int64)sbw * siy * bytepp;
                int* dp = (int*)dbuf.ToPointer() + (Int64)dbw * y;
                for (int x = 0; x < dbw; x++, dp++) {
                    int six = sixs[x];
                    if (siy == -1 || six == -1) {   // out of boundary of image
                        *dp = bgColor;
                    } else {
                        byte *sp = &sptr[six * bytepp];
                        if (bytepp == 1) {          // 8bit gray
                            *dp = sp[0] | sp[0] << 8 | sp[0] << 16 | 0xff << 24;
                        } else if (bytepp == 2) {   // 16bit gray (*.hra)
                            *dp = sp[0] | sp[0] << 8 | sp[0] << 16 | 0xff << 24;
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
}
