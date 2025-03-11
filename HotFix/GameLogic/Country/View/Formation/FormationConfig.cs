using static GameLogic.Country.View.Animation.AnimationDeploy;

namespace GameLogic.Country.View.Formation
{
    /// <summary>
    /// 阵型配置
    /// </summary>
    public class FormationConfig
    {
        public FormationType FormationType { get; set; }
        public int LeaderConfigId { get; set; }
        public int SoldierConfigId { get; set; }
        public int SoldierCount { get; set; }
    }
}
