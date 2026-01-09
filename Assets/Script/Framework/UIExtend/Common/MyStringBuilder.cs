public class MyStringBuilder
{
    private char[] mChars;

    private int mCount;

    /// <summary>
    /// 容积
    /// </summary>
    private int _mCapacity;

    public int Length
    {
        get { return mCount; }
        set
        {
            _addLength(value - mCount);
            mCount = value;
        }
    }

    /// <summary>
    /// 构造函数capacity 容积
    /// </summary>
    /// <param name="capacity"></param>
    public MyStringBuilder(int capacity)
    {
        mCount = 0;
        _mCapacity = capacity;
        mChars = new char[_mCapacity];
    }

    public void Append(string strAppend)
    {
        int addLength = strAppend.Length;
        _addLength(addLength);
        for (int i = 0; i < addLength; i++)
            mChars[mCount++] = strAppend[i];
    }

    public void Append(char charAppend)
    {
        _addLength(1);
        mChars[mCount++] = charAppend;
    }

    public void Append(string strAppend, int startIndex, int count)
    {
        int iEnd = startIndex + count;
        if (strAppend.Length <= iEnd)
        {
            iEnd = strAppend.Length;
            count = iEnd - startIndex;
        }
        _addLength(count);
        for (int i = startIndex; i < iEnd; i++)
            mChars[mCount++] = strAppend[i];
    }

    public void Append(string strAppend, int startIndex)
    {
        int iEnd = strAppend.Length;
        int count = iEnd - startIndex;
        _addLength(count);
        for (int i = startIndex; i < iEnd; i++)
            mChars[mCount++] = strAppend[i];
    }

    public void Append(int appendValue)
    {
        Append((long)appendValue);
    }

    public void Append(long appendValue)
    {
        _addLength(20);
        if (appendValue < 0)
        {
            mChars[mCount++] = '-';
            appendValue = -appendValue;
        }
        else if (appendValue == 0)
        {
            mChars[mCount++] = '0';
            return;
        }
        for (int i = 0; i < 20; i++)
        {
            mIntBuffer[i] = mIntToChar[appendValue % 10];
            appendValue /= 10;
            if (appendValue == 0)
            {
                for (int j = i; j >= 0; j--)
                {
                    mChars[mCount++] = mIntBuffer[j];
                }
                return;
            }
        }
    }

    private static char[] mIntBuffer = new char[20];
    private static readonly char[] mIntToChar = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    public override string ToString()
    {
        return new string(mChars, 0, mCount);
    }

    private void _addLength(int addLength)
    {
        int afterAddLength = mCount + addLength;
        if (afterAddLength > _mCapacity)
        {
            _mCapacity *= (afterAddLength / _mCapacity + 1);
            char[] afterAddChars = new char[_mCapacity];
            for (int i = 0; i < mCount; i++)
                afterAddChars[i] = mChars[i];
            mChars = afterAddChars;
        }
    }
}