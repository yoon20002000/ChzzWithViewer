@echo off
echo ========================================
echo ChzzWithViewer 배포 스크립트
echo ========================================

:: 배포 폴더 생성
set RELEASE_DIR=ChzzWithViewer_Release
if exist %RELEASE_DIR% rmdir /s /q %RELEASE_DIR%
mkdir %RELEASE_DIR%

echo 배포 폴더 생성 완료: %RELEASE_DIR%

:: 필수 파일들 복사
echo 파일 복사 중...

:: 실행 파일
copy "Builds\ChzzWithViewer.exe" "%RELEASE_DIR%\"
copy "Builds\UnityPlayer.dll" "%RELEASE_DIR%\"
copy "Builds\GameAssembly.dll" "%RELEASE_DIR%\"
copy "Builds\baselib.dll" "%RELEASE_DIR%\"
copy "Builds\UnityCrashHandler64.exe" "%RELEASE_DIR%\"

:: 데이터 폴더
xcopy "Builds\ChzzWithViewer_Data" "%RELEASE_DIR%\ChzzWithViewer_Data" /E /I /Y

:: D3D12 폴더
xcopy "Builds\D3D12" "%RELEASE_DIR%\D3D12" /E /I /Y

:: 설정 파일 (있는 경우)
if exist "ChzzWithViewer.txt" copy "ChzzWithViewer.txt" "%RELEASE_DIR%\"

echo 파일 복사 완료!

:: 압축 파일 생성
echo 압축 파일 생성 중...
powershell Compress-Archive -Path "%RELEASE_DIR%" -DestinationPath "ChzzWithViewer_v1.0.zip" -Force

echo ========================================
echo 배포 완료!
echo 생성된 파일: ChzzWithViewer_v1.0.zip
echo ========================================

pause 