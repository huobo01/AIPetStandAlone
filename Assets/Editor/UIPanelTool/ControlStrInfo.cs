/// <summary>
/// 控件动态生成代码的字符串数据结构
/// 分别存储控件的变量声明、查找、事件监听、响应函数四部分代码片段
/// </summary>
public class ControlStrInfo
{
    //控件的变量声明代码
    public string nameStr;
    //控件的查找（GetComponent）代码
    public string findStr;
    //控件的事件监听注册代码
    public string listenerStr;
    //控件的事件响应函数代码
    public string funcStr;

    /// <summary>
    /// 重载+运算符，方便多个控件的代码片段进行拼接
    /// </summary>
    public static ControlStrInfo operator +(ControlStrInfo one, ControlStrInfo two)
    {
        one.nameStr += two.nameStr;
        one.findStr += two.findStr;
        one.listenerStr += two.listenerStr;
        one.funcStr += two.funcStr;
        return one;
    }
}
