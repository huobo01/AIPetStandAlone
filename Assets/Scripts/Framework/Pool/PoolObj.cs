using UnityEngine;

/// <summary>
/// 挂载到需要使用缓存池功能的预设体上
/// 用于设置该类型对象在场景中的数量上限
/// </summary>
public class PoolObj : MonoBehaviour
{
    /// <summary>
    /// 该类型对象的最大数量上限
    /// 0表示不限制（实际代码中NeedCreate会始终返回true）
    /// </summary>
    public int maxNum;
}
