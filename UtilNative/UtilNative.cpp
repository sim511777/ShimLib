// UtilNative.cpp : DLL을 위해 내보낸 함수를 정의합니다.
//

#include "pch.h"
#include "framework.h"
#include "UtilNative.h"


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
    auto rasterizeAction = [=](int y) {
        int siy = siys[y];
        BYTE* sptr = (BYTE*)sbuf + (INT64)sbw * siy * bytepp;
        int* dp = (int*)dbuf + (INT64)dbw * y;
        for (int x = 0; x < dbw; x++, dp++) {
            int six = sixs[x];
            if (siy == -1 || six == -1) {   // out of boundary of image
                *dp = bgColor;
            }
            else {
                BYTE* sp = &sptr[six * bytepp];
                if (bytepp == 1) {          // 8bit gray
                    *dp = sp[0] | sp[0] << 8 | sp[0] << 16 | 0xff << 24;
                }
                else if (bytepp == 2) {   // 16bit gray (*.hra)
                    *dp = sp[0] | sp[0] << 8 | sp[0] << 16 | 0xff << 24;
                }
                else if (bytepp == 3) {   // 24bit bgr
                    *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                }
                else if (bytepp == 4) {   // 32bit bgra
                    *dp = sp[0] | sp[1] << 8 | sp[2] << 16 | 0xff << 24;
                }
            }
        }
    };
    if (useParallel) {
        parallel_for(0, dbh, 1, rasterizeAction);
    }
    else {
        for (int y = 0; y < dbh; y++)
            rasterizeAction(y);
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

    // dst 범위만큼 루프를 돌면서 해당 픽셀값 쓰기
    auto rasterizeAction = [=](int y){
        int siy = siys[y];
        BYTE* sptr = (BYTE*)sbuf + (INT64)sbw * siy * bytepp;
        int* dp = (int*)dbuf + (INT64)dbw * y;
        float ty0 = sitys[y];
        float ty1 = 1.0f - ty0;
        for (int x = 0; x < dbw; x++, dp++) {
            int six = sixs[x];
            if (siy == -1 || six == -1) {   // out of boundary of image
                *dp = bgColor;
            }
            else {
                BYTE* sp = &sptr[six * bytepp];
                float tx0 = sitxs[x];
                float tx1 = 1.0f - tx0;
                if (bytepp == 1) {          // 8bit gray
                    BYTE* sp1 = sp + 1;
                    BYTE* sp2 = sp + sbw;
                    BYTE* sp3 = sp2 + 1;
                    float grayIpl = (sp[0] * tx0 + sp1[0] * tx1) * ty0 + (sp2[0] * tx0 + sp3[0] * tx1) * ty1;
                    int gray = (int)grayIpl;
                    *dp = gray | gray << 8 | gray << 16 | 0xff << 24;
                }
                else if (bytepp == 2) {   // 16bit gray (*.hra)
                    BYTE* sp1 = sp + 2;
                    BYTE* sp2 = sp + sbw * 2;
                    BYTE* sp3 = sp2 + 2;
                    float grayIpl = (sp[0] * tx0 + sp1[0] * tx1) * ty0 + (sp2[0] * tx0 + sp3[0] * tx1) * ty1;
                    int gray = (int)grayIpl;
                    *dp = gray | gray << 8 | gray << 16 | 0xff << 24;
                }
                else if (bytepp == 3) {   // 24bit bgr
                    BYTE* sp1 = sp + 3;
                    BYTE* sp2 = sp + sbw * 3;
                    BYTE* sp3 = sp2 + 3;
                    float blIpl = (sp[0] * tx0 + sp1[0] * tx1) * ty0 + (sp2[0] * tx0 + sp3[0] * tx1) * ty1;
                    float grIpl = (sp[1] * tx0 + sp1[1] * tx1) * ty0 + (sp2[1] * tx0 + sp3[1] * tx1) * ty1;
                    float reIpl = (sp[2] * tx0 + sp1[2] * tx1) * ty0 + (sp2[2] * tx0 + sp3[2] * tx1) * ty1;
                    int blue = (int)blIpl;
                    int green = (int)grIpl;
                    int red = (int)reIpl;
                    *dp = blue | green << 8 | red << 16 | 0xff << 24;
                }
                else if (bytepp == 4) {   // 32bit bgra
                    BYTE* sp1 = sp + 4;
                    BYTE* sp2 = sp + sbw * 4;
                    BYTE* sp3 = sp2 + 4;
                    float blIpl = (sp[0] * tx0 + sp1[0] * tx1) * ty0 + (sp2[0] * tx0 + sp3[0] * tx1) * ty1;
                    float grIpl = (sp[1] * tx0 + sp1[1] * tx1) * ty0 + (sp2[1] * tx0 + sp3[1] * tx1) * ty1;
                    float reIpl = (sp[2] * tx0 + sp1[2] * tx1) * ty0 + (sp2[2] * tx0 + sp3[2] * tx1) * ty1;
                    int blue = (int)blIpl;
                    int green = (int)grIpl;
                    int red = (int)reIpl;
                    *dp = blue | green << 8 | red << 16 | 0xff << 24;
                }
            }
        }
    };

    if (useParallel) {
        parallel_for(0, dbh, 1, rasterizeAction);
    }
    else {
        for (int y = 0; y < dbh; y++)
            rasterizeAction(y);
    }

    delete[] siys;
    delete[] sixs;
    delete[] sitys;
    delete[] sitxs;
}
