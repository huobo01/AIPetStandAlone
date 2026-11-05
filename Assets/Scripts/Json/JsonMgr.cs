using Newtonsoft.Json;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Json序列化方案枚举
/// </summary>
public enum JsonType
{
    /// <summary>Newtonsoft.Json，功能完整支持Dictionary/多态/JsonProperty，主力方案</summary>
    Newtonsoft,
    /// <summary>Unity自带JsonUtility，性能最好但不支持Dictionary、字段必须加[Serializable]</summary>
    JsonUtility,
}

/// <summary>
/// Json持久化管理器
/// 负责将对象序列化存储到硬盘，以及从硬盘反序列化读取到内存
/// 存储优先级：persistentDataPath（可读写） > streamingAssetsPath（只读默认数据）
/// </summary>
public class JsonMgr
{
    private static JsonMgr instance = new JsonMgr();
    public static JsonMgr Instance => instance;

    private JsonMgr() { }

    /// <summary>
    /// 默认序列化设置：缩进格式便于阅读，忽略循环引用，忽略空值
    /// </summary>
    private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
    };

    /// <summary>
    /// 存储Json数据到硬盘（persistentDataPath目录）
    /// </summary>
    /// <param name="data">要序列化的数据对象</param>
    /// <param name="fileName">文件名（不含后缀）</param>
    /// <param name="type">序列化方案，默认Newtonsoft</param>
    public void SaveData(object data, string fileName, JsonType type = JsonType.Newtonsoft)
    {
        //确定存储路径（persistentDataPath为可读写目录）
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        //序列化得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtility:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.Newtonsoft:
                jsonStr = JsonConvert.SerializeObject(data, DefaultSettings);
                break;
        }
        //以UTF-8无BOM编码写入，避免中文乱码
        File.WriteAllText(path, jsonStr, new UTF8Encoding(false));
    }

    /// <summary>
    /// 从硬盘读取Json数据并反序列化
    /// 优先从streamingAssetsPath（默认配置数据）读取，若不存在则从persistentDataPath读取
    /// </summary>
    /// <typeparam name="T">目标数据类型，必须有无参构造函数</typeparam>
    /// <param name="fileName">文件名（不含后缀）</param>
    /// <param name="type">反序列化方案，默认Newtonsoft</param>
    /// <returns>反序列化后的对象，文件不存在时返回默认值</returns>
    public T LoadData<T>(string fileName, JsonType type = JsonType.Newtonsoft) where T : new()
    {
        //首先判断streamingAssetsPath中是否有默认配置数据
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //若默认数据文件夹不存在目标文件，则从persistentDataPath可读写目录寻找
        if (!File.Exists(path))
            path = Application.persistentDataPath + "/" + fileName + ".json";
        //若两个目录都没有该文件，返回默认对象
        if (!File.Exists(path))
            return new T();

        //以UTF-8编码读取，避免中文乱码
        string jsonStr = File.ReadAllText(path, Encoding.UTF8);
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtility:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.Newtonsoft:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
        }
        return data;
    }

    /// <summary>
    /// 将对象序列化为Json字符串（不落盘）
    /// </summary>
    /// <param name="data">要序列化的数据对象</param>
    /// <param name="type">序列化方案，默认Newtonsoft</param>
    /// <returns>Json字符串</returns>
    public string ToJson(object data, JsonType type = JsonType.Newtonsoft)
    {
        switch (type)
        {
            case JsonType.JsonUtility:
                return JsonUtility.ToJson(data);
            case JsonType.Newtonsoft:
                return JsonConvert.SerializeObject(data, DefaultSettings);
        }
        return "";
    }

    /// <summary>
    /// 将Json字符串反序列化为对象
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="jsonStr">Json字符串</param>
    /// <param name="type">反序列化方案，默认Newtonsoft</param>
    /// <returns>反序列化后的对象，失败返回默认值</returns>
    public T FromJson<T>(string jsonStr, JsonType type = JsonType.Newtonsoft)
    {
        try
        {
            switch (type)
            {
                case JsonType.JsonUtility:
                    return JsonUtility.FromJson<T>(jsonStr);
                case JsonType.Newtonsoft:
                    return JsonConvert.DeserializeObject<T>(jsonStr);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JsonMgr] 反序列化失败：{e.Message}");
        }
        return default(T);
    }
}
