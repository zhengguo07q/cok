using UnityEngine;

namespace GameLogic.Country.View.Animation
{
    /// <summary>
    /// 动画部署相关
    /// </summary>
    public class AnimationDeploy
    {
        /// <summary>
        /// 阵型类型
        /// </summary>
        public enum FormationType
        {
            None = 0,
            Single = 1,      // 单体移动
            Square = 2,      // 方阵
            Triangle = 3,    // 三角阵
            Circle = 4,      // 圆形阵
            Arrow = 5        // 箭头阵
        }

        /// <summary>
        /// 队伍角色类型
        /// </summary>
        public enum UnitRoleType
        {
            None = 0,
            Leader = 1,                 // 领袖
            Guard = 2,
            Soldier = 3,                // 护卫
        }

        // 阵型配置
        public static class FormationConfig
        {
            // 方阵配置
            public static readonly Vector2[] SquareFormation = new Vector2[]
            {
                new Vector2(0, 0),      // 领袖位置
                new Vector2(-1, -1),    // 护卫1
                new Vector2(1, -1),     // 护卫2
                new Vector2(-1, 1),     // 护卫3
                new Vector2(1, 1)       // 护卫4
            };

            // 三角阵配置
            public static readonly Vector2[] TriangleFormation = new Vector2[]
            {
                new Vector2(0, 1),      // 领袖位置
                new Vector2(-1, 0),     // 护卫1
                new Vector2(1, 0),      // 护卫2
                new Vector2(-2, -1),    // 士兵1
                new Vector2(2, -1)      // 士兵2
            };

            // 单位间距
            public static readonly float UnitSpacing = 2f;
            // 阵型旋转速度
            public static readonly float RotationSpeed = 5f;
            // 队形保持容差
            public static readonly float PositionTolerance = 0.1f;
        }

        /// <summary>
        /// 获取阵型位置
        /// </summary>
        public static Vector2[] GetFormationPositions(FormationType formation)
        {
            switch (formation)
            {
                case FormationType.Square:
                    return FormationConfig.SquareFormation;
                case FormationType.Triangle:
                    return FormationConfig.TriangleFormation;
                // ... 其他阵型
                default:
                    return new Vector2[] { Vector2.zero };
            }
        }

        /// <summary>
        /// 获取角色在阵型中的位置
        /// </summary>
        public static Vector2 GetUnitPositionInFormation(
            FormationType formation, 
            UnitRoleType role, 
            int unitIndex)
        {
            var positions = GetFormationPositions(formation);
            int posIndex = 0;

            // 根据角色类型和索引确定位置
            switch (role)
            {
                case UnitRoleType.Leader:
                    posIndex = 0;  // 领袖总是在第一个位置
                    break;
                case UnitRoleType.Guard:
                    posIndex = Mathf.Min(unitIndex + 1, positions.Length - 1);
                    break;
                case UnitRoleType.Soldier:
                    posIndex = Mathf.Min(unitIndex + 3, positions.Length - 1);
                    break;
            }

            return positions[posIndex] * FormationConfig.UnitSpacing;
        }

    }
}
