@echo off
set CurrentBatPath=%~dp0
cd %CurrentBatPath%

@echo =====Copy ChineseOutPut.txt To Project
xcopy "%CurrentBatPath%\src\FontExtract\ChineseOutPut.txt" "%CurrentBatPath%\intermediate" /Y

@echo =====Copy unChineseOutPut.txt To Project
xcopy "%CurrentBatPath%\src\FontExtract\unChineseOutPut.txt" "%CurrentBatPath%\intermediate" /Y

@echo =====Run Python Script
python FontPruner.py --inputPath=./ --inputFont=./JingNanBoBoHei-Bold-2.TTF --tempPath=./

@echo =====Copy JingNanBoBoHei-Bold-2 To Project
echo f| c:\windows\system32\xcopy "%CurrentBatPath%\output\JingNanBoBoHei-Bold-2.TTF" "%CurrentBatPath%..\..\Assets\Bundles\Common\Font" /Y