// UtilNative.cpp : DLL을 위해 내보낸 함수를 정의합니다.
//

#include "pch.h"
#include "framework.h"
#include "UtilNative.h"

using namespace std;
using namespace Concurrency;

// 내보낸 함수의 예제입니다.
UTILNATIVE_API int fnUtilNative(void)
{
    return 0;
}

// 범위 제한 함수
int IntClamp(int value, int min, int max) {
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

// 이미지 버퍼를 디스플레이 버퍼에 복사
UTILNATIVE_API void CopyImageBufferZoom(void* sbuf, int sbw, int sbh, void* dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor, BOOL useParallel) {
    // 인덱스 버퍼 생성
    int* siys = new int[dbh];
    int* sixs = new int[dbw];
    for (int y = 0; y < dbh; y++) {
        int siy = (int)floor((y - pany) / zoom);
        siys[y] = (sbuf == nullptr || siy < 0 || siy >= sbh) ? -1 : siy;
    }
    for (int x = 0; x < dbw; x++) {
        int six = (int)floor((x - panx) / zoom);
        sixs[x] = (sbuf == nullptr || six < 0 || six >= sbw) ? -1 : six;
    }

    // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
    function<void(int)> rasterizeAction = [=](int y) {
        int siy = siys[y];
        BYTE* sptr = (BYTE*)sbuf + (INT64)sbw * siy * bytepp;
        int* dp = (int*)dbuf + (INT64)dbw * y;
        for (int x = 0; x < dbw; x++, dp++) {
            int six = sixs[x];
            if (siy == -1 || six == -1) {   // out of boundary of image
                *dp = bgColor;
            } else {
                BYTE* sp = &sptr[six * bytepp];
                if (bytepp == 1) {          // 8bit gray
                    int v = sp[0];
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 2) {   // 16bit gray (*.hra)
                    int v = sp[0];
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 3) {   // 24bit bgr
                    *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                } else if (bytepp == 4) {   // 32bit bgra
                    *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                }
            }
        }
    };

    if (useParallel) {
        parallel_for(0, dbh, 1, rasterizeAction);
    } else {
        for (int y = 0; y < dbh; y++) rasterizeAction(y);
    }

    delete[] siys;
    delete[] sixs;
}

// 이미지 버퍼를 디스플레이 버퍼에 복사 확대시에 선형보간
UTILNATIVE_API void CopyImageBufferZoomIpl(void* sbuf, int sbw, int sbh, void* dbuf, int dbw, int dbh, int panx, int pany, double zoom, int bytepp, int bgColor, BOOL useParallel) {
    // 인덱스 버퍼 생성
    int* siy0s = new int[dbh];
    int* siy1s = new int[dbh];
    int* six0s = new int[dbw];
    int* six1s = new int[dbw];
	int* sity0s = new int[dbh];
	int* sity1s = new int[dbh];
	int* sitx0s = new int[dbw];
	int* sitx1s = new int[dbw];
    for (int y = 0; y < dbh; y++) {
        double siy = (y + 0.5 - pany) / zoom - 0.5;
        if (sbuf == nullptr || siy < -0.5 || siy >= sbh - 0.5) {
            siy0s[y] = -1;
            continue;
        }
        int siy0 = (int)floor(siy);
        int siy1 = siy0 + 1;
		sity0s[y] = (int)((siy1 - siy) * 256);
		sity1s[y] = (int)((siy - siy0) * 256);
        siy0s[y] = IntClamp(siy0, 0, sbh - 1);
        siy1s[y] = IntClamp(siy1, 0, sbh - 1);
    }
    for (int x = 0; x < dbw; x++) {
        double six = (x + 0.5 - panx) / zoom - 0.5;
        if (sbuf == nullptr || six < -0.5 || six >= sbw - 0.5) {
            six0s[x] = -1;
            continue;
        }
        int six0 = (int)floor(six);
        int six1 = six0 + 1;
		sitx0s[x] = (int)((six1 - six) * 256);
		sitx1s[x] = (int)((six - six0) * 256);
        six0s[x] = IntClamp(six0, 0, sbw - 1);
        six1s[x] = IntClamp(six1, 0, sbw - 1);
    }

    // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
    function<void(int)> rasterizeAction = [=](int y) {
        int siy0 = siy0s[y];
        int siy1 = siy1s[y];
        BYTE* sptr0 = (BYTE*)sbuf + (INT64)sbw * siy0 * bytepp;
        BYTE* sptr1 = (BYTE*)sbuf + (INT64)sbw * siy1 * bytepp;
        int* dp = (int*)dbuf + (INT64)dbw * y;
        int ty0 = sity0s[y];
        int ty1 = sity1s[y];
        for (int x = 0; x < dbw; x++, dp++) {
            int six0 = six0s[x];
            int six1 = six1s[x];
            if (siy0 == -1 || six0 == -1) {   // out of boundary of image
                *dp = bgColor;
            } else {
                BYTE* sp00 = &sptr0[six0 * bytepp];
                BYTE* sp01 = &sptr0[six1 * bytepp];
                BYTE* sp10 = &sptr1[six0 * bytepp];
                BYTE* sp11 = &sptr1[six1 * bytepp];
                int tx0 = sitx0s[x];
                int tx1 = sitx1s[x];
                int t00 = ty0 * tx0;
                int t01 = ty0 * tx1;
                int t10 = ty1 * tx0;
                int t11 = ty1 * tx1;
                if (bytepp == 1) {          // 8bit gray
                    int v = (sp00[0] * t00 + sp01[0] * t01 + sp10[0] * t10 + sp11[0] * t11) >> 16;
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 2) {   // 16bit gray (*.hra)
                    int v = (sp00[0] * t00 + sp01[0] * t01 + sp10[0] * t10 + sp11[0] * t11) >> 16;
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 3) {   // 24bit bgr
                    int b = (sp00[0] * t00 + sp01[0] * t01 + sp10[0] * t10 + sp11[0] * t11) >> 16;
                    int g = (sp00[1] * t00 + sp01[1] * t01 + sp10[1] * t10 + sp11[1] * t11) >> 16;
                    int r = (sp00[2] * t00 + sp01[2] * t01 + sp10[2] * t10 + sp11[2] * t11) >> 16;
                    *dp = b | g << 8 | r << 16 | 0xff << 24;
                } else if (bytepp == 4) {   // 32bit bgra
                    int b = (sp00[0] * t00 + sp01[0] * t01 + sp10[0] * t10 + sp11[0] * t11) >> 16;
                    int g = (sp00[1] * t00 + sp01[1] * t01 + sp10[1] * t10 + sp11[1] * t11) >> 16;
                    int r = (sp00[2] * t00 + sp01[2] * t01 + sp10[2] * t10 + sp11[2] * t11) >> 16;
                    *dp = b | g << 8 | r << 16 | 0xff << 24;
                }
            }
        }
    };

    if (useParallel) {
        parallel_for(0, dbh, rasterizeAction);
    } else {
        for (int y = 0; y < dbh; y++) rasterizeAction(y);
    }

    delete[] siy0s;
    delete[] siy1s;
    delete[] six0s;
    delete[] six1s;
	delete[] sity0s;
	delete[] sity1s;
	delete[] sitx0s;
	delete[] sitx1s;
}
