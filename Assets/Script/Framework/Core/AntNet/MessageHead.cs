namespace AntNet
{
    /// <summary>
    /// 消息协议头
    /// </summary>
    public class MessageHead
    {
        private static ObjectPoolEx<MessageHead> recvDatas = new (null, data => { data.Clear(); });

        public static MessageHead getHeadData()
        {
            return recvDatas.Get();
        }

        public static MessageHead getHeadData(MessageHead head)
        {
            var data= recvDatas.Get();
            data.Clone(head);
            return data;
        }

        public static void Free(MessageHead head)
        {
            recvDatas.Release(head);
        }

        public class Flags
        {
            public static ushort FlagEncrypt = 1 << 0;          //数据是经过加密的
            public static ushort FlagCompress = 1 << 1;          //数据是经过压缩的
            public static ushort FlagContinue = 1 << 2;          //有后续数据
            public static ushort FlagNeedAck = 1 << 3;           //数据需要确认
            public static ushort FlagAck = 1 << 4;               //确认包
            public static ushort FlagReSend = 1 << 5;            //数据是重发的
            public static ushort FlagServer = 1 << 6;            //数据来自服务器
        };
        /// <summary>
        /// 数据长度
        /// </summary>
        public uint len = 0;
        /// <summary>
        /// 错误码
        /// </summary>
        public ushort error = 0;
        /// <summary>
        /// 命令
        /// </summary>
        public ushort cmd = 0;
        /// <summary>
        /// 动作
        /// </summary>
        public uint act = 0;
        /// <summary>
        /// 序号
        /// </summary>
        public ushort index = 0;
        /// <summary>
        /// 标记
        /// </summary>
        public ushort flags = 0;

        //超时处理 
        //internal MessageHead nextTimeout = null;

        /// <summary>
        /// 获取协议头长度
        /// </summary>
        public static int Length
        {
            get { return 16; }
        }

        /// <summary>
        /// 最大接收数据长度
        /// </summary>
        public static int MaxRecvLength
        {
            get { return 1024 * 1024; }
        }

        public bool needAck
        {
            get { return (flags & Flags.FlagNeedAck) > 0; }
            set
            {
                if (value)
                {
                    flags |= Flags.FlagNeedAck;
                }
                else if (needAck)
                {
                    flags -= Flags.FlagNeedAck;
                }
            }
        }

        /// <summary>
        /// 命令和活动联合key
        /// </summary>
        public int CmdAct
        {
            get
            {
                int _cmd = EncryptUtil.UintToInt(cmd);
                int _act = EncryptUtil.UintToInt(act);
                return _cmd * 1024 + _act;
            }
        }

        /// <summary>
        /// 解析协议头
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>解析好的协议头</returns>
        public static MessageHead Parse(byte[] data)
        {
            MessageHead head = new MessageHead();
            unsafe
            {
                fixed (byte* pb = data)
                {
                    head.len = *((uint*)pb);
                    head.error = *((ushort*)(pb + 4));
                    head.cmd = *((ushort*)(pb + 6));
                    head.act = *((uint*)(pb + 8));
                    head.index = *((ushort*)(pb + 12));
                    head.flags = *((ushort*)(pb + 14));
                }
            }
            return head;
        }

        public void Serialize(NetBuffer netBuffer)
        {
            unsafe
            {
                fixed (byte* pb = netBuffer.data)
                {
                    *((uint*)pb) = len;
                    *((ushort*)(pb + 4)) = error;
                    *((ushort*)(pb + 6)) = cmd;
                    *((uint*)(pb + 8)) = act;
                    *((ushort*)(pb + 12)) = index;
                    *((ushort*)(pb + 14)) = flags;
                }
            }
        }

        private void Clone(MessageHead head)
        {
            len = head.len;
            error = head.error;
            cmd = head.cmd;
            act = head.act;
            index = head.index;
            flags = head.flags;
        }

        public void Clear()
        {
            len = 0;                //数据长度
            error = 0;            //错误码
            cmd = 0;                //命令
            act = 0;                //动作
            index = 0;            //序号
            flags = 0;            //标记
        }
    }
}













