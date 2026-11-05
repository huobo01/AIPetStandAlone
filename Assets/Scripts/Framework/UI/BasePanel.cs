using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI面板基类
/// 自动查找子控件并缓存到字典，自动为Button/Slider/Toggle绑定事件
/// 子类重写 ShowMe/HideMe 实现面板显示/隐藏逻辑
/// </summary>
public abstract class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 缓存所有需要代码操作的UI控件（按控件名索引）
    /// </summary>
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();

    /// <summary>
    /// 默认控件名列表 —— 这些名字的控件不会被代码使用，仅作显示
    /// </summary>
    private static readonly List<string> defaultNameList = new List<string>()
    {
        "Image",
        "Text (TMP)",
        "RawImage",
        "Background",
        "Checkmark",
        "Label",
        "Text (Legacy)",
        "Arrow",
        "Placeholder",
        "Fill",
        "Handle",
        "Viewport",
        "Scrollbar Horizontal",
        "Scrollbar Vertical",
        "Text"
    };

    protected virtual void Awake()
    {
        //仅查找并缓存所有子控件到 controlDic（供 GetControl 使用）
        //事件绑定由子类（UIPanelTool 生成的代码）自行处理，避免重复绑定
        FindChildrenControl<Button>();
        FindChildrenControl<Toggle>();
        FindChildrenControl<Slider>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<TMP_Dropdown>();
        FindChildrenControl<TMP_InputField>();
        FindChildrenControl<Dropdown>();
        FindChildrenControl<InputField>();
        FindChildrenControl<Text>();
        FindChildrenControl<TextMeshProUGUI>();
        FindChildrenControl<Image>();
    }

    /// <summary>
    /// 面板显示时调用
    /// </summary>
    public abstract void ShowMe();

    /// <summary>
    /// 面板隐藏时调用
    /// </summary>
    public abstract void HideMe();

    /// <summary>
    /// 按名字和类型获取缓存的控件
    /// </summary>
    public T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (control == null)
                Debug.LogError($"[BasePanel] 不存在名字为{name}且类型为{typeof(T)}的控件");
            return control;
        }
        else
        {
            Debug.LogError($"[BasePanel] 不存在名字为{name}的控件");
            return null;
        }
    }

    /// <summary>
    /// 按钮点击响应（子类重写）
    /// </summary>
    protected virtual void ClickBtn(string btnName) { }

    /// <summary>
    /// Slider值变化响应（子类重写）
    /// </summary>
    protected virtual void SliderValueChange(string sliderName, float value) { }

    /// <summary>
    /// Toggle值变化响应（子类重写）
    /// </summary>
    protected virtual void ToggleValueChange(string toggleName, bool value) { }

    /// <summary>
    /// 查找并缓存指定类型的所有子控件（仅缓存到 controlDic，不绑定事件）
    /// 事件绑定由 UIPanelTool 生成的子类代码统一处理
    /// </summary>
    private void FindChildrenControl<T>() where T : UIBehaviour
    {
        T[] controls = this.GetComponentsInChildren<T>(true);
        for (int i = 0; i < controls.Length; i++)
        {
            string controlName = controls[i].gameObject.name;
            //跳过默认名字的控件和已记录的控件
            if (controlDic.ContainsKey(controlName) || defaultNameList.Contains(controlName))
                continue;

            controlDic.Add(controlName, controls[i]);
        }
    }
}
