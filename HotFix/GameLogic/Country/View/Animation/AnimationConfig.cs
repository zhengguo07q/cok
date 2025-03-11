namespace GameLogic.Country.View.Animation
{
    /// <summary>
    /// 动画配置， 有些动画是有的， 有些没有, 默认除开
    /// </summary>
    public class AnimationConfig
    {
        public string Id;
        public bool HasIdleAnimation = true; // 默认存在
        public bool HasMoveAnimation = false;
        public bool HasAttackAnimation = false;
        public bool HasDeathAnimation = false;
        public bool HasHitAnimation = false;
    }
}
