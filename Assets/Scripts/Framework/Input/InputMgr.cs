using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 输入管理器
/// 基于旧Input系统，支持键盘/鼠标按键监听，通过事件中心分发输入事件
/// 支持改键、获取下一次按键输入
/// </summary>
public class InputMgr : BaseManager<InputMgr>
{
    //事件类型 -> 输入信息 的映射
    private System.Collections.Generic.Dictionary<E_EventType, InputInfo> inputDic = new System.Collections.Generic.Dictionary<E_EventType, InputInfo>();
    //当前遍历到的输入信息
    private InputInfo nowInputInfo;
    //是否开启输入检测
    private bool isStart;
    //获取下一次输入的回调
    private UnityAction<InputInfo> getInputInfoCallBack;
    //是否开始检测下一次输入
    private bool isBeginCheckInput;

    private InputMgr()
    {
        MonoMgr.Instance.AddUpdateListener(InputUpdate);
    }

    /// <summary>开启或关闭输入检测</summary>
    public void StartOrCloseInputMgr(bool isStart)
    {
        this.isStart = isStart;
    }

    /// <summary>
    /// 配置键盘按键监听
    /// </summary>
    public void ChangeKeyboardInfo(E_EventType eventType, KeyCode key, InputInfo.E_InputType inputType)
    {
        if (!inputDic.ContainsKey(eventType))
            inputDic.Add(eventType, new InputInfo(inputType, key));
        else
        {
            inputDic[eventType].keyOrMouse = InputInfo.E_KeyOrMouse.Key;
            inputDic[eventType].key = key;
            inputDic[eventType].inputType = inputType;
        }
    }

    /// <summary>
    /// 配置鼠标按键监听
    /// </summary>
    public void ChangeMouseInfo(E_EventType eventType, int mouseID, InputInfo.E_InputType inputType)
    {
        if (!inputDic.ContainsKey(eventType))
            inputDic.Add(eventType, new InputInfo(inputType, mouseID));
        else
        {
            inputDic[eventType].keyOrMouse = InputInfo.E_KeyOrMouse.Mouse;
            inputDic[eventType].mouseID = mouseID;
            inputDic[eventType].inputType = inputType;
        }
    }

    /// <summary>移除指定事件的输入监听</summary>
    public void RemoveInputInfo(E_EventType eventType)
    {
        if (inputDic.ContainsKey(eventType))
            inputDic.Remove(eventType);
    }

    /// <summary>获取下一次按键输入（用于改键界面）</summary>
    public void GetInputInfo(UnityAction<InputInfo> callBack)
    {
        getInputInfoCallBack = callBack;
        MonoMgr.Instance.StartCoroutine(BeginCheckInput());
    }

    private IEnumerator BeginCheckInput()
    {
        yield return 0;
        isBeginCheckInput = true;
    }

    private void InputUpdate()
    {
        //获取下一次输入的检测逻辑
        if (isBeginCheckInput)
        {
            if (Input.anyKeyDown)
            {
                InputInfo inputInfo = null;
                //遍历所有键盘按键
                Array keyCodes = Enum.GetValues(typeof(KeyCode));
                foreach (KeyCode inputKey in keyCodes)
                {
                    if (Input.GetKeyDown(inputKey))
                    {
                        inputInfo = new InputInfo(InputInfo.E_InputType.Down, inputKey);
                        break;
                    }
                }
                //检测鼠标按键
                for (int i = 0; i < 3; i++)
                {
                    if (Input.GetMouseButtonDown(i))
                    {
                        inputInfo = new InputInfo(InputInfo.E_InputType.Down, i);
                        break;
                    }
                }
                getInputInfoCallBack?.Invoke(inputInfo);
                getInputInfoCallBack = null;
                isBeginCheckInput = false;
            }
        }

        if (!isStart) return;

        //遍历所有已配置的输入监听
        foreach (E_EventType eventType in inputDic.Keys)
        {
            nowInputInfo = inputDic[eventType];
            if (nowInputInfo.keyOrMouse == InputInfo.E_KeyOrMouse.Key)
            {
                switch (nowInputInfo.inputType)
                {
                    case InputInfo.E_InputType.Down:
                        if (Input.GetKeyDown(nowInputInfo.key))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                    case InputInfo.E_InputType.Up:
                        if (Input.GetKeyUp(nowInputInfo.key))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                    case InputInfo.E_InputType.Always:
                        if (Input.GetKey(nowInputInfo.key))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                }
            }
            else
            {
                switch (nowInputInfo.inputType)
                {
                    case InputInfo.E_InputType.Down:
                        if (Input.GetMouseButtonDown(nowInputInfo.mouseID))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                    case InputInfo.E_InputType.Up:
                        if (Input.GetMouseButtonUp(nowInputInfo.mouseID))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                    case InputInfo.E_InputType.Always:
                        if (Input.GetMouseButton(nowInputInfo.mouseID))
                            EventCenter.Instance.Trigger(eventType);
                        break;
                }
            }
        }

        //水平/竖直轴输入
        EventCenter.Instance.Trigger<float>(E_EventType.E_Input_Horizontal, Input.GetAxis("Horizontal"));
        EventCenter.Instance.Trigger<float>(E_EventType.E_Input_Vertical, Input.GetAxis("Vertical"));
    }
}
