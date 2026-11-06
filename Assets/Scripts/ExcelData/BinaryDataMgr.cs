using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

/// <summary>
/// 2进制数据管理器
/// </summary>
public class BinaryDataMgr
{
    /// <summary>
    /// 2进制数据存储位置路径
    /// </summary>
    public static string DATA_BINARY_PATH = Application.streamingAssetsPath + "/Binary/";

    /// <summary>
    /// 用于存储所有Excel表数据的容器
    /// </summary>
    private Dictionary<string, object> tableDic = new Dictionary<string, object>();

    /// <summary>
    /// 数据存储的位置
    /// </summary>
    private static string SAVE_PATH = Application.persistentDataPath + "/Data/";

    private static BinaryDataMgr instance = new BinaryDataMgr();
    public static BinaryDataMgr Instance => instance;

    private BinaryDataMgr()
    {
        InitData();
    }

    public void InitData()
    {
        
    }

    /// <summary>
    /// 加载Excel表的2进制数据到内存中 
    /// </summary>
    /// <typeparam name="T">容器类名</typeparam>
    /// <typeparam name="K">数据结构类类名</typeparam>
    public void LoadTable<T,K>()
    {
        // 幂等检查：已加载过同类型容器则跳过，避免重复反射和抛重复 key 异常
        string tableKey = typeof(T).Name;
        if (tableDic.ContainsKey(tableKey))
        {
            Debug.Log($"[BinaryDataMgr] {tableKey} 已加载，跳过重复加载");
            return;
        }

        //读取 excel表对应的2进制文件 来进行解析
        using (FileStream fs = File.Open(DATA_BINARY_PATH + typeof(K).Name + ".Bo", FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            //用于记录当前读取了多少字节了
            int index = 0;

            //读取多少行数据
            int count = BitConverter.ToInt32(bytes, index);
            index += 4;

            //读取主键的名字
            int keyNameLength = BitConverter.ToInt32(bytes, index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(bytes, index, keyNameLength);
            index += keyNameLength;

            //创建容器类对象
            Type contaninerType = typeof(T);
            object contaninerObj = Activator.CreateInstance(contaninerType);
            //得到数据结构类的Type
            Type classType = typeof(K);
            //通过反射 得到数据结构类 所有字段的信息
            FieldInfo[] infos = classType.GetFields();

            //读取每一行的信息
            for (int i = 0; i < count; i++)
            {
                //实例化一个数据结构类 对象
                object dataObj = Activator.CreateInstance(classType);
                foreach (FieldInfo info in infos)
                {
                    if( info.FieldType == typeof(int) )
                    {
                        //相当于就是把2进制数据转为int 然后赋值给了对应的字段
                        info.SetValue(dataObj, BitConverter.ToInt32(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(float))
                    {
                        info.SetValue(dataObj, BitConverter.ToSingle(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(bool))
                    {
                        info.SetValue(dataObj, BitConverter.ToBoolean(bytes, index));
                        index += 1;
                    }
                    else if (info.FieldType == typeof(string))
                    {
                        //读取字符串字节数组的长度
                        int length = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        info.SetValue(dataObj, Encoding.UTF8.GetString(bytes, index, length));
                        index += length;
                    }
                    else if (info.FieldType == typeof(long))
                    {
                        info.SetValue(dataObj, BitConverter.ToInt64(bytes, index));
                        index += 8;
                    }
                    else if (info.FieldType == typeof(double))
                    {
                        info.SetValue(dataObj, BitConverter.ToDouble(bytes, index));
                        index += 8;
                    }
                    else if (info.FieldType == typeof(Vector2))
                    {
                        float x = BitConverter.ToSingle(bytes, index); index += 4;
                        float y = BitConverter.ToSingle(bytes, index); index += 4;
                        info.SetValue(dataObj, new Vector2(x, y));
                    }
                    else if (info.FieldType == typeof(Vector3))
                    {
                        float x = BitConverter.ToSingle(bytes, index); index += 4;
                        float y = BitConverter.ToSingle(bytes, index); index += 4;
                        float z = BitConverter.ToSingle(bytes, index); index += 4;
                        info.SetValue(dataObj, new Vector3(x, y, z));
                    }
                    else if (info.FieldType == typeof(Vector4))
                    {
                        float x = BitConverter.ToSingle(bytes, index); index += 4;
                        float y = BitConverter.ToSingle(bytes, index); index += 4;
                        float z = BitConverter.ToSingle(bytes, index); index += 4;
                        float w = BitConverter.ToSingle(bytes, index); index += 4;
                        info.SetValue(dataObj, new Vector4(x, y, z, w));
                    }
                    else if (info.FieldType == typeof(Color))
                    {
                        float r = BitConverter.ToSingle(bytes, index); index += 4;
                        float g = BitConverter.ToSingle(bytes, index); index += 4;
                        float b = BitConverter.ToSingle(bytes, index); index += 4;
                        float a = BitConverter.ToSingle(bytes, index); index += 4;
                        info.SetValue(dataObj, new Color(r, g, b, a));
                    }
                    else
                    {
                        Debug.LogError($"[BinaryDataMgr] 不支持的字段类型: {info.FieldType} (字段名: {info.Name})");
                    }
                }

                //读取完一行的数据了 应该把这个数据添加到容器对象中
                //得到容器对象中的 字典对象
                object dicObject = contaninerType.GetField("dataDic").GetValue(contaninerObj);
                //通过字典对象得到其中的 Add方法
                MethodInfo mInfo = dicObject.GetType().GetMethod("Add");
                //得到数据结构类对象中 指定主键字段的值
                object keyValue = classType.GetField(keyName).GetValue(dataObj);
                mInfo.Invoke(dicObject, new object[] { keyValue, dataObj });
            }

            //把读取完的表记录下来
            tableDic.Add(typeof(T).Name, contaninerObj);

            fs.Close();
        }
    }

    /// <summary>
    /// 得到一张表的信息
    /// </summary>
    /// <typeparam name="T">容器类名</typeparam>
    /// <returns></returns>
    public T GetTable<T>() where T:class
    {
        string tableName = typeof(T).Name;
        if (tableDic.ContainsKey(tableName))
            return tableDic[tableName] as T;
        return null;
    }

    /// <summary>
    /// 存储类对象数据
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fileName"></param>
    [System.Obsolete("BinaryFormatter 已过时且存在安全风险，运行时存档请改用JSON方案。此方法仅用于兼容旧代码。")]
    public void Save(object obj, string fileName)
    {
        Debug.LogWarning("[BinaryDataMgr] Save方法使用了已过时的BinaryFormatter，存在反序列化安全风险，建议改用JSON存档。");
        //先判断路径文件夹有没有
        if (!Directory.Exists(SAVE_PATH))
            Directory.CreateDirectory(SAVE_PATH);

        using (FileStream fs = new FileStream(SAVE_PATH + fileName + ".Bo", FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);
            fs.Close();
        }
    }

    /// <summary>
    /// 读取2进制数据转换成对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [System.Obsolete("BinaryFormatter 已过时且存在安全风险，运行时存档请改用JSON方案。此方法仅用于兼容旧代码。")]
    public T Load<T>(string fileName) where T:class
    {
        Debug.LogWarning("[BinaryDataMgr] Load方法使用了已过时的BinaryFormatter，存在反序列化安全风险，建议改用JSON存档。");
        //如果不存在这个文件 就直接返回泛型对象的默认值
        if( !File.Exists(SAVE_PATH + fileName + ".Bo") )
            return default(T);

        T obj;
        using (FileStream fs = File.Open(SAVE_PATH + fileName + ".Bo", FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs) as T;
            fs.Close();
        }

        return obj;
    }
}
