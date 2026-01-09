using System;
using System.Collections.Generic;

namespace AntNet
{
    /// <summary>
    /// 接收到的数据
    /// </summary>
    public class RecvData
    {
        public byte[] data = null;                      //数据
        public MessageHead head = null;                 //协议头

        public RecvData()
        {
            head = MessageHead.getHeadData();
        }
        
        public RecvData(MessageHead head)
        {
            this.head = head;
        }

        public RecvData(MessageHead head, byte[] data)
        {
            this.head = head;
            this.data = data;
        }
    }
}