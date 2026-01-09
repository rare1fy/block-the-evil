#python2没有下面Options:那一块
"""
Usage:
  FontPruner.py --inputPath=<inputPaths>... --inputFont=<inputFont>... [--tempPath=<tempPath>]
Options:
  --inputPath 路径列表
  --inputFont 字体文件列表
  --tempPath 临时目录 [default: tmp]
"""
from docopt import docopt
import os

SepPath = os.path.sep
TempPathDefault = "tmp"
InputFilelist = "input_filelist.txt"
IntermediateFolder = "intermediate"
OutputFolder = "output"
ChineseOutPut = "ChineseOutPut.txt"
UnChineseOutPut = "unChineseOutPut.txt"
Succ = 0
def genFilePathList(inputPath,FileListOP):    
  fullPara = ""
  for path in inputPath:
    fullPara +=path+" "
  fullPara += " "+FileListOP + SepPath+InputFilelist    
  command ="java -jar bin"+SepPath+"GenFileList.jar " +fullPara
  if os.system(command) is not Succ:
    raise Exception('generate fileList.txt error!'+command)


def extractFileString(temp):
  fileListPath = temp+SepPath+InputFilelist
  outputPath = temp+SepPath+IntermediateFolder

  command ="java -jar bin"+SepPath+"fontExtract.jar " +fileListPath + " "+outputPath
  if os.system(command) is not Succ:
    raise Exception('extract font string  error!'+command)


def bulidNewFont(originPath,outPutPath):
    fullOutPut = outPutPath+SepPath+OutputFolder
    if not os.path.exists(fullOutPut):
      os.makedirs(fullOutPut)
    for fontOrigin in originPath:
      index = len(fontOrigin) - fontOrigin.rfind(SepPath)
      fontName = fontOrigin[-index:]
      fullPara = ""
      fullPara += outPutPath+SepPath+IntermediateFolder+SepPath+ChineseOutPut+"  "+outPutPath+SepPath+IntermediateFolder+SepPath+UnChineseOutPut+" "+fontOrigin + " " + fullOutPut+SepPath+fontName 
      command ="java -jar bin"+SepPath+"sfnttool.jar -c " +fullPara
      
      if os.system(command) is not Succ:
        raise Exception('build new font error!'+command)


if __name__ == '__main__':
  arguments = docopt(__doc__, version='0.1.1rc')

  for path in arguments['--inputPath']:
    if not os.path.exists(path):
      raise Exception("inputPath - bad path: " + path)

  for path in arguments['--inputFont']:
      if not os.path.exists(path):
         raise Exception("inputFont - bad path: " + path)

  tmp = arguments['--tempPath']
  if tmp is None:
    tmp = TempPathDefault

  if not os.path.exists(tmp):
    os.makedirs(tmp)

  genFilePathList(arguments['--inputPath'], tmp) 

  extractFileString(tmp)
  
  bulidNewFont(arguments['--inputFont'], tmp)
  print("-----------------------------------------Success-------------------------------------------------------")
  
           
    
