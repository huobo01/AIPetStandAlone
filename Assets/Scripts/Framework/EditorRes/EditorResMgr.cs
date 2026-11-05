using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 编辑器资源管理器（仅开发时可用）
/// 直接通过 AssetDatabase 从工程目录加载资源，用于在不打AB包的情况下调试功能
/// 发布后该类无法使用（依赖 UnityEditor）
/// </summary>
public class EditorResMgr : BaseManager<EditorResMgr>
{
    //需要打包进AB包的资源根路径
    private string rootPath = "Assets/Editor/ArtRes/";

    private EditorResMgr() { }

    /// <summary>
    /// 加载单个资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">相对rootPath的路径（不含后缀）</param>
    public T LoadEditorRes<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        string suffixName = "";
        //根据类型推断文件后缀
        if (typeof(T) == typeof(GameObject))
            suffixName = ".prefab";
        else if (typeof(T) == typeof(Material))
            suffixName = ".mat";
        else if (typeof(T) == typeof(Texture))
            suffixName = ".png";
        else if (typeof(T) == typeof(AudioClip))
            suffixName = ".mp3";
        T res = AssetDatabase.LoadAssetAtPath<T>(rootPath + path + suffixName);
        return res;
#else
        return null;
#endif
    }

    /// <summary>
    /// 加载图集中的指定子图片
    /// </summary>
    public Sprite LoadSprite(string path, string spriteName)
    {
#if UNITY_EDITOR
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            if (spriteName == item.name)
                return item as Sprite;
        }
        return null;
#else
        return null;
#endif
    }

    /// <summary>
    /// 加载图集文件中的所有子图片
    /// </summary>
    public Dictionary<string, Sprite> LoadSprites(string path)
    {
#if UNITY_EDITOR
        Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            spriteDic.Add(item.name, item as Sprite);
        }
        return spriteDic;
#else
        return null;
#endif
    }
}
