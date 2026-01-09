import subprocess

if  __name__ =="__main__":

    subprocess.run(["python", "Build_Db.py"])
    subprocess.run(["python", "build_pb.py"])
    exit()

