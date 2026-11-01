# PC 스마트 리모컨 프로젝트 with Joy-Con

데스크탑 PC에서 편하게 쓸 무선 입력 장치를 찾던 중, 추가 장비를 구매하는 대신 집에 있던 **닌텐도 스위치 조이콘(Joy-Con)**을 범용 마우스 겸 리모컨으로 활용해보고자 시작된 토이 프로젝트입니다.

- **시작 배경**: 프로콘과 마찬가지로 조이콘 역시 블루투스를 통해 PC 연결 및 입력 제어가 가능하므로, 마우스 기능을 충분히 대체할 수 있을 것으로 판단
- **주요 목적**: 키보드 타이핑보다는 **한 손 마우스 조작 및 웹서핑·미디어 제어(재생/볼륨/스크롤)**에 최적화

## 1. 참고 자료 원본 링크
웹 검색을 통해 조이콘 컨트롤러를 PC에서 마우스로 활용하기 위한 다양한 시도와 도구들이 존재한다는 것을 확인했습니다.
- https://joytokey.net/en/ - 조이콘 마우스로 변환
- https://github.com/merlinstardust/joycon-key-mappings - 조이콘 마우스 키매핑
- https://github.com/davidobot/betterjoy - 조이콘 자이로 인식
- https://github.com/JibbSmart/JoyShockMapper - 조이콘 마우스 매핑 (최신 SDL2 릴리스: https://github.com/Electronicks/JoyShockMapper)
- https://www.nintendo.com/kr/hardware/modal/specs/joy-con.html?width=960 - 한국닌텐도 공식 Joy-Con (L)/(R) 하드웨어 사양 및 버튼 배치도

---

## 2. 자료 비교 분석 요약

| 도구 / 자료 | 핵심 동작 원리 | 자이로(모션) 지원 | 드라이버 설치 | 좌/우 개별 사용 특징 |
| :--- | :--- | :---: | :---: | :--- |
| **1. JoyToKey** + **2. joycon-key-mappings** | Windows DirectInput으로 조이콘의 아날로그 스틱과 버튼을 키보드/마우스 입력으로 변환 | ❌ 미지원 (스틱만 가능) | 불필요 | 설정이 직관적이나 자이로(에어마우스) 센서를 읽을 수 없고 셰어웨어 팝업이 존재함 |
| **3. BetterJoy** | 조이콘 HID 통신으로 자이로/가속도계 인식 및 Xbox 360(XInput) 가상 패드 에뮬레이션 | ⭕ 지원 | **ViGEmBus** 및 **HidGuardian** 설치 필요 | 에뮬레이터(Cemu/Yuzu) 및 패드 전용 게임에 유리하나 시스템 드라이버 설치가 무거움 |
| **4. JoyShockMapper (채택)** | **SDL2 기반 네이티브 HID 통신**으로 조이콘의 3축 자이로 + 스틱 + 버튼을 직접 마우스/키보드로 매핑 | ⭕ **정밀 지원** (`PLAYER_TURN` 공간 보정, 2단 가속) | **불필요** (무설치 포터블 실행) | `RECONNECT_CONTROLLERS SPLIT`과 `JOYCON_GYRO_MASK = USE_BOTH`를 통해 **좌/우 조이콘을 각각 독립된 무선 에어마우스+스틱 마우스**로 즉시 사용 가능 |

> **결론**: 별도의 가상 버스 드라이버 설치 없이 **자이로 모션(에어마우스)**과 **아날로그 스틱 마우스**, 그리고 **탭/홀드/더블탭/조합키(Fn 레이어)**까지 모두 지원하는 **JoyShockMapper (v3.6.1 SDL2)** 기반으로 세팅 및 최적화를 완료했습니다.

---

## 3. 빠른 시작 방법 (Quick Start)

1. **블루투스 연결**:
   - 사용할 조이콘(**L 단독**, **R 단독**, 또는 **L+R 모두**) 측면 레일(`SL`과 `SR` 버튼 사이)의 **작은 동그란 싱크(SYNC) 버튼**을 LED가 왕복 점멸할 때까지 길게 누릅니다.
   - Windows `설정 > Bluetooth 및 장치`에서 **`Joy-Con (L)`** 및/또는 **`Joy-Con (R)`**을 연결합니다.
2. **마우스 프로그램 즉시 실행 (키보드 입력 불필요)**:
   - **[`조이콘_마우스_실행.bat`](./조이콘_마우스_실행.bat)**: 더블 클릭하면 **즉시 하이브리드 마우스 모드(아날로그 스틱 커서 이동 + 측면 버튼 홀드 시 정밀 자이로 에어마우스 + 버튼 휠 스크롤)**로 시작됩니다.
   - **[`모드2_자이로포인터_스틱스크롤_실행.bat`](./모드2_자이로포인터_스틱스크롤_실행.bat)**: 마우스 포인터는 자이로 모션으로 움직이고, **아날로그 스틱 상/하로 웹페이지 휠 스크롤**을 수행하는 모드로 즉시 시작합니다.
   - **[`모드3_스틱전용_마우스_실행.bat`](./모드3_스틱전용_마우스_실행.bat)**: 자이로를 완전히 끄고 **아날로그 스틱으로만 마우스 커서를 이동**하는 모드로 즉시 시작합니다.
3. **버튼 및 감도 커스터마이징 (GUI 지원)**:
   - **[`조이콘_버튼_커스텀_GUI.html`](./조이콘_버튼_커스텀_GUI.html)**: 브라우저에서 공식 사양과 동일한 조이콘 그림의 버튼을 클릭해 기능을 바꾸고 `.txt` 설정 파일을 바로 열거나 덮어써서 저장할 수 있습니다.
   - **[`조이콘_버튼_커스텀_GUI.exe`](./조이콘_버튼_커스텀_GUI.exe)**: 별도 설치 없이 바로 실행되는 Windows 전용 네이티브 GUI 편집기입니다.

---

## 4. 좌/우 조이콘 개별 사용 키 매핑표 (기본 하이브리드 모드 기준)

왼손 조이콘(`Joy-Con (L)`)을 들든 오른손 조이콘(`Joy-Con (R)`)을 들든 **동일한 손가락 위치에서 똑같은 마우스/미디어 기능**이 작동하도록 설계되어 있습니다.
> ※ 실제 하드웨어 구조상 측면 레일 위쪽 버튼은 **좌측 조이콘이 `LSL`**, **우측 조이콘이 `RSR`**이며, 레일 아래쪽 버튼은 **좌측 조이콘이 `LSR`**, **우측 조이콘이 `RSL`**입니다.

### 4.1 기본 마우스 & 브라우저 조작

| 조작 부위 | 왼쪽 조이콘 (L) | 오른쪽 조이콘 (R) | 동작 기능 |
| :--- | :---: | :---: | :--- |
| **아날로그 스틱 밀기** | `LEFT_STICK` | `RIGHT_STICK` | **마우스 커서 이동** (손을 떼면 커서가 흔들림 없이 즉시 고정됨) |
| **측면 위쪽 레일 버튼 누른 채 조작** | `LSL` 홀드 | `RSR` 홀드 | **정밀 감속 스틱(`360 → 140`) + 자이로 에어마우스 활성화** |
| **방아쇠 트리거 (검지)** | `ZL` | `ZR` | **마우스 좌클릭** (누른 채 이동 시 드래그 앤 드롭) |
| **상단 숄더 범퍼 (검지)** | `L` | `R` | **마우스 우클릭** |
| **스틱 누르기 (엄지)** | `L3` | `R3` | **짧게 클릭**: 마우스 휠 클릭 (링크 새 탭 열기)<br>**길게 누르기**: `Space` (영상 재생/일시정지) |
| **전면 위쪽 버튼** | `UP` (▲) | `X` (`N`) | **마우스 휠 위로 스크롤** (누르고 있으면 연속 고속 스크롤) |
| **전면 아래쪽 버튼** | `DOWN` (▼) | `B` (`S`) | **마우스 휠 아래로 스크롤** (누르고 있으면 연속 고속 스크롤) |
| **전면 왼쪽 버튼** | `LEFT` (◀) | `Y` (`W`) | **짧게 클릭**: 왼쪽 화살표 `←` (유튜브 5초 뒤로)<br>**길게 누르기**: 웹 브라우저 **뒤로 가기 (`BMOUSE`)** |
| **전면 오른쪽 버튼** | `RIGHT` (▶) | `A` (`E`) | **짧게 클릭**: 오른쪽 화살표 `→` (유튜브 5초 앞으로)<br>**길게 누르기**: 웹 브라우저 **앞으로 가기 (`FMOUSE`)** |
| **상단 보조 버튼** | `-` (마이너스) | `+` (플러스) | **짧게 클릭**: `ESC` (전체화면 해제/취소)<br>**길게 누르기**: `Win + Tab` (윈도우 작업 보기 / 창 전환)<br>**더블 탭 (`-,-` / `+,+`)**: `Ctrl + W` (현재 브라우저 탭 닫기) |
| **하단 기능 버튼** | `CAPTURE` (캡처) | `HOME` (홈) | **클릭 / 길게 누르기**: **자이로 토글 및 영점 재보정 (`CALIBRATE`)**<br>**더블 탭 (`CAPTURE,CAPTURE` / `HOME,HOME`)**: `Win + D` (바탕화면 보기/복귀) |

---

### 4.2 측면 아래쪽 레일 버튼 (`LSR` / `RSL`) 조합 (`Fn` 미디어/단축키 레이어)

측면 레일 아래쪽 버튼(**좌측 `LSR` / 우측 `RSL`**)은 단독으로 짧게 누르면 **미디어 재생/일시정지**가 되며, **누른 상태**에서 다른 버튼을 누르면 보조 단축키가 작동합니다.

| 조합 입력 (왼쪽 L) | 조합 입력 (오른쪽 R) | 동작 기능 |
| :---: | :---: | :--- |
| `LSR` 단독 클릭 | `RSL` 단독 클릭 | **미디어 재생 / 일시정지 (`PLAY_PAUSE`)** |
| `LSR,UP` (▲) | `RSL,N` (`X`) | **시스템 볼륨 업 (`VOLUME_UP`)** (누르고 있으면 연속 증가) |
| `LSR,DOWN` (▼) | `RSL,S` (`B`) | **시스템 볼륨 다운 (`VOLUME_DOWN`)** (누르고 있으면 연속 감소) |
| `LSR,L3` (스틱클릭) | `RSL,R3` (스틱클릭) | **음소거 토글 (`MUTE`)** |
| `LSR,LEFT` (◀) | `RSL,W` (`Y`) | **웹 브라우저 뒤로 가기 (`BMOUSE`)** 즉시 실행 |
| `LSR,RIGHT` (▶) | `RSL,E` (`A`) | **웹 브라우저 앞으로 가기 (`FMOUSE`)** 즉시 실행 |
| `LSR,ZL` (트리거) | `RSL,ZR` (트리거) | **엔터 (`ENTER`)** |
| `LSR,L` (범퍼) | `RSL,R` (범퍼) | **백스페이스 (`BACKSPACE`)** |

---

## 5. 릴리즈 노트 및 주요 개선 사항 (Release Notes & Improvements)

### v1.2 — 공식 사양 기반 HTML/네이티브 커스텀 GUI 출시
- **HTML 기반 시각적 버튼 커스텀 GUI ([`조이콘_버튼_커스텀_GUI.html`](./조이콘_버튼_커스텀_GUI.html)) 추가**:
  - 한국닌텐도 공식 Joy-Con 사양 페이지와 1:1로 매칭된 듀얼 조이콘(L/R) 인터랙티브 UI 탑재.
  - 브라우저에서 물리 버튼을 직접 클릭하여 프리셋 변경 및 **키보드 키 직접 감지(Key Capture)** 지원.
  - File System Access API를 활용하여 브라우저에서 `JoyCon_Individual_Hybrid.txt` 등 설정 파일을 직접 불러오고 즉시 덮어쓰기 저장 지원.
- **공식 사양 인터랙티브 버튼 매칭 프리뷰 ([`joycon-preview.html`](./joycon-preview.html)) 추가**:
  - `ZL`/`L`, `ZR`/`R` 상단 숄더·트리거, 분리형 원형 방향 버튼(`▲/▼/◀/▶`), 정사각형 음각 `캡처 버튼`, `HOME 버튼`, 측면 레일(`LSL`/`LSR`, `RSR`/`RSL`, `플레이어 LED`, `SYNC`) 등 공식 하드웨어 외형 구현 및 매핑 코드 가이드 제공.
- **Windows 네이티브 실행형 커스텀 GUI ([`조이콘_버튼_커스텀_GUI.exe`](./조이콘_버튼_커스텀_GUI.exe) / [`JoyConCustomGUI.cs`](./JoyConCustomGUI.cs)) 추가**:
  - 별도 프레임워크 설치 없이 Windows에서 즉시 실행되어 두 프로필(`JoyCon_Individual_Hybrid.txt`, `JoyCon_Individual_StickOnly.txt`)에 동시 저장할 수 있는 WinForms 기반 실행 파일 및 C# 소스 코드 제공.

### v1.1 — 마우스 포인터 좌측 상단 쏠림(Drift) 및 콘솔 종료 버그 해결
- **`SDL2.dll` 조이콘 스틱 캘리브레이션 버퍼 덮어쓰기 버그 패치**:
  - **문제 원인**: `JoyShockMapper v3.6.1` 내장 `SDL2.dll`의 `LoadStickCalibration`(`0x1EB0E0`) 함수가 조이콘 SPI 플래시에서 사용자 보정값(`0x8010`)과 공장 보정값(`0x603D`)을 연속 호출할 때 동일한 수신 버퍼(`ctx->m_rgucReadBuffer`)를 공유하여 스틱 보정 데이터가 손상되고, 중립 상태에서 스틱이 좌측 상단(`LEFT` + `UP`) 최대치로 고정 인식되던 문제 규명.
  - **해결 내용**: `SDL2.dll` 내 `LoadStickCalibration`의 분기 명령 3곳(`0x1EB675`, `0x1EB6C2`, `0x1EB73B`)을 패치하여 손상된 SPI 데이터 대신 `SDL2` 자체 표준 중심값(`2048` / `0x800`, 가동 범위 `±70%`) 폴백 루틴이 동작하도록 수정 (원본 파일은 `SDL2.dll.bak`으로 백업).
- **평상시 포인터 흔들림 방지 (정밀 자이로 홀드 방식 도입)**:
  - 기본 자이로 감도를 `0`으로 두어 스틱을 건드리지 않을 때 마우스 포인터가 화면에서 완전히 고정되도록 개선하고, 측면 상단 레일 버튼(`RSR` / `LSL`)을 누르는 동안에만 자이로 에어마우스와 정밀 감속 스틱이 작동하도록 개선.
- **JoyShockMapper v3.6.1 설정 스크립트 파싱 에러 전면 수정**:
  - 콘솔 시작 시 발생하던 조합키·더블탭 문법 오류(`ESC LWINDOWS+!TAB`, `++`, `CAPTURE+`, `HOME+`)를 정식 문법(`ESC' LWINDOWS_ !TAB_`, `-,- = LCONTROL\ !W\`, `+,+ = LCONTROL\ !W\`, `CAPTURE,CAPTURE = LWINDOWS\ !D\`, `HOME,HOME = LWINDOWS\ !D\`)으로 교정하여 파싱 에러 0건 달성.

### v1.0 — 초기 릴리즈
- `JoyShockMapper v3.6.1 (SDL2 x64)` 포터블 환경 구축 및 좌/우 조이콘 독립 분리(`SPLIT`) 자동화.
- 3가지 마우스 작동 모드(하이브리드 / 자이로 포인터+스틱 스크롤 / 스틱 전용) 원클릭 `.bat` 런처 구성.

---

## 6. 프로젝트 폴더 구조 (File Tree) 및 구성 파일 안내

```text
📦 조이콘(Joy-Con) 마우스 변환 프로젝트
├── 🚀 조이콘_마우스_실행.bat                    # [모드 1] 기본 하이브리드 마우스 즉시 실행 런처
├── 🚀 모드2_자이로포인터_스틱스크롤_실행.bat    # [모드 2] 자이로 포인터 + 스틱 스크롤 실행 런처
├── 🚀 모드3_스틱전용_마우스_실행.bat            # [모드 3] 자이로 OFF 순수 스틱 마우스 실행 런처
│
├── 🎨 조이콘_버튼_커스텀_GUI.html               # [추천] 공식 사양 디자인 기반 HTML 버튼/감도 커스텀 GUI
├── 🎨 joycon-preview.html                       # 닌텐도 공식 사양 기준 Joy-Con (L)/(R) 버튼 1:1 매칭 프리뷰
├── 🖥️ 조이콘_버튼_커스텀_GUI.exe                # Windows 네이티브 버튼 커스텀 GUI 실행 파일
├── 📄 JoyConCustomGUI.cs                        # 네이티브 커스텀 GUI C# 원본 소스 코드
│
├── 📂 JoyShockMapper/
│   └── 📂 JoyShockMapper_SDL2_x64/              # 핵심 실행 엔진 폴더 (필수 파일만 유지)
│       ├── ⚙️ JoyShockMapper.exe                # 오픈소스 컨트롤러 매핑 메인 엔진 (v3.6.1)
│       ├── 🔧 SDL2.dll                          # 스틱 좌측 상단 쏠림 버그가 패치된 SDL2 통신 라이브러리
│       ├── 📝 OnStartup.txt                     # 프로그램 시작 시 자동 로드할 프로필 경로 지정 파일
│       ├── 📝 OnReset.txt                       # RESET_MAPPINGS 시 적용되는 기본 캘리브레이션 설정
│       └── 📂 GyroConfigs/                      # 조이콘 마우스 모드별 설정집(대본) 보관 폴더
│           ├── 📄 JoyCon_Individual_Hybrid.txt      # [프로필 1] 스틱 + 정밀 자이로 하이브리드 설정
│           ├── 📄 JoyCon_Individual_GyroScroll.txt  # [프로필 2] 자이로 포인터 + 스틱 스크롤 설정
│           └── 📄 JoyCon_Individual_StickOnly.txt   # [프로필 3] 자이로 OFF 순수 스틱 마우스 설정
│
├── 📂 trash/                                    # 실행에 불필요하여 분리 보관된 원본 부속/샘플 파일들
│   ├── 🔧 SDL2.dll.bak                          # 패치 전 순정 SDL2.dll 백업본
│   ├── 📄 LICENSE.md / README.md                # JoyShockMapper 영문 라이선스 및 원본 설명서
│   ├── 📂 AutoLoad/                             # 미사용 자동 로드 안내 폴더
│   └── 📂 GyroConfigs/                          # 미사용 타 컨트롤러(PS4, Xbox 등) 기본 샘플 설정들
│
├── 🛠️ setup_joyshockmapper.ps1                  # JoyShockMapper 릴리스 다운로드/초기화 스크립트
├── 📦 JoyShockMapper.zip                        # JoyShockMapper_SDL2_x64 원본 압축 아카이브
└── 📖 readme.md                                 # 프로젝트 통합 문서 (현재 파일)
```

### 주요 구성 파일 상세 설명

#### 1) 실행 런처 및 커스텀 GUI
- [`조이콘_마우스_실행.bat`](./조이콘_마우스_실행.bat): 원클릭 모드 선택 및 JoyShockMapper 실행 런처 (기본 하이브리드 모드)
- [`모드2_자이로포인터_스틱스크롤_실행.bat`](./모드2_자이로포인터_스틱스크롤_실행.bat): 자이로 포인터 + 스틱 휠 스크롤 모드 즉시 실행 런처
- [`모드3_스틱전용_마우스_실행.bat`](./모드3_스틱전용_마우스_실행.bat): 자이로 OFF 순수 아날로그 스틱 마우스 모드 즉시 실행 런처
- [`조이콘_버튼_커스텀_GUI.html`](./조이콘_버튼_커스텀_GUI.html): **[추천]** 공식 사양 디자인 기반 HTML 버튼/감도 커스텀 GUI (브라우저 실행, `.txt` 열기/저장 지원)
- [`joycon-preview.html`](./joycon-preview.html): 한국닌텐도 공식 사양 기준 Joy-Con (L)/(R) 물리 버튼 1:1 매칭 인터랙티브 프리뷰
- [`조이콘_버튼_커스텀_GUI.exe`](./조이콘_버튼_커스텀_GUI.exe): Windows 네이티브 버튼 커스텀 GUI 실행 파일 (원본 소스: [`JoyConCustomGUI.cs`](./JoyConCustomGUI.cs))

#### 2) 설정 프로필 및 코어 엔진 파일
- [`JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_Hybrid.txt`](./JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_Hybrid.txt): **[프로필 1]** 스틱 + 정밀 자이로 하이브리드 마우스 설정
- [`JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_GyroScroll.txt`](./JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_GyroScroll.txt): **[프로필 2]** 자이로 포인터 + 스틱 스크롤 설정
- [`JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_StickOnly.txt`](./JoyShockMapper/JoyShockMapper_SDL2_x64/GyroConfigs/JoyCon_Individual_StickOnly.txt): **[프로필 3]** 자이로 OFF 순수 스틱 마우스 설정
- [`JoyShockMapper/JoyShockMapper_SDL2_x64/SDL2.dll`](./JoyShockMapper/JoyShockMapper_SDL2_x64/SDL2.dll): 스틱 보정 버퍼 덮어쓰기 버그가 패치된 SDL2 라이브러리 (원본 백업: [`trash/SDL2.dll.bak`](./trash/SDL2.dll.bak))
- [`setup_joyshockmapper.ps1`](./setup_joyshockmapper.ps1): JoyShockMapper 릴리스 자동 다운로드/초기화 스크립트
