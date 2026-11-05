using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 数学工具类
/// 提供角度转换、距离计算、位置判断、射线检测、范围检测等常用数学功能
/// </summary>
public class MathUtil
{
    #region 角度和弧度

    /// <summary>角度转弧度</summary>
    public static float Deg2Rad(float deg)
    {
        return deg * Mathf.Deg2Rad;
    }

    /// <summary>弧度转角度</summary>
    public static float Rad2Deg(float rad)
    {
        return rad * Mathf.Rad2Deg;
    }

    #endregion

    #region 距离计算

    /// <summary>获取XZ平面上两点的距离</summary>
    public static float GetObjDistanceXZ(Vector3 srcPos, Vector3 targetPos)
    {
        //使用局部变量，避免修改传入的参数
        srcPos.y = 0;
        targetPos.y = 0;
        return Vector3.Distance(srcPos, targetPos);
    }

    /// <summary>判断XZ平面上两点距离是否小于等于目标距离</summary>
    public static bool CheckObjDistanceXZ(Vector3 srcPos, Vector3 targetPos, float dis)
    {
        return GetObjDistanceXZ(srcPos, targetPos) <= dis;
    }

    /// <summary>获取XY平面上两点的距离</summary>
    public static float GetObjDistanceXY(Vector3 srcPos, Vector3 targetPos)
    {
        srcPos.z = 0;
        targetPos.z = 0;
        return Vector3.Distance(srcPos, targetPos);
    }

    /// <summary>判断XY平面上两点距离是否小于等于目标距离</summary>
    public static bool CheckObjDistanceXY(Vector3 srcPos, Vector3 targetPos, float dis)
    {
        return GetObjDistanceXY(srcPos, targetPos) <= dis;
    }

    #endregion

    #region 位置判断

    /// <summary>
    /// 判断世界坐标点是否在屏幕可见范围外
    /// </summary>
    public static bool IsWorldPosOutScreen(Vector3 pos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        if (screenPos.x >= 0 && screenPos.x <= Screen.width &&
            screenPos.y >= 0 && screenPos.y <= Screen.height)
            return false;
        return true;
    }

    /// <summary>
    /// 判断目标是否在指定扇形范围内（XZ平面）
    /// </summary>
    /// <param name="pos">扇形中心点</param>
    /// <param name="forward">朝向</param>
    /// <param name="targetPos">目标点</param>
    /// <param name="radius">半径</param>
    /// <param name="angle">扇形角度</param>
    public static bool IsInSectorRangeXZ(Vector3 pos, Vector3 forward, Vector3 targetPos, float radius, float angle)
    {
        pos.y = 0;
        forward.y = 0;
        targetPos.y = 0;
        return Vector3.Distance(pos, targetPos) <= radius && Vector3.Angle(forward, targetPos - pos) <= angle / 2f;
    }

    #endregion

    #region 射线检测

    /// <summary>射线检测，返回RaycastHit信息</summary>
    public static void RayCast(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
            callBack.Invoke(hitInfo);
    }

    /// <summary>射线检测，返回GameObject</summary>
    public static void RayCast(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
            callBack.Invoke(hitInfo.collider.gameObject);
    }

    /// <summary>射线检测，返回指定组件</summary>
    public static void RayCast<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
            callBack.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
    }

    /// <summary>射线检测所有命中对象，返回RaycastHit信息</summary>
    public static void RayCastAll(Ray ray, UnityAction<RaycastHit> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack.Invoke(hitInfos[i]);
    }

    /// <summary>射线检测所有命中对象，返回GameObject</summary>
    public static void RayCastAll(Ray ray, UnityAction<GameObject> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack.Invoke(hitInfos[i].collider.gameObject);
    }

    /// <summary>射线检测所有命中对象，返回指定组件</summary>
    public static void RayCastAll<T>(Ray ray, UnityAction<T> callBack, float maxDistance, int layerMask)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        for (int i = 0; i < hitInfos.Length; i++)
            callBack.Invoke(hitInfos[i].collider.gameObject.GetComponent<T>());
    }

    #endregion

    #region 范围检测

    /// <summary>
    /// 盒装范围检测
    /// </summary>
    /// <typeparam name="T">Collider / GameObject / 组件类型</typeparam>
    public static void OverlapBox<T>(Vector3 center, Quaternion rotation, Vector3 halfExtents, int layerMask, UnityAction<T> callBack) where T : class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callBack.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callBack.Invoke(colliders[i].gameObject as T);
            else
                callBack.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }

    /// <summary>
    /// 球体范围检测
    /// </summary>
    /// <typeparam name="T">Collider / GameObject / 组件类型</typeparam>
    public static void OverlapSphere<T>(Vector3 center, float radius, int layerMask, UnityAction<T> callBack) where T : class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callBack.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callBack.Invoke(colliders[i].gameObject as T);
            else
                callBack.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }

    #endregion
}
