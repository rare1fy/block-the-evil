using System.Collections.Generic;
using UnityEngine;

namespace AntNet
{
    /// <summary>
    ///	内存块
    /// </summary>
    public class NetBuffer
    {
        /// <summary>
        /// 存储的数据.
        /// </summary>
        public byte[] data;

        /// <summary>
        ///	数据长度
        /// </summary>
        public int Size;

        /// <summary>
        /// 尺寸模式
        /// </summary>
        Mode NetBuffMode;

        public void Copy(byte[] msg, int desOffset, int count = -1)
        {
            System.Buffer.BlockCopy(msg, 0, data, desOffset, count == -1 ? msg.Length : count);
        }

        NetBuffer(Mode m, int size)
        {
            switch (m)
            {
                case Mode.Small:
                    data = new byte[SMALL_SIZE];
                    break;
                case Mode.Normal:
                    data = new byte[NORMAL_SIZE];
                    break;
                case Mode.Big:
                    data = new byte[BIG_SIZE];
                    break;
                case Mode.Big_one:
                    data = new byte[BIG_SIZE_ONE];
                    break;
                case Mode.Big_two:
                    data = new byte[BIG_SIZE_TWO];
                    break;
                case Mode.Big_three:
                    data = new byte[BIG_SIZE_THREE];
                    break;
                case Mode.Big_fore:
                    data = new byte[BIG_SIZE_FORE];
                    break;
                case Mode.Big_five:
                    data = new byte[BIG_SIZE_FIVE];
                    break;
                case Mode.Big_six:
                    data = new byte[BIG_SIZE_SIX];
                    break;
                case Mode.Max:
                    data = new byte[MAX_SIZE];
                    break;
            }
            Size = size;
            NetBuffMode = m;
        }

        /// <summary>
        /// 获取内存块
        /// </summary>
        public static NetBuffer Alloc(int size)
        {
            if (size > MAX_SIZE)
            {
                Debug.LogError("------------------------------------------data size too looger length------------------------" + size);
            }
            NetBuffer find = null;
            if (size <= SMALL_SIZE)
            {
                find = normal.Count > 0 ? normal.Pop() : new NetBuffer(Mode.Small, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Small, size);
            }
            else if (size <= NORMAL_SIZE)
            {
                find = normal.Count > 0 ? normal.Pop() : new NetBuffer(Mode.Normal, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Normal, size);
            }
            else if (size <= BIG_SIZE)
            {
                find = big.Count > 0 ? big.Pop() : new NetBuffer(Mode.Big, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big, size);
            }
            else if (size <= BIG_SIZE_ONE)
            {
                find = big_one.Count > 0 ? big_one.Pop() : new NetBuffer(Mode.Big_one, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_one, size);
            }
            else if (size <= BIG_SIZE_TWO)
            {
                find = big_two.Count > 0 ? big_two.Pop() : new NetBuffer(Mode.Big_two, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_two, size);
            }
            else if (size <= BIG_SIZE_THREE)
            {
                find = big_three.Count > 0 ? big_three.Pop() : new NetBuffer(Mode.Big_three, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_three, size);
            }
            else if (size <= BIG_SIZE_FORE)
            {
                find = big_fore.Count > 0 ? big_fore.Pop() : new NetBuffer(Mode.Big_fore, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_fore, size);
            }
            else if (size <= BIG_SIZE_FIVE)
            {
                find = big_five.Count > 0 ? big_five.Pop() : new NetBuffer(Mode.Big_five, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_five, size);
            }
            else if (size <= BIG_SIZE_SIX)
            {
                find = big_six.Count > 0 ? big_six.Pop() : new NetBuffer(Mode.Big_six, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Big_six, size);
            }
            else
            {
                find = max.Count > 0 ? max.Pop() : new NetBuffer(Mode.Max, size);
                if (ReferenceEquals(find, null))
                    find = new NetBuffer(Mode.Max, size);
            }
            find.Size = size;
            return find;
        }

        static readonly int SMALL_SIZE = 32;
        static readonly int NORMAL_SIZE = 64;
        static readonly int BIG_SIZE = 128;
        static readonly int BIG_SIZE_ONE = 256;
        static readonly int BIG_SIZE_TWO = 512;
        static readonly int BIG_SIZE_THREE = 1024;
        static readonly int BIG_SIZE_FORE = 1024 * 2;
        static readonly int BIG_SIZE_FIVE = 1024 * 3;
        static readonly int BIG_SIZE_SIX = 1024 * 4;
        static readonly int MAX_SIZE = 1024 * 8;


        static readonly Stack<NetBuffer> small = new();
        static readonly Stack<NetBuffer> normal = new();
        static readonly Stack<NetBuffer> big = new();
        static readonly Stack<NetBuffer> big_one = new();
        static readonly Stack<NetBuffer> big_two = new();
        static readonly Stack<NetBuffer> big_three = new();
        static readonly Stack<NetBuffer> big_fore = new();
        static readonly Stack<NetBuffer> big_five = new();
        static readonly Stack<NetBuffer> big_six = new();
        static readonly Stack<NetBuffer> max = new();

        /// <summary>
        ///	释放一个块以便以后重用它
        /// </summary>
        /// <param name="b">XBuffer instance.</param>
        public static void Free(NetBuffer b)
        {
            if (b != null)
            {
                switch (b.NetBuffMode)
                {
                    case Mode.Small: small.Push(b); break;
                    case Mode.Normal: normal.Push(b); break;
                    case Mode.Big: big.Push(b); break;
                    case Mode.Big_one: big_one.Push(b); break;
                    case Mode.Big_two: big_two.Push(b); break;
                    case Mode.Big_three: big_three.Push(b); break;
                    case Mode.Big_fore: big_fore.Push(b); break;
                    case Mode.Big_five: big_five.Push(b); break;
                    case Mode.Big_six: big_six.Push(b); break;
                    case Mode.Max: max.Push(b); break;
                }
            }
        }

        enum Mode
        {
            Small,
            Normal,
            Big,
            Big_one,
            Big_two,
            Big_three,
            Big_fore,
            Big_five,
            Big_six,
            Max,
        }

        public static void Dispose()
        {
            small.Clear();
            normal.Clear();
            big.Clear();
            big_one.Clear();
            big_two.Clear();
            big_three.Clear();
            big_fore.Clear();
            big_five.Clear();
            big_six.Clear();
            max.Clear();
        }
    }
}
