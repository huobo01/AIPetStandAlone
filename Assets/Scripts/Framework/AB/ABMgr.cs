// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;

// /// <summary>
// /// AssetBundle 资源加载管理器
// /// 负责主包、依赖包的加载与卸载，支持泛型/Type/名字三种异步加载方式
// /// </summary>
// public class ABMgr : SingletonAutoMono<ABMgr>
// {
//     //主包
//     private AssetBundle mainAB = null;
//     //主包依赖配置文件
//     private AssetBundleManifest manifest = null;
//     //已加载的AB包字典（AB包不能重复加载，否则报错）
//     private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

//     /// <summary>
//     /// AB包存放路径
//     /// </summary>
//     private string PathUrl => Application.streamingAssetsPath + "/";

//     /// <summary>
//     /// 主包名（根据平台不同）
//     /// </summary>
//     private string MainName
//     {
//         get
//         {
// #if UNITY_IOS
//             return "IOS";
// #elif UNITY_ANDROID
//             return "Android";
// #else
//             return "PC";
// #endif
//         }
//     }

//     /// <summary>
//     /// 加载主包和配置文件（加载其他包前必须先加载主包以获取依赖信息）
//     /// </summary>
//     private void LoadMainAB()
//     {
//         if (mainAB == null)
//         {
//             mainAB = AssetBundle.LoadFromFile(PathUrl + MainName);
//             manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
//         }
//     }

//     /// <summary>
//     /// 加载指定包的所有依赖包
//     /// </summary>
//     private void LoadDependencies(string abName)
//     {
//         LoadMainAB();
//         string[] strs = manifest.GetAllDependencies(abName);
//         for (int i = 0; i < strs.Length; i++)
//         {
//             if (!abDic.ContainsKey(strs[i]))
//             {
//                 AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + strs[i]);
//                 abDic.Add(strs[i], ab);
//             }
//         }
//     }

//     /// <summary>
//     /// 泛型异步加载资源
//     /// </summary>
//     /// <typeparam name="T">资源类型</typeparam>
//     /// <param name="abName">AB包名</param>
//     /// <param name="resName">资源名</param>
//     /// <param name="callBack">加载完成回调</param>
//     /// <param name="isSync">是否同步加载AB包（资源仍为异步加载）</param>
//     public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync = false) where T : Object
//     {
//         StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callBack, isSync));
//     }

//     private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync) where T : Object
//     {
//         //加载主包
//         LoadMainAB();
//         //获取并加载依赖包
//         string[] strs = manifest.GetAllDependencies(abName);
//         for (int i = 0; i < strs.Length; i++)
//         {
//             if (!abDic.ContainsKey(strs[i]))
//             {
//                 if (isSync)
//                 {
//                     AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + strs[i]);
//                     abDic.Add(strs[i], ab);
//                 }
//                 else
//                 {
//                     //先存入null标记"正在加载"，加载完成后替换
//                     abDic.Add(strs[i], null);
//                     AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + strs[i]);
//                     yield return req;
//                     abDic[strs[i]] = req.assetBundle;
//                 }
//             }
//             else
//             {
//                 //如果字典值为null，说明正在异步加载中，等待加载完成
//                 while (abDic[strs[i]] == null)
//                     yield return 0;
//             }
//         }

//         //加载目标包
//         if (!abDic.ContainsKey(abName))
//         {
//             if (isSync)
//             {
//                 AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + abName);
//                 abDic.Add(abName, ab);
//             }
//             else
//             {
//                 abDic.Add(abName, null);
//                 AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + abName);
//                 yield return req;
//                 abDic[abName] = req.assetBundle;
//             }
//         }
//         else
//         {
//             while (abDic[abName] == null)
//                 yield return 0;
//         }

//         //加载包中资源
//         if (isSync)
//         {
//             T res = abDic[abName].LoadAsset<T>(resName);
//             callBack(res);
//         }
//         else
//         {
//             AssetBundleRequest abq = abDic[abName].LoadAssetAsync<T>(resName);
//             yield return abq;
//             callBack(abq.asset as T);
//         }
//     }

//     /// <summary>
//     /// Type异步加载资源
//     /// </summary>
//     public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync = false)
//     {
//         StartCoroutine(ReallyLoadResAsync(abName, resName, type, callBack, isSync));
//     }

//     private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync)
//     {
//         LoadMainAB();
//         string[] strs = manifest.GetAllDependencies(abName);
//         for (int i = 0; i < strs.Length; i++)
//         {
//             if (!abDic.ContainsKey(strs[i]))
//             {
//                 if (isSync)
//                 {
//                     AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + strs[i]);
//                     abDic.Add(strs[i], ab);
//                 }
//                 else
//                 {
//                     abDic.Add(strs[i], null);
//                     AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + strs[i]);
//                     yield return req;
//                     abDic[strs[i]] = req.assetBundle;
//                 }
//             }
//             else
//             {
//                 while (abDic[strs[i]] == null)
//                     yield return 0;
//             }
//         }

//         if (!abDic.ContainsKey(abName))
//         {
//             if (isSync)
//             {
//                 AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + abName);
//                 abDic.Add(abName, ab);
//             }
//             else
//             {
//                 abDic.Add(abName, null);
//                 AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + abName);
//                 yield return req;
//                 abDic[abName] = req.assetBundle;
//             }
//         }
//         else
//         {
//             while (abDic[abName] == null)
//                 yield return 0;
//         }

//         if (isSync)
//         {
//             Object res = abDic[abName].LoadAsset(resName, type);
//             callBack(res);
//         }
//         else
//         {
//             AssetBundleRequest abq = abDic[abName].LoadAssetAsync(resName, type);
//             yield return abq;
//             callBack(abq.asset);
//         }
//     }

//     /// <summary>
//     /// 按名字异步加载资源
//     /// </summary>
//     public void LoadResAsync(string abName, string resName, UnityAction<Object> callBack, bool isSync = false)
//     {
//         StartCoroutine(ReallyLoadResAsync(abName, resName, callBack, isSync));
//     }

//     private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<Object> callBack, bool isSync)
//     {
//         LoadMainAB();
//         string[] strs = manifest.GetAllDependencies(abName);
//         for (int i = 0; i < strs.Length; i++)
//         {
//             if (!abDic.ContainsKey(strs[i]))
//             {
//                 if (isSync)
//                 {
//                     AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + strs[i]);
//                     abDic.Add(strs[i], ab);
//                 }
//                 else
//                 {
//                     abDic.Add(strs[i], null);
//                     AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + strs[i]);
//                     yield return req;
//                     abDic[strs[i]] = req.assetBundle;
//                 }
//             }
//             else
//             {
//                 while (abDic[strs[i]] == null)
//                     yield return 0;
//             }
//         }

//         if (!abDic.ContainsKey(abName))
//         {
//             if (isSync)
//             {
//                 AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + abName);
//                 abDic.Add(abName, ab);
//             }
//             else
//             {
//                 abDic.Add(abName, null);
//                 AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(PathUrl + abName);
//                 yield return req;
//                 abDic[abName] = req.assetBundle;
//             }
//         }
//         else
//         {
//             while (abDic[abName] == null)
//                 yield return 0;
//         }

//         if (isSync)
//         {
//             Object obj = abDic[abName].LoadAsset(resName);
//             callBack(obj);
//         }
//         else
//         {
//             AssetBundleRequest abq = abDic[abName].LoadAssetAsync(resName);
//             yield return abq;
//             callBack(abq.asset);
//         }
//     }

//     /// <summary>
//     /// 卸载指定AB包
//     /// </summary>
//     /// <param name="name">AB包名</param>
//     /// <param name="callBackResult">卸载结果回调（true成功 false失败-正在加载中）</param>
//     public void UnLoadAB(string name, UnityAction<bool> callBackResult)
//     {
//         if (abDic.ContainsKey(name))
//         {
//             if (abDic[name] == null)
//             {
//                 callBackResult(false);
//                 return;
//             }
//             abDic[name].Unload(false);
//             abDic.Remove(name);
//             callBackResult(true);
//         }
//     }

//     /// <summary>
//     /// 清空所有已加载的AB包
//     /// </summary>
//     public void ClearAB()
//     {
//         //清理前停止所有协程，避免异步加载过程中出错
//         StopAllCoroutines();
//         AssetBundle.UnloadAllAssetBundles(false);
//         abDic.Clear();
//         mainAB = null;
//     }
// }
