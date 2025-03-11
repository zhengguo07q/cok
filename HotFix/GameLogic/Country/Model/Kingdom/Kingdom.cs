namespace GameLogic.Country.Model
{
    /// <summary>
    /// 王国
    /// </summary>
    public class Kingdom
    {
        /// <summary>
        /// ID
        /// </summary>
        public float id; 

        /// <summary>
        /// 实体ID
        /// </summary>
        public float entityId;

        /// <summary>
        /// 编号
        /// </summary>
        public int count;

        /// <summary>
        /// 创建时间
        /// </summary>
        public float createdAt;


        public static Kingdom FromPb(global::Country.V1.Kingdom pb) {
            Kingdom kingdom = new()
            {
                id = pb.Id,
                entityId = pb.EntityId,
                count = pb.Count,
                createdAt = pb.CreatedAt,
            };
            return kingdom;
        }
    }
}
