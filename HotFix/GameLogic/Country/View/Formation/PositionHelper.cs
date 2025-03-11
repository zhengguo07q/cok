using static GameLogic.Country.View.Animation.AnimationDeploy;
using UnityEngine;

namespace GameLogic.Country.View.Formation
{
    /// <summary>
    /// 位置辅助器
    /// </summary>
    public static class PositionHelper
    {
        /// <summary>
        /// 获取单位在编队中的位置
        /// </summary>
        /// <param name="formationType">编队类型</param>
        /// <param name="tactics">当前战术</param>
        /// <param name="roleType">单位角色类型</param>
        /// <param name="unitIndex">单位索引</param>
        /// <returns>单位在编队中的相对位置</returns>
        public static Vector2 GetUnitPositionInFormation(
            FormationType formationType,
            FormationTactics tactics,
            UnitRoleType roleType,
            int unitIndex,
            Vector2 formationForward
            )
        {
            // 基础位置
            Vector2 basePosition = GetBasePosition(formationType, roleType, unitIndex);
            // 根据战术调整位置
            Vector2 adjustedPosition = AdjustPositionByTactics(basePosition, tactics, roleType);
            return adjustedPosition;
            // 将位置旋转到编队方向
            //    return RotatePosition(adjustedPosition, formationForward);
        }

        private static Vector2 GetBasePosition(FormationType formationType, UnitRoleType roleType, int unitIndex)
        {
            // 领袖总是在中心位置
            if (roleType == UnitRoleType.Leader)
            {
                return Vector2.zero;
            }

            // 士兵位置根据编队类型计算
            switch (formationType)
            {
                case FormationType.Triangle:
                    return GetTrianglePosition(unitIndex);
                case FormationType.Square:
                    return GetSquarePosition(unitIndex);
                case FormationType.Circle:
                    return GetCirclePosition(unitIndex);
                default:
                    return Vector2.zero;
            }
        }

        private static Vector2 GetTrianglePosition(int index)
        {
            float spacing = 1f; // 单位间距
            int row = Mathf.FloorToInt((-1 + Mathf.Sqrt(1 + 8 * index)) / 2);
            int rowStart = (row * (row + 1)) / 2;
            int column = index - rowStart;

            float x = (column - row / 2f) * spacing;
            float z = -row * spacing;

            return new Vector2(x, z);
        }

        private static Vector2 GetSquarePosition(int index)
        {
            float spacing = 1f;
            int size = Mathf.CeilToInt(Mathf.Sqrt(index + 1));
            int row = index / size;
            int column = index % size;

            float x = (column - (size - 1) / 2f) * spacing;
            float z = (-row + (size - 1) / 2f) * spacing;

            return new Vector2(x, z);
        }

        private static Vector2 GetCirclePosition(int index)
        {
            float radius = 1f;
            float angle = (index * 2 * Mathf.PI) / 8; // 假设最多8个单位

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            return new Vector2(x, z);
        }

        private static Vector2 AdjustPositionByTactics(Vector2 basePosition, FormationTactics tactics, UnitRoleType roleType)
        {
            switch (tactics)
            {
                case FormationTactics.Aggressive:
                    // 进攻阵型：单位更加靠前
                    return new Vector2(basePosition.x, basePosition.y * 0.8f);

                case FormationTactics.Defensive:
                    // 防守阵型：单位更加集中，保护领袖
                    if (roleType == UnitRoleType.Leader)
                    {
                        return basePosition * 0.5f; // 领袖后撤
                    }
                    return basePosition * 0.7f; // 其他单位收缩

                case FormationTactics.Retreat:
                    // 撤退阵型：单位更加分散
                    return basePosition * 1.2f;

                default: // FormationTactics.Normal
                    return basePosition;
            }
        }

        private static Vector2 RotatePosition(Vector2 position, Vector2 forward)
        {
            float angle = Vector2.SignedAngle(Vector2.up, forward.normalized);
            float rad = angle * Mathf.Deg2Rad;
            
            float x = position.x * Mathf.Cos(rad) - position.y * Mathf.Sin(rad);
            float y = position.x * Mathf.Sin(rad) + position.y * Mathf.Cos(rad);
            
            return new Vector2(x, y);
        }
    }
}
