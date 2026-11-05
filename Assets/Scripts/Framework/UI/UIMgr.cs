using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// UI层级枚举
/// </summary>
public enum E_UILayer
{
    /// <summary>最底层</summary>
    Bottom,
    /// <summary>中层</summary>
    Middle,
    /// <summary>高层</summary>
    Top,
    /// <summary>系统层（最高层）</summary>
    System,
}

/// <summary>
/// UI面板管理器
/// 管理所有UI面板的显示、隐藏、获取，自动创建Canvas/EventSystem
/// 注意：面板预设体名必须与面板类名一致
/// </summary>
public class UIMgr : BaseManager<UIMgr>
{
    /// <summary>
    /// 面板信息基类，用于字典中里式替换存储
    /// </summary>
    private abstract class BasePanelInfo { }

    /// <summary>
    /// 面板信息：存储面板实例、加载回调、隐藏标记
    /// </summary>
    private class PanelInfo<T> : BasePanelInfo where T : BasePanel
    {
        public T panel;
        public UnityAction<T> callBack;
        public bool isHide;

        public PanelInfo(UnityAction<T> callBack)
        {
            this.callBack += callBack;
        }
    }

    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;

    //四个层级的父对象
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;

    //所有面板的字典：panelName -> PanelInfo
    private Dictionary<string, BasePanelInfo> panelDic = new Dictionary<string, BasePanelInfo>();

    private UIMgr()
    {
        //动态创建UI摄像机
        uiCamera = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);

        //动态创建Canvas
        uiCanvas = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/Canvas")).GetComponent<Canvas>();
        uiCanvas.worldCamera = uiCamera;
        GameObject.DontDestroyOnLoad(uiCanvas.gameObject);

        //获取四个层级父对象
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");

        //动态创建EventSystem
        uiEventSystem = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiEventSystem.gameObject);
    }

    /// <summary>
    /// 获取对应层级的父对象
    /// </summary>
    public Transform GetLayerFather(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom: return bottomLayer;
            case E_UILayer.Middle: return middleLayer;
            case E_UILayer.Top: return topLayer;
            case E_UILayer.System: return systemLayer;
            default: return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="layer">显示层级</param>
    /// <param name="callBack">加载完成回调</param>
    /// <param name="isSync">是否同步加载</param>
    public void ShowPanel<T>(E_UILayer layer = E_UILayer.Middle, UnityAction<T> callBack = null, bool isSync = false) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        //面板已存在
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //正在异步加载中
            if (panelInfo.panel == null)
            {
                panelInfo.isHide = false;
                if (callBack != null)
                    panelInfo.callBack += callBack;
            }
            else //已加载完成
            {
                if (!panelInfo.panel.gameObject.activeSelf)
                    panelInfo.panel.gameObject.SetActive(true);
                panelInfo.panel.ShowMe();
                callBack?.Invoke(panelInfo.panel);
            }
            return;
        }

        //不存在面板：先占位置，再异步加载
        panelDic.Add(panelName, new PanelInfo<T>(callBack));

        ABResMgr.Instance.LoadResAsync<GameObject>("ui", panelName, (res) =>
        {
            PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
            //加载完成前就被要求隐藏
            if (panelInfo.isHide)
            {
                panelDic.Remove(panelName);
                return;
            }

            Transform father = GetLayerFather(layer) ?? middleLayer;
            GameObject panelObj = GameObject.Instantiate(res, father, false);

            T panel = panelObj.GetComponent<T>();
            panel.ShowMe();
            panelInfo.callBack?.Invoke(panel);
            panelInfo.callBack = null;
            panelInfo.panel = panel;
        }, isSync);
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="isDestory">是否销毁（false则仅失活，下次显示可复用）</param>
    public void HidePanel<T>(bool isDestory = false) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (!panelDic.ContainsKey(panelName)) return;

        PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
        //正在加载中
        if (panelInfo.panel == null)
        {
            panelInfo.isHide = true;
            panelInfo.callBack = null;
        }
        else //已加载完成
        {
            panelInfo.panel.HideMe();
            if (isDestory)
            {
                GameObject.Destroy(panelInfo.panel.gameObject);
                panelDic.Remove(panelName);
            }
            else
            {
                panelInfo.panel.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 获取已显示的面板
    /// </summary>
    public void GetPanel<T>(UnityAction<T> callBack) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (!panelDic.ContainsKey(panelName)) return;

        PanelInfo<T> panelInfo = panelDic[panelName] as PanelInfo<T>;
        if (panelInfo.panel == null)
        {
            //加载中：等加载完通过回调传出
            panelInfo.callBack += callBack;
        }
        else if (!panelInfo.isHide)
        {
            callBack?.Invoke(panelInfo.panel);
        }
    }

    /// <summary>
    /// 为控件添加自定义事件监听（保证只挂载一个EventTrigger）
    /// </summary>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callBack)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);
        trigger.triggers.Add(entry);
    }
}
