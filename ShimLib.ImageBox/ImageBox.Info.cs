using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public partial class ImageBox {
        public const string VersionHistory =
@"ImageBox for .NET
v1.0.0.20 - 20200708
1. VersionHistory partial class로 분리
2. 이미지 좌표계 변경, 0,0 이 첫번째 픽셀의 가운데가 되도록 수정

v1.0.0.19 - 20200611
1. EraseBackground 아무것도 안하도록 재정의
2. BufferdGraphics 사용 속도 증가 14ms -> 10ms

v1.0.0.18 - 20200531
1. Interpolation 기능 제거
2. Parallel 기능 제거
3. PaintBackBuffer 이벤트 추가
4. FontAscii_5x8 추가
5. FontAscii_5x12, Unifont 삭제
6. PanX, PanY int 타입으로 변경
7. PointD 에서 PointF 로 변경
8. 픽셀값 표시 3자리 정렬
9. Pan Clamp 처리시 2pixel 남김

v1.0.0.17 - 20200522
1. BitmapFont CMD 레스터 폰트 캡쳐한것으로 변경
  - FontAscii_4x6
  - FontAscii_5x12
2. BitmapFont 렌더링시 윈도우 경계부분 글자단위로 짤리는 문제 수정

v1.0.0.16 - 20200505
1. DrawPixelValue()에 SKIA 제거
  - 의존하는 추가 DLL들이 너무 많음
  - DrawString 외에 다른 함수는 느림
  - 실제적으로 DrawPixelValue에서만 유용한데, BitmapFontRender으로 충분

v1.0.0.15 - 20200429
1. ShimLib.Util.dll 분리
2. CopyImageBuffer 함수 Util클래스에서 ImageBox 클래스로 이동
3. 1bit bmp 로딩 가능하도록 수정
4. DrawString 함수 하나로 통일
5. 그래픽은 이미지버퍼에 모두 그림
6. DrawPixelValue()에 SKIA 사용 - 렌더링 시간 줄일수 있음
7. WPF PointD 없애고 직접 정의
8. ImageBuffer 클래스 추가
9. Bitmap FontRenderer 추가
10. 설정창 속성 변경시 즉시 적용, 취소시 원복
11. UniFont 렌더링 기능 추가

v1.0.0.14 - 20200325
1. ZoomLevelMin, ZoomLevelMax 속성 추가
2. UseMousePanClamp 속성 추가

v1.0.0.13 - 20200320
1. 16비트 hra버퍼 저장 시 24비트 bmp로 저장 되도록 수정

v1.0.0.12 - 20200316
1. float, double buffer 표시 기능 추가
2. float, double buffer 전처리 해서 표시 기능 추가
3. 버파 파일저장 시 8bit png 포멧으로 저장되는 버그 수정

v1.0.0.11 - 20200315
1. 옵션창에서 버퍼 파일저장, 버퍼 클립보드 복사 추가

v1.0.0.10 - 20200308
1. 개별 픽셀 표시 폰트와 일반 표시 폰트 두가지 따로 갈것
2. pseudo 기본 컬러 더 잘보이는 것으로 수정

v1.0.0.9 - 20200305
1. BufferedGraphics 안쓰고 DoubleBuffered = true; 사용
2. MouseMove 오동작 수정

v1.0.0.8 - 20200304
1. 버전정보 창에 속성 변경기능 추가
2. 쿼드클릭 대신 ctrl + 더블클릭 누를때 버전정보 창 띄움
3. ShowAbout() 함수 추가

v1.0.0.7 - 20200217
1. DrawInfo() 깜빡이 않게 더블버퍼 처리

v1.0.0.6 - 20200214
1. ImageBox로 이름 변경
2. ImgToDisp(), DIspToImg() 함수 PointD 타입으로 변경
3. ImageGraphics 클래스 추가 및 테스트
4. immediate 드로잉 마우스 Move시에 안지워지도록 수정
5. PixelValueDispZoomFactor 속성 추가

v1.0.0.5 - 20200131
1. Quadruple 클릭시 버전정보창 띄움 (마우스 다운이 아닌 마우스 업에서 처리)
2. PtPanning => double PanX, PanY 로 변경
3. 더 큰 이미지 (2000000width) 표시시 CenterLine 오버플로우 다운 수정
4. 더 큰 이미지 (2000000width) 표시시 PanX, PanY int타입 오버플로우로 발생하는 계산 에러 수정
5. 더 큰 이미지 (2000000width) 표시를 위해서 zoom레벨 (1/10000000x) ~ (1000000x)로 수정
6. DrawString()시 Control.Font 사용

v1.0.0.4 - 20200129
1. 필터링시 +0.5 offset 추가
2. panning 좌표 FloatF 로 변경 하여 축소 확대시 위치 안벗어남
3. Native.dll 불필요 해서 지움
4. Quadruple 클릭시 버전정보창 띄움
5. PtPanning 속성 숨김

v1.0.0.3 - 20200127
1. 필터링시 가장자리 0.5픽셀 처리 안하던것 처리하도록 수정
2. SetImgBuf 함수에 bInvalidate 파라미터 추가

v1.0.0.2 - 20200119
1. 레스터라이즈 Parallel 라이브러리 사용해서멀티쓰레드 처리
2. C++ dll 프로젝트 추가
3. Draw Time 표시에 추가 정보 포함(이미지버퍼, 드로우옵션, 마우스옵션) 

v1.0.0.1 - 20200116
1. 확대시 선형보간 기능 추가
2. DispToImg(Rectangle rect) 버그 수정 - Floor 하지 말아야 함

v1.0.0.0 - 20200115
1. 기본 기능 완성
2. 버전정보 추가

v0.1.0.1 - 20191201
1. 기본 알고리즘 구현

v0.0.0.0 - 20191001
1. 설계
  - Native 이미지 버퍼 표시 기능
  - 아주 큰 이미지도 표시 가능
  - 마우스 줌, 마우스 패닝 기능
  - 확대 되었을때 픽셀값 표시
  - 마우스 이동시 커서 위치의 픽셀좌표와 픽셀값 표시
  - C, C# 모두 구현하여 속도 비교 테스트
  - 닷넷 컨트롤로 구현하여 폼디자이너에서 사용하기 쉽게 만듦
";
    }
}
