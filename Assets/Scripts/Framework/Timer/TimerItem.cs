using UnityEngine.Events;

/// <summary>
/// 计时器数据对象
/// 存储计时器的时间、回调等数据，配合对象池复用
/// </summary>
public class TimerItem : IPoolObject
{
    //唯一ID
    public int keyID;
    //总时间结束回调
    public UnityAction overCallBack;
    //间隔时间回调
    public UnityAction callBack;

    //总计时时间（毫秒）
    public int allTime;
    //初始总时间（用于重置）
    public int maxAllTime;

    //间隔执行时间（毫秒）
    public int intervalTime;
    //初始间隔时间（用于重置）
    public int maxIntervalTime;

    //是否正在计时
    public bool isRuning;

    /// <summary>
    /// 初始化计时器数据
    /// </summary>
    public void InitInfo(int keyID, int allTime, UnityAction overCallBack, int intervalTime = 0, UnityAction callBack = null)
    {
        this.keyID = keyID;
        this.maxAllTime = this.allTime = allTime;
        this.overCallBack = overCallBack;
        this.maxIntervalTime = this.intervalTime = intervalTime;
        this.callBack = callBack;
        this.isRuning = true;
    }

    /// <summary>重置计时器到初始状态</summary>
    public void ResetTimer()
    {
        this.allTime = this.maxAllTime;
        this.intervalTime = this.maxIntervalTime;
        this.isRuning = true;
    }

    /// <summary>回收到对象池时清除引用</summary>
    public void ResetInfo()
    {
        overCallBack = null;
        callBack = null;
    }
}
