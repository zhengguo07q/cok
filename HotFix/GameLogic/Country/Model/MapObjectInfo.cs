using UnityEngine;
using TEngine;
using GameConfig.Country;

namespace GameLogic.Country.Model
{
    /// <summary>
    /// 地图场景对象信息
    /// </summary>
    public class MapObjectInfo
    {
        /// <summary>
        /// 对象ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public MapObject MapObjectEntity { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3Int Position { get; set; }

        /// <summary>
        /// 对象名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 对象等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public long LeagueId { get; set; }

        /// <summary>
        /// 联盟名称
        /// </summary>
        public string LeagueName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreatedAt { get; set; }

        /// <summary>
        /// 从 Protobuf 对象创建
        /// </summary>
        public static MapObjectInfo FromPB(global::Country.V1.MapObject proto)
        {
            if (proto == null) return null;
            var mapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(proto.EntityId);
            if (mapObjectEntity == null)
            {
                Log.Error($"地图对象实体不存在: {proto.EntityId}");
                return null;
            }

            return new MapObjectInfo
            {
                Id = proto.Id,
                MapObjectEntity = mapObjectEntity,
                Position = new Vector3Int(
                    (int)proto.Position.X,
                    (int)proto.Position.Y,
                    (int)proto.Position.Z
                ),
                Name = proto.Name,
                Level = proto.Level,
                LeagueId = proto.LeagueId,
                LeagueName = proto.LeagueName,
                CreatedAt = proto.CreatedAt
            };
        }
    }
}