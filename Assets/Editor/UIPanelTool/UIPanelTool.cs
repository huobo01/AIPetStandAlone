using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI面板代码自动生成工具
/// 选中面板根节点后，通过菜单"GameObject/UI/自动生成面板脚本文件"打开工具窗口
/// 自动扫描面板下所有可交互控件，生成变量声明、查找、事件监听及响应函数的代码
/// 生成两份脚本：{面板名}Base.cs（基类，自动生成可覆盖）和 {面板名}.cs（子类，手动编写逻辑）
/// </summary>
public class UIPanelTool : EditorWindow
{
    //基类脚本内容
    private string panelBaseScript;
    //子类脚本内容
    private string panelScript;
    //面板名
    private string panelName;

    //GUI滚动视图位置
    private Vector2 nowPos;
    private Vector2 nowPos2;

    //Unity默认创建的UI子物体名称，这些控件不需要生成代码
    private readonly List<string> defaultName = new List<string>()
    {
        "Image", "Text (TMP)", "RawImage", "Background", "Checkmark",
        "Label", "Text (Legacy)", "Arrow", "Placeholder", "Fill",
        "Handle", "Viewport", "Scrollbar Horizontal", "Scrollbar Vertical",
        "Text"
    };

    //记录已处理控件名及其类型，用于检测同名同类型冲突
    private Dictionary<string, Type> controlType = new Dictionary<string, Type>();

    [MenuItem("GameObject/UI/自动生成面板脚本文件")]
    private static void CreateToolPanel()
    {
        UIPanelTool win = EditorWindow.GetWindow<UIPanelTool>("自动生成面板脚本工具");
        win.Show();
        win.InitInfo();
    }

    public void InitInfo()
    {
        panelBaseScript = "";
        panelScript = "";

        //避免残留上一次的信息（比如工具窗口没关闭）
        controlType.Clear();
        nowPos = Vector2.zero;
        nowPos2 = Vector2.zero;

        //获取选择的对象
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
            return;
        panelName = obj.name.Trim();

        ControlStrInfo strInfo = new ControlStrInfo();
        //需要扫描的控件类型列表（顺序决定生成代码的顺序）
        Type[] controlTypes = new Type[]
        {
            typeof(Button),
            typeof(Toggle),
            typeof(Slider),
            typeof(TMP_InputField),
            typeof(TMP_Dropdown),
            typeof(InputField),
            typeof(Dropdown),
            typeof(ScrollRect),
            typeof(Text),
            typeof(TextMeshProUGUI),
            typeof(Image),
            typeof(RawImage),
        };

        for (int i = 0; i < controlTypes.Length; i++)
        {
            ControlStrInfo controlInfo = FindControl(obj, controlTypes[i]);
            //返回null证明存在同名同类型控件，需要用户先解决
            if (controlInfo == null)
                return;
            strInfo += controlInfo;
        }

        //读取模板并填充参数
        TextAsset baseStr = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Editor/UIPanelTool/UIConfigBase.txt");
        panelBaseScript = string.Format(baseStr.text,
                                        obj.name,               //{0} 面板名
                                        strInfo.nameStr,        //{1} 控件变量的声明
                                        strInfo.findStr,        //{2} 控件的查找
                                        strInfo.listenerStr,    //{3} 控件事件的监听
                                        strInfo.funcStr);       //{4} 控件事件对应的响应函数

        TextAsset str = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Editor/UIPanelTool/UIConfig.txt");
        panelScript = string.Format(str.text, obj.name, obj.name);
    }

    /// <summary>
    /// 查找面板下指定类型的所有控件并生成对应代码片段
    /// </summary>
    private ControlStrInfo FindControl(GameObject obj, Type type)
    {
        ControlStrInfo info = new ControlStrInfo();
        Component[] controls = obj.GetComponentsInChildren(type);
        for (int i = 0; i < controls.Length; i++)
        {
            //跳过Unity默认UI子物体和面板根节点自身
            if (defaultName.Contains(controls[i].name) || controls[i].name == obj.name)
                continue;

            if (controlType.ContainsKey(controls[i].name))
            {
                //同名同类型控件冲突，提示用户修改
                if (controlType[controls[i].name] == type)
                {
                    EditorUtility.DisplayDialog("重复控件名", $"有两个类型相同的对象重名了：{controls[i].name}", "确定");
                    return null;
                }
                //同名不同类型（如Button和Image在同一对象上），保留已记录的重要控件，忽略当前
                continue;
            }

            controlType.Add(controls[i].name, type);

            //控件变量声明
            info.nameStr += $"public {type.Name} {controls[i].gameObject.name};\n\t";
            //控件查找代码
            info.findStr += $"{controls[i].gameObject.name} = this.transform.Find(\"{GetPath(controls[i].transform, obj.transform)}\").GetComponent<{type.Name}>();\n\t\t";

            //根据控件类型生成对应的事件监听代码
            switch (type.Name)
            {
                case "Button":
                    info.listenerStr += $"{controls[i].gameObject.name}.onClick.AddListener(On{controls[i].gameObject.name}Click);\n\t\t";
                    info.funcStr += $"protected virtual void On{controls[i].gameObject.name}Click(){{}}\n\t";
                    break;
                case "Toggle":
                    info.listenerStr += $"{controls[i].gameObject.name}.onValueChanged.AddListener(On{controls[i].gameObject.name}ValueChanged);\n\t\t";
                    info.funcStr += $"protected virtual void On{controls[i].gameObject.name}ValueChanged(bool value){{}}\n\t";
                    break;
                case "Slider":
                    info.listenerStr += $"{controls[i].gameObject.name}.onValueChanged.AddListener(On{controls[i].gameObject.name}ValueChanged);\n\t\t";
                    info.funcStr += $"protected virtual void On{controls[i].gameObject.name}ValueChanged(float value){{}}\n\t";
                    break;
                case "TMP_Dropdown":
                case "Dropdown":
                    info.listenerStr += $"{controls[i].gameObject.name}.onValueChanged.AddListener(On{controls[i].gameObject.name}ValueChanged);\n\t\t";
                    info.funcStr += $"protected virtual void On{controls[i].gameObject.name}ValueChanged(int value){{}}\n\t";
                    break;
                case "TMP_InputField":
                case "InputField":
                    info.listenerStr += $"{controls[i].gameObject.name}.onValueChanged.AddListener(On{controls[i].gameObject.name}ValueChanged);\n\t\t";
                    info.funcStr += $"protected virtual void On{controls[i].gameObject.name}ValueChanged(string value){{}}\n\t";
                    break;
                default:
                    break;
            }
        }
        return info;
    }

    /// <summary>
    /// 获取控件相对于面板根节点的路径
    /// </summary>
    private string GetPath(Transform obj, Transform panelTrans)
    {
        string path = obj.name;
        while (obj.parent != panelTrans)
        {
            path = obj.parent.name + "/" + path;
            obj = obj.parent;
        }
        return path;
    }

    private void OnGUI()
    {
        if (panelBaseScript != "")
        {
            nowPos = EditorGUILayout.BeginScrollView(nowPos);
            GUILayout.Label(panelBaseScript);
            EditorGUILayout.EndScrollView();
            GUILayout.Label("-----------------------------------");
            nowPos2 = EditorGUILayout.BeginScrollView(nowPos2);
            GUILayout.Label(panelScript);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("选择保存路径"))
            {
                string path = EditorUtility.SaveFilePanel("脚本保存路径", Application.dataPath + "/Scripts/", panelName + "Base", "cs");
                if (path != "")
                {
                    //保存基类脚本（每次都会覆盖，子类脚本不会被覆盖）
                    File.WriteAllText(path, panelBaseScript);
                    //生成子类脚本路径（去掉Base后缀）
                    int index = path.LastIndexOf("Base");
                    string subPath = path.Substring(0, index) + ".cs";
                    //子类脚本已存在则不覆盖，避免覆盖用户已写的逻辑
                    if (!File.Exists(subPath))
                        File.WriteAllText(subPath, panelScript);

                    CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
                    AssetDatabase.Refresh();
                }
            }
        }
        else
            GUILayout.Label("存在同名同类型控件，请先解决该问题");
    }

    /// <summary>
    /// 编译完成后自动将生成的面板脚本挂载到选中的面板对象上
    /// </summary>
    private void OnCompilationFinished(string assemblyPath, CompilerMessage[] messages)
    {
        //只关注主程序集编译完成
        if (assemblyPath.Contains("Assembly-CSharp.dll"))
        {
            //通过反射加载程序集，获取生成的面板类型并挂载组件
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + assemblyPath);
            Selection.activeGameObject.AddComponent(assembly.GetType(panelName));
            CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
        }
    }
}
