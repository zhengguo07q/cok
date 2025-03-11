using GameLogic.Country.View.Component;
using GameLogic.Country.View.Formation;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 可战斗对象
    /// </summary>
    public class CombatObject : MovableObject
    {
        /// <summary>
        /// 只有怪物和士兵才会有动画和战斗对象
        /// </summary>
        protected override void InitializeHolder() 
        {
            var compAnimation = HolderRef.Add<CompAnimation>();
            compAnimation.SceneObject = this;

            var compCombat = HolderRef.Add<CompCombat>();
            compCombat.Owner = this;

            base.InitializeHolder();
        }
    }

    /// <summary>
    /// 怪物公共基类, 怪物资源使用的是模型
    /// </summary>
    public class Monster : CombatObject 
    { 
    }

    /// <summary>
    /// 野外怪物
    /// </summary>
    public class WildAnimal : Monster
    { 
    }

    /// <summary>
    ///  大型怪物
    /// </summary>
    public class Boss : Monster
    { 
    }

    /// <summary>
    /// 普通士兵
    /// </summary>
    public class SoldierObject : CombatObject
    {
    }


    /// <summary>
    /// 领袖士兵
    /// </summary>
    public class Leader : SoldierObject
    {
    }
}
