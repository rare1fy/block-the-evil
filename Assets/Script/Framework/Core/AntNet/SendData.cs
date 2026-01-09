using UnityEngine;
using System;
using System.IO;
using System.Reflection;

namespace AntNet
{
    /// <summary>
    /// 发送数据
    /// </summary>
    public class SendData
    {
        /// <summary>
        /// 发送的对象的缓存池
        /// </summary>
        private static readonly ObjectPoolEx<SendData> sendDatas = new (null, data => { data.Release(); });
        /// <summary>
        /// 错误码
        /// </summary>
        public ushort error = Error.ErrOk;
        /// <summary>
        /// 消息头 
        /// </summary>
        public MessageHead head = new();
        /// <summary>
        /// 超时时间
        /// </summary>
        internal float endTime = 0;
        /// <summary>
        /// 发送的数据
        /// </summary>
        protected NetBuffer netBuffer = null;
        /// <summary>
        /// 序列化回调
        /// </summary>
        protected Func<byte[]> dataGenCallback = null;
        /// <summary>
        /// 头部时候序列化
        /// </summary>
        protected bool isSerializeHead = false;
        /// <summary>
        /// 发送数据索引
        /// </summary>
        protected static ushort _index = 1;
        /// <summary>
        /// 锁
        /// </summary>
        private static object lockObj = new();

        /// <summary>
        /// 设置发送数据
        /// </summary>
        /// <param name="act"></param> 协议id
        /// <param name="wdata"></param> 协议数据
        /// <param name="timeout"></param> 时间
        /// <param name="flags"></param> 是否封包
        /// <returns></returns>
        public static SendData GetData(int act, byte[] wdata, float timeout = 0, ushort flags = 0) {
            return GetData(Util.MESSAGE_CMD, act, wdata, timeout, flags);
        }

        public static SendData GetData(int cmd, int act, byte[] wdata, float timeout = 0, ushort flags = 0)
        {
            ushort _cmd = IntToUshort(cmd);
            uint _act = IntToUint(act);

            var sendData = sendDatas.Get();
            sendData.Init(_cmd, _act, wdata, timeout, flags);
            return sendData;
        }


        public void Release()
        {
            endTime = 0;
            isSerializeHead = false;
            error = Error.ErrOk;
            head.Clear();
            NetBuffer.Free(netBuffer);
        }

        public static void Free(SendData sendData)
        {
            sendDatas.Release(sendData);
        }

        public static void Dispose()
        {
            sendDatas.Dispose();
        }

        private static ushort dataIndex
        {
            get
            {
                lock (lockObj)
                {
                    if (_index < ushort.MaxValue)
                        return _index++;
                    else
                    {
                        _index = 1;
                        return _index++;
                    }
                }
            }
        }

        /// <summary>
        /// 获取需要发送数据
        /// </summary>
        /// <returns>需要发送的数据</returns>
        public NetBuffer GetData()
        {
            if (dataGenCallback != null)
            {
                byte[] wdata = dataGenCallback();
                int _cmd = UintToInt(head.cmd);
                int _act = UintToInt(head.act);
                wdata = EncryptUtil.Encrypt(_cmd, _act, wdata);
                netBuffer = NetBuffer.Alloc(MessageHead.Length + wdata.Length);
                head.len = (uint)wdata.Length;
                netBuffer.Copy(wdata, MessageHead.Length, wdata.Length);
                dataGenCallback = null;
            }

            if (!isSerializeHead)
            {
                isSerializeHead = true;
                head.Serialize(netBuffer);
            }
            return netBuffer;
        }

        /// <summary>
        /// 发送数据，包括协议头和有效数据
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="act">活动</param>
        /// <param name="wdata">数据</param>
        /// <param name="timeout">超时</param>
        public void Init(ushort cmd, uint act, byte[] wdata, float timeout = 0, ushort flags = 0)
        {
            netBuffer = NetBuffer.Alloc(MessageHead.Length + wdata.Length);
            head.len = (uint)wdata.Length;
            head.cmd = cmd;
            head.act = act;
            head.index = dataIndex;
            head.flags = flags;
            netBuffer.Copy(wdata, MessageHead.Length, wdata.Length);
            if (timeout > 0)
            {
                endTime = Time.realtimeSinceStartup + timeout;
            }
            else
            {
                endTime = -1;
            }
        }

        /// <summary>
        ///  32,64位转int
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int UintToInt(uint o) {
            int i = 0;
            checked { 
                i = (int)o;
            }
            return i;
        }

        /// <summary>
        ///   int 转 Uint 转64位
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static uint IntToUint(int i) {
            uint v = 0;
            checked {
                v = (uint)(i & 0xffff);
            }
            return v;
        }

        /// <summary>
        /// int 转 short 转32位
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static ushort IntToUshort(int i)
        {
            ushort v = 0;
            checked
            {
                v = (ushort)(i & 0xffff);
            }
            return v;
        }
    }
}
