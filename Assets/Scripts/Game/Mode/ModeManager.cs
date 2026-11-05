using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模式管理器
/// 管理三种运行模式的切换，负责资源加载/卸载调度
/// 策略模式：每种模式实现 IMode，切换时调用 Exit/Enter
/// </summary>
public class ModeManager : BaseManager<ModeManager>
{
    //当前模式
    private IMode currentMode;
    //所有模式实例
    private Dictionary<ModeType, IMode> modeDic = new Dictionary<ModeType, IMode>();

    private ModeManager()
    {
        MonoMgr.Instance.AddUpdateListener(UpdateCurrentMode);
    }

    /// <summary>
    /// 注册模式
    /// </summary>
    public void RegisterMode(IMode mode)
    {
        if (mode == null) return;
        modeDic[mode.ModeType] = mode;
    }

    /// <summary>
    /// 切换到指定模式
    /// </summary>
    public void SwitchMode(ModeType modeType)
    {
        if (!modeDic.ContainsKey(modeType))
        {
            Debug.LogError($"[ModeManager] 未注册模式：{modeType}");
            return;
        }

        //退出当前模式
        if (currentMode != null)
        {
            currentMode.Exit();
            Debug.Log($"[ModeManager] 退出模式：{currentMode.ModeType}");
        }

        //进入新模式
        currentMode = modeDic[modeType];
        currentMode.Enter();
        Debug.Log($"[ModeManager] 进入模式：{currentMode.ModeType}");
    }

    /// <summary>获取当前模式</summary>
    public IMode CurrentMode => currentMode;

    /// <summary>当前模式类型</summary>
    public ModeType CurrentModeType => currentMode?.ModeType ?? ModeType.Minimal;

    private void UpdateCurrentMode()
    {
        currentMode?.Update(Time.deltaTime);
    }
}
