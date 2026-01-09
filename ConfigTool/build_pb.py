import shutil
import sys
import os

pb_path = os.path.join(sys.path[0], 'protobuf')
output_path = os.path.join(sys.path[0], 'protobuf/cs')
pb_exe_path = os.path.join(sys.path[0], 'protoc/bin/protoc.exe')
csPath = os.path.join(sys.path[0],"../Assets/Script/ProtoMessage") 
rootPath = os.path.dirname(os.path.abspath(__file__))

'''
读取文件
'''
def read_file():
    _file_path = os.path.join(pb_path, 'cmd.txt')
    _file = open(_file_path, 'r',encoding='utf-8') 
    lines = _file.readlines()
    _file.close()
    return [line.strip() for line in lines]

def write_file(file_path, content):
    _file_path = os.path.join(sys.path[0], file_path)
    _str_01 = "using System;\nusing System.Collections.Generic;\n\n\n//协议ID枚举\nenum MESSAGE_ID\n{\n"

    _str_02 = "//协议类型，解析协议用\npublic static class MessageDef\n{\n\tpublic static Dictionary<int, Type> MessageMap = new Dictionary<int, Type>()\n\t{\n"

    for line in content:
        if line == '':
            continue
        _s = line.split('#')
        #print(_s)
        _str_01 += '\t' + '//'+ _s[len(_s)-1] + '\n\t' + _s[1] + ' = ' + _s[0] + ',\n'
        _str_02 += '\t\t//'+ _s[len(_s)-1] + '\n'+'\t\t[(int)MESSAGE_ID.'+_s[1]+']'+ " = " + 'Type.GetType(\"'+_s[len(_s)-2]+'\"),\n'

    _str_01 += '}\n\n'
    _str_02 += '\t};\n}'

    _str = _str_01 + _str_02
    #print(_str)

    _file = open(_file_path, 'w',encoding='utf-8')  
    _file.write(_str)
    _file.close()

def copy_pb_file():
    files = os.listdir(output_path)
    for v in files:
        if v.endswith(".cs"):
            shutil.copy(os.path.join(output_path,v),csPath)

'''
生成pb文件
'''
def build_pb():
    os.system('%s -I=%s --csharp_out=%s %s/*.proto'%(pb_exe_path,pb_path,output_path,pb_path))


if __name__ == '__main__':
    if(not os.path.exists(output_path)):
        os.mkdir(output_path)
    if(not os.path.exists(csPath)):
        os.mkdir(csPath)
    _v = read_file()
    write_file(os.path.join(output_path,'MessageDef.cs'), _v)
    build_pb()
    copy_pb_file()
    print("---------------------------------------协议生成完毕----------------------------------------------------------------------------")
