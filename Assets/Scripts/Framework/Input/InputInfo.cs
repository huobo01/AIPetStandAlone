using UnityEngine;

/// <summary>
/// 输入信息数据结构
/// 记录一个输入事件的按键类型、按键值、触发方式
/// </summary>
public class InputInfo
{
    public enum E_KeyOrMouse
    {
        /// <summary>键盘输入</summary>
        Key,
        /// <summary>鼠标输入</summary>
        Mouse,
    }

    public enum E_InputType
    {
        /// <summary>按下</summary>
        Down,
        /// <summary>抬起</summary>
        Up,
        /// <summary>长按</summary>
        Always,
    }

    //输入类型：键盘或鼠标
    public E_KeyOrMouse keyOrMouse;
    //触发方式：按下/抬起/长按
    public E_InputType inputType;
    //键盘按键
    public KeyCode key;
    //鼠标按键ID
    public int mouseID;

    /// <summary>键盘输入初始化</summary>
    public InputInfo(E_InputType inputType, KeyCode key)
    {
        this.keyOrMouse = E_KeyOrMouse.Key;
        this.inputType = inputType;
        this.key = key;
    }

    /// <summary>鼠标输入初始化</summary>
    public InputInfo(E_InputType inputType, int mouseID)
    {
        this.keyOrMouse = E_KeyOrMouse.Mouse;
        this.inputType = inputType;
        this.mouseID = mouseID;
    }
}
