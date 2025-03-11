using System;
using UnityEngine;

namespace GameBase.Utility
{ 
    public class MathUtility
    {
        /// <summary>
        /// 获得2D位置
        /// </summary>
        /// <param name="postion"></param>
        /// <returns></returns>
        public static Vector2 Get2DPosition(Vector3 postion)
        {
            return new (postion.x, postion.y);
        }

        /// <summary>
        /// 转换成TILE所需位置
        /// </summary>
        /// <param name="postion"></param>
        /// <returns></returns>
        public static Vector3Int Get3DPositionToInt(Vector3 postion)
        {
            return new ((int)Math.Floor(postion.x), (int)Math.Floor(postion.y), (int)Math.Floor(postion.z));
        }
    }
}
