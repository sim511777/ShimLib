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
    int* siys = new int[dbh];
    int* sixs = new int[dbw];
    float* sitys = new float[dbh];
    float* sitxs = new float[dbw];
    for (int y = 0; y < dbh; y++) {
        float siy = (float)((y - pany) / zoom) - 0.5f;
        siys[y] = (sbuf == nullptr || siy < 0 || siy > sbh - 1) ? -1 : (int)siy;
        sitys[y] = siys[y] + 1.0f - siy;
    }
    for (int x = 0; x < dbw; x++) {
        float six = (float)((x - panx) / zoom) - 0.5f;
        sixs[x] = (sbuf == nullptr || six < 0 || six > sbw - 1) ? -1 : (int)six;
        sitxs[x] = sixs[x] + 1.0f - six;
    }

    int sbw2 = sbw * 2;
    int sbw3 = sbw * 3;
    int sbw4 = sbw * 4;
    // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
    function<void(int)> rasterizeAction = [=](int y) {
        int siy = siys[y];
        BYTE* sptr = (BYTE*)sbuf + (INT64)sbw * siy * bytepp;
        int* dp = (int*)dbuf + (INT64)dbw * y;
        float ty0 = sitys[y];
        float ty1 = 1.0f - ty0;
        for (int x = 0; x < dbw; x++, dp++) {
            int six = sixs[x];
            if (siy == -1 || six == -1) {   // out of boundary of image
                *dp = bgColor;
            } else {
                BYTE* sp00 = &sptr[six * bytepp];
                float tx0 = sitxs[x];
                float tx1 = 1.0f - tx0;
                if (bytepp == 1) {          // 8bit gray
                    BYTE* sp01 = sp00 + 1;
                    BYTE* sp10 = sp00 + sbw;
                    BYTE* sp11 = sp10 + 1;
                    int v = (int)((sp00[0] * tx0 + sp01[0] * tx1) * ty0 + (sp10[0] * tx0 + sp11[0] * tx1) * ty1);
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 2) {   // 16bit gray (*.hra)
                    BYTE* sp01 = sp00 + 2;
                    BYTE* sp10 = sp00 + sbw2;
                    BYTE* sp11 = sp10 + 2;
                    int v = (int)((sp00[0] * tx0 + sp01[0] * tx1) * ty0 + (sp10[0] * tx0 + sp11[0] * tx1) * ty1);
                    *dp = v | v << 8 | v << 16 | 0xff << 24;
                } else if (bytepp == 3) {   // 24bit bgr
                    BYTE* sp01 = sp00 + 3;
                    BYTE* sp10 = sp00 + sbw3;
                    BYTE* sp11 = sp10 + 3;
                    int b = (int)((sp00[0] * tx0 + sp01[0] * tx1) * ty0 + (sp10[0] * tx0 + sp11[0] * tx1) * ty1);
                    int g = (int)((sp00[1] * tx0 + sp01[1] * tx1) * ty0 + (sp10[1] * tx0 + sp11[1] * tx1) * ty1);
                    int r = (int)((sp00[2] * tx0 + sp01[2] * tx1) * ty0 + (sp10[2] * tx0 + sp11[2] * tx1) * ty1);
                    *dp = b | g << 8 | r << 16 | 0xff << 24;
                } else if (bytepp == 4) {   // 32bit bgra
                    BYTE* sp01 = sp00 + 4;
                    BYTE* sp10 = sp00 + sbw4;
                    BYTE* sp11 = sp10 + 4;
                    int b = (int)((sp00[0] * tx0 + sp01[0] * tx1) * ty0 + (sp10[0] * tx0 + sp11[0] * tx1) * ty1);
                    int g = (int)((sp00[1] * tx0 + sp01[1] * tx1) * ty0 + (sp10[1] * tx0 + sp11[1] * tx1) * ty1);
                    int r = (int)((sp00[2] * tx0 + sp01[2] * tx1) * ty0 + (sp10[2] * tx0 + sp11[2] * tx1) * ty1);
                    *dp = b | g << 8 | r << 16 | 0xff << 24;
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
    delete[] sitys;
    delete[] sitxs;
}
