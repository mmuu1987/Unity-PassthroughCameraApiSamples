using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMap
{

    event Action MapEndEvent;
    /// <summary>
    /// 进入地图，地图所需要做的事
    /// </summary>
    void EnterMap();

    /// <summary>
    /// 退出地图
    /// </summary>
    void ExitMap();


    /// <summary>
    ///  获取相机视图下的，左下，左上，右下，右上的映射到3D空间上的坐标
    /// </summary>
    /// <param name="depth">3D空间的深度,即Z轴</param>
    /// <returns></returns>
    List<Vector3> GetPosition(float depth);
}
