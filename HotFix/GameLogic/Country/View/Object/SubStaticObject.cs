using UnityEngine;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 景观对象， 不可以选中触发的对象，目前景观对象都通过编辑器创建
    /// </summary>
    public class Landscape : StaticObject{}

    /// <summary>
    /// 树
    /// </summary>
    public class Tree : Landscape
    {
    }

    /// <summary>
    /// 石头
    /// </summary>
    public class Rock : Landscape
    {
    }

    /// <summary>
    ///  山
    /// </summary>
    public class Mountain : Landscape
    {
    }

    /// <summary>
    /// 湖
    /// </summary>
    public class Lake : Landscape
    {
    }


    public class BuildObject : StaticObject { }

    /// <summary>
    /// 联盟旗帜
    /// </summary>
    public class LeagueFlag : BuildObject
    {
        protected override string ObjectPath => "Building_trianglewar_fort_201_1";
        protected override Vector3 MountBottom => transform.position + new Vector3(0, 20, 0);
        protected override Vector3 MountHead => transform.position + new Vector3(0, -20, 0);
    }


    /// <summary>
    /// 领主，玩家角色的建筑
    /// </summary>
    public class Warlord : BuildObject { }

    /// <summary>
    /// 城堡
    /// </summary>
    public class Castle : BuildObject { }

    /// <summary>
    /// 堡垒，一种可以被领主占领的建筑
    /// </summary>
    public class Fortress : BuildObject { }

    /// <summary>
    /// 工程站点
    /// </summary>
    public class EngineeringStation : BuildObject { }

    /// <summary>
    /// 炮台
    /// </summary>
    public class CannonTower : BuildObject { }

    /// <summary>
    /// 太阳城，一种可以被领主占领的建筑
    /// </summary>
    public class SunCity : BuildObject { }

    /// <summary>
    /// 领主占领的领地
    /// </summary>
    public class Occupy : BuildObject { }


    /// <summary>
    /// 资源
    /// 资源公共基类, 资源使用的是图片
    /// 服务器动态生成，它的坐标，离散度，数量，方向等都由服务器随机控制或者配置控制
    /// </summary>
    public class Resource : StaticObject
    {
    }

    /// <summary>
    /// 牧场
    /// </summary>
    public class Pasture : Resource
    {
    }

    /// <summary>
    /// 木材厂
    /// </summary>
    public class TimberMill : Resource
    {
    }

    /// <summary>
    /// 煤矿场
    /// </summary>
    public class CoalMine : Resource
    {
    }

    /// <summary>
    /// 铁矿场
    /// </summary>
    class IronMine : Resource
    {
    }
}
