using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 计时器管理器
/// 支持普通计时器（受Time.timeScale影响）和真实计时器（不受影响）
/// 支持总时间回调、间隔回调、暂停/恢复/重置
/// </summary>
public class TimerMgr : BaseManager<TimerMgr>
{
    //当前计时器唯一ID
    private int TIMER_KEY = 0;
    //受timeScale影响的计时器字典
    private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();
    //不受timeScale影响的真实计时器字典
    private Dictionary<int, TimerItem> realTimerDic = new Dictionary<int, TimerItem>();
    //待移除列表
    private List<TimerItem> delList = new List<TimerItem>();

    //复用WaitForSeconds对象，避免每帧GC
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(intervalTime);
    private WaitForSeconds waitForSeconds = new WaitForSeconds(intervalTime);

    private Coroutine timer;
    private Coroutine realTimer;

    //计时协程的间隔时间（秒）
    private const float intervalTime = 0.1f;

    private TimerMgr()
    {
        Start();
    }

    /// <summary>开启计时器管理器</summary>
    public void Start()
    {
        timer = MonoMgr.Instance.StartCoroutine(StartTiming(false, timerDic));
        realTimer = MonoMgr.Instance.StartCoroutine(StartTiming(true, realTimerDic));
    }

    /// <summary>关闭计时器管理器</summary>
    public void Stop()
    {
        MonoMgr.Instance.StopCoroutine(timer);
        MonoMgr.Instance.StopCoroutine(realTimer);
    }

    IEnumerator StartTiming(bool isRealTime, Dictionary<int, TimerItem> timerDic)
    {
        while (true)
        {
            if (isRealTime)
                yield return waitForSecondsRealtime;
            else
                yield return waitForSeconds;

            //遍历所有计时器更新数据
            foreach (TimerItem item in timerDic.Values)
            {
                if (!item.isRuning) continue;
                //间隔回调
                if (item.callBack != null)
                {
                    item.intervalTime -= (int)(intervalTime * 1000);
                    if (item.intervalTime <= 0)
                    {
                        item.callBack.Invoke();
                        item.intervalTime = item.maxIntervalTime;
                    }
                }
                //总时间更新
                item.allTime -= (int)(intervalTime * 1000);
                if (item.allTime <= 0)
                {
                    item.overCallBack.Invoke();
                    delList.Add(item);
                }
            }

            //移除过期计时器
            for (int i = 0; i < delList.Count; i++)
            {
                timerDic.Remove(delList[i].keyID);
                PoolMgr.Instance.PushObj(delList[i]);
            }
            delList.Clear();
        }
    }

    /// <summary>
    /// 创建计时器
    /// </summary>
    /// <param name="isRealTime">是否不受Time.timeScale影响</param>
    /// <param name="allTime">总时间（毫秒）</param>
    /// <param name="overCallBack">总时间结束回调</param>
    /// <param name="intervalTime">间隔时间（毫秒），0表示无间隔回调</param>
    /// <param name="callBack">间隔回调</param>
    /// <returns>计时器唯一ID</returns>
    public int CreateTimer(bool isRealTime, int allTime, UnityAction overCallBack, int intervalTime = 0, UnityAction callBack = null)
    {
        int keyID = ++TIMER_KEY;
        TimerItem timerItem = PoolMgr.Instance.GetObj<TimerItem>();
        timerItem.InitInfo(keyID, allTime, overCallBack, intervalTime, callBack);
        if (isRealTime)
            realTimerDic.Add(keyID, timerItem);
        else
            timerDic.Add(keyID, timerItem);
        return keyID;
    }

    /// <summary>移除计时器</summary>
    public void RemoveTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
        {
            PoolMgr.Instance.PushObj(timerDic[keyID]);
            timerDic.Remove(keyID);
        }
        else if (realTimerDic.ContainsKey(keyID))
        {
            PoolMgr.Instance.PushObj(realTimerDic[keyID]);
            realTimerDic.Remove(keyID);
        }
    }

    /// <summary>重置计时器</summary>
    public void ResetTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
            timerDic[keyID].ResetTimer();
        else if (realTimerDic.ContainsKey(keyID))
            realTimerDic[keyID].ResetTimer();
    }

    /// <summary>开启计时器（暂停后恢复）</summary>
    public void StartTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
            timerDic[keyID].isRuning = true;
        else if (realTimerDic.ContainsKey(keyID))
            realTimerDic[keyID].isRuning = true;
    }

    /// <summary>暂停计时器</summary>
    public void StopTimer(int keyID)
    {
        if (timerDic.ContainsKey(keyID))
            timerDic[keyID].isRuning = false;
        else if (realTimerDic.ContainsKey(keyID))
            realTimerDic[keyID].isRuning = false;
    }
}
