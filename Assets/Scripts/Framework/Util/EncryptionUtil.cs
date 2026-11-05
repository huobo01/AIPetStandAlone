using UnityEngine;

/// <summary>
/// 加密工具类
/// 基于异或运算的简单数值加密，用于防作弊（非安全加密）
/// </summary>
public class EncryptionUtil
{
    /// <summary>获取随机密钥</summary>
    public static int GetRandomKey()
    {
        return Random.Range(1, 10000) + 5;
    }

    /// <summary>加密int值</summary>
    public static int LockValue(int value, int key)
    {
        value = value ^ (key % 9);
        value = value ^ 0xADAD;
        value = value ^ (1 << 5);
        value += key;
        return value;
    }

    /// <summary>加密long值</summary>
    public static long LockValue(long value, int key)
    {
        value = value ^ (key % 9);
        value = value ^ 0xADAD;
        value = value ^ (1 << 5);
        value += key;
        return value;
    }

    /// <summary>解密int值</summary>
    public static int UnlockValue(int value, int key)
    {
        //未加密过的值（0）直接返回
        if (value == 0)
            return value;
        value -= key;
        value = value ^ (key % 9);
        value = value ^ 0xADAD;
        value = value ^ (1 << 5);
        return value;
    }

    /// <summary>解密long值</summary>
    public static long UnlockValue(long value, int key)
    {
        if (value == 0)
            return value;
        value -= key;
        value = value ^ (key % 9);
        value = value ^ 0xADAD;
        value = value ^ (1 << 5);
        return value;
    }
}
