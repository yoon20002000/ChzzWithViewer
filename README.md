# ChzzWithViewer

치지직(Chzzk) 스트리밍 채팅을 실시간으로 수신하여 룰렛 게임을 진행할 수 있는 Unity 애플리케이션입니다.

## 🎯 주요 기능

### 📺 실시간 치지직 채팅 연동
- 치지직 스트리밍 채팅 실시간 수신
- 채팅 메시지, 도네이션, 구독 이벤트 처리
- WebSocket을 통한 안정적인 연결 유지
- 자동 재연결 기능

### 🎰 룰렛 게임 시스템
- **가중치 기반 룰렛**: 각 항목별로 다른 확률 설정 가능
- **실시간 데이터 수집**: 채팅을 통한 룰렛 항목 추가
- **도네이션 연동**: 도네이션 금액에 따른 가중치 증가
- **시각적 룰렛**: Unity UI를 통한 애니메이션 룰렛

### ⚙️ 설정 관리
- **금지어 관리**: 특정 단어 필터링
- **해상도 설정**: 다양한 해상도 및 전체화면 모드 지원
- **그래픽 품질**: 낮음/보통/높음 설정
- **설정 저장**: JSON 형태로 설정 파일 자동 저장

## 🏗️ 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── ChzzAPI/                    # 치지직 API 연동
│   │   ├── ChzzkUnity.cs          # 메인 API 클래스
│   │   └── DataTypes/             # API 데이터 구조체
│   ├── Roulette/                   # 룰렛 게임 시스템
│   │   ├── RouletteGame.cs        # 메인 게임 로직
│   │   ├── RouletteDataManager.cs # 룰렛 데이터 관리
│   │   └── Data/
│   │       └── RoulettePieceData.cs # 항목 데이터
│   ├── Setting/                    # 설정 관리
│   │   ├── GameSettingData.cs     # 설정 데이터 구조
│   │   └── GameSettingManager.cs  # 설정 관리자
│   ├── UI/                         # 사용자 인터페이스
│   │   ├── Setting/               # 설정 UI
│   │   └── UI_PieceData.cs        # 룰렛 데이터 UI
│   └── GameManager.cs              # 게임 매니저
├── Prefabs/                        # UI 프리팹
├── Resources/                      # 게임 리소스
└── Scenes/                         # Unity 씬
```

## 🚀 시작하기

### 시스템 요구사항
- Windows 10 이상
- .NET Framework 4.7.1 이상
- 최소 4GB RAM
- 그래픽 카드: DirectX 11 지원

### 설치 및 실행
1. **빌드 파일 다운로드**
   - `ChzzWithViewer.exe` 실행 파일 다운로드
   - 압축 파일을 원하는 위치에 압축 해제

2. **실행**
   - `ChzzWithViewer.exe` 더블클릭으로 실행
   - 첫 실행 시 설정 파일이 자동 생성됩니다

3. **채널 연결**
   - 치지직 채널 ID 입력
   - "연결 시작" 버튼 클릭

## 🎮 사용법

### 룰렛 게임 진행
1. **채널 연결**: 치지직 스트리밍 채널 ID 입력 후 연결
2. **룰렛 항목 추가**: 수동으로 항목과 가중치 입력
3. **도네이션 연동**: 도네이션 시 자동으로 해당 항목 가중치 증가
	- 인식 조건 : 도네이션 메시지 내 [룰렛] 포함, 원하는 항목 명 "(큰따옴표)로 감싸기
	- EX)
		- [룰렛] "홍어" 이거 ㅋ        : 인식 홍어
		- [룰렛]  러시아 ㄱ "탈콥"     : 인식 탈콥
		- "1" 111111111111111  [룰렛] : 인식 1
4. **룰렛 실행**: "룰렛 돌리기" 버튼으로 결과 확인

### 설정 관리
- **금지어 설정**: 특정 단어를 필터링하여 룰렛에 추가되지 않도록 설정
- **전체 화면 설정**: ALT + Enter로 전환

### 채팅 명령어
- `"항목명"`: 해당 항목을 룰렛에 추가 (가중치 1)
- 도네이션 + `"항목명"`: 도네이션 금액에 비례하여 가중치 증가

## 🔧 기술 스택

### 외부 라이브러리
- **WebSocketSharp**: WebSocket 통신
- **Newtonsoft.Json**: JSON 직렬화/역직렬화
- **NuGet for Unity**: 패키지 관리

### API 연동
- **치지직 API**: 채널 정보, 라이브 상태 조회
- **치지직 채팅 API**: 실시간 채팅, 도네이션 수신

## 📁 파일 구조

### 설정 파일
- `ChzzWithViewer.txt`: 게임 설정 저장 파일 (JSON 형식)
  ```json
  {
    "bannedWords": ["금지어1", "금지어2"],
    "defaultBannedWords": ["기본금지어"],
    "enableDevChat": false,
  }
  ```

### 빌드 파일
- `ChzzWithViewer.exe`: 메인 실행 파일
- `ChzzWithViewer_Data/`: 게임 데이터 폴더


## 🛠️ 개발 환경 설정

### 필요한 패키지
- Universal RP
- TextMesh Pro
- Input System
- NuGet for Unity
- ChzzkUnity 비공식 API : https://github.com/JoKangHyeon/ChzzkUnity

## 🐛 문제 해결

### 설정 파일 문제
- `ChzzWithViewer.txt` 파일 삭제 후 재실행
- 관리자 권한으로 실행

---

**주의사항**: 이 애플리케이션은 치지직의 공식 제품이 아닙니다. 

