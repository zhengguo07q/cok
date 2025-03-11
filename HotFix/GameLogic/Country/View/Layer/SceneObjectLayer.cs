using GameBase.Layer;
using GameBase.Utility;
using GameConfig.Country;
using GameLogic.Country.Manager;
using GameLogic.Country.Model;
using GameLogic.Country.View.Formation;
using GameLogic.Country.View.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using TEngine;
using UnityEngine;
using Tree = GameLogic.Country.View.Object.Tree;


namespace GameLogic.Country.View.Layer
{
    /// <summary>
    /// 场景对象层
    /// </summary>
    [LayerBinding(layerName: LayerName.SceneObjectLayer)]
    public class SceneObjectLayer : WindowLayerBase
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;

        [Header("References")]
        [SerializeField] private Transform rootReference;

        [Header("Display Settings")]
        [SerializeField] private float baseShowThreshold = 0.3f;
        [SerializeField] private float levelThresholdMultiplier = 1.5f;
        [SerializeField] private float minVisibleY = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private int totalObjectCount = 0;
        [SerializeField] private int visibleObjectCount = 0;

        [Header("Visibility Settings")]
        [SerializeField] private float updateThreshold = 0.5f;

        // 按王国ID组织场景对象: <KingdomId, <ObjectId, SceneObject>>
        private Dictionary<long, Dictionary<long, SceneObject>> kingdomSceneObjects;

        public override void Initialize()
        {
            kingdomSceneObjects = new Dictionary<long, Dictionary<long, SceneObject>>();
            rootReference = FindChild(SceneRef.Scene.SceneGameObject.transform, "SceneMap/ObjectLayer");

            // 设置世界边界
            VisibilityManager.Instance.ResetQuadTree(LayerNameStr, ViewportManager.Instance.WorldBounds);


            if (showDebugInfo)
            {
                Debug.Log("SceneObjectLayer initialized");
            }
        }

        /// <summary>
        /// 创建场景对象（服务器下发）
        /// </summary>
        public void CreateSceneObject(long kingdomId, SceneObjectInfo sceneObjectInfo)
        {
            CreateSceneObjectInternal(kingdomId, sceneObjectInfo);
        }

        /// <summary>
        /// 创建临时场景对象（事件系统）
        /// </summary>
        public SceneObject CreateTemporarySceneObject(long kingdomId, SceneObjectInfo sceneObjectInfo, long temporaryId = 0)
        {
            return CreateSceneObjectInternal(kingdomId, sceneObjectInfo, isTemporary: true, temporaryId: temporaryId);
        }

        /// <summary>
        /// 内部创建场景对象的通用方法
        /// </summary>
        /// <param name="kingdomId">王国ID</param>
        /// <param name="sceneObjectInfo">场景对象信息</param>
        /// <param name="isTemporary">是否为临时对象（事件系统创建）</param>
        /// <returns>创建的场景对象</returns>
        private SceneObject CreateSceneObjectInternal(long kingdomId, SceneObjectInfo sceneObjectInfo, bool isTemporary = false, long temporaryId = 0)
        {
            if (!kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                sceneObjectMap = new Dictionary<long, SceneObject>();
                kingdomSceneObjects[kingdomId] = sceneObjectMap;
            }

            // 检查是否已存在相同ID的对象
            if (sceneObjectMap.ContainsKey(sceneObjectInfo.Id))
            {
                Log.Warning($"场景对象已存在 KingdomId:{kingdomId} ObjectId:{sceneObjectInfo.Id}");
                return sceneObjectMap[sceneObjectInfo.Id];
            }

            // 创建场景对象实例
            SceneObject sceneObject = CreateSceneObjectInstance(sceneObjectInfo.MapObjectEntity.ObjectType);
            sceneObject.SceneObjectInfo = sceneObjectInfo;
            sceneObject.IsTemporary = isTemporary;  // 标记是否为临时对象
            sceneObject.Initialize();
            
            // 添加到管理字典
            sceneObjectMap.Add(sceneObjectInfo.Id, sceneObject);

            // 添加到可见性管理器
            VisibilityManager.Instance.Insert(LayerNameStr, 
                new Vector2(sceneObjectInfo.Position.x, sceneObjectInfo.Position.y), 
                sceneObject);

            totalObjectCount = GetTotalObjectCount();
            
            if (showDebugInfo)
            {
                Debug.Log($"Created {(isTemporary ? "temporary " : "")}object {sceneObjectInfo.MapObjectEntity.ObjectType} " +
                         $"at position {sceneObjectInfo.Position} with ID {sceneObjectInfo.Id}");
            }

            return sceneObject;
        }

        /// <summary>
        /// 生成临时对象的唯一ID
        /// 使用位运算来组合两个ID
        /// buildId 使用高4位
        /// temporaryId 使用低4位
        /// 这样可以保证ID的唯一性，同时保持两个ID的完整信息
        /// </summary>
        private long GenerateTemporaryObjectId(long buildId, long temporaryId)
        {
            const long TEMP_MASK = 0xFF;  // 8位掩码

            // 确保temporaryId不会超过32位
            temporaryId = temporaryId & TEMP_MASK;

            // 将buildId左移32位，然后与temporaryId组合
            return (buildId << 4) | temporaryId;
        }

        /// <summary>
        /// 解析方法
        /// </summary>
        private (long buildId, long temporaryId) ParseTemporaryObjectId(long combinedId)
        {
            const long TEMP_MASK = 0xFFFFFFFF;  // 32位掩码

            long buildId = combinedId >> 32;
            long temporaryId = combinedId & TEMP_MASK;

            return (buildId, temporaryId);
        }

        /// <summary>
        /// 清理临时对象
        /// </summary>
        public void ClearTemporaryObjects(long kingdomId)
        {
            if (kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                var tempObjects = sceneObjectMap.Values
                    .Where(obj => obj.IsTemporary)
                    .ToList();

                foreach (var obj in tempObjects)
                {
                    RemoveSceneObject(kingdomId, obj.SceneObjectInfo.Id);
                }
            }
        }

        private SceneObject CreateSceneObjectInstance(SceneObjectType objectType)
        {
            SceneObject sceneObject = objectType switch
            {
                // 景观对象
                SceneObjectType.Tree => Tree.CreateInstance<Tree>(rootReference),
                SceneObjectType.Rock => Rock.CreateInstance<Rock>(rootReference),
                SceneObjectType.Mountain => Mountain.CreateInstance<Mountain>(rootReference),
                SceneObjectType.Lake => Lake.CreateInstance<Lake>(rootReference),

                // 建筑对象
                SceneObjectType.Castle => Castle.CreateInstance<Castle>(rootReference),
                SceneObjectType.Fortress => Fortress.CreateInstance<Fortress>(rootReference),
                SceneObjectType.LeagueFlag => LeagueFlag.CreateInstance<LeagueFlag>(rootReference),
                SceneObjectType.Warlord => Warlord.CreateInstance<Warlord>(rootReference),
                SceneObjectType.EngineeringStation => EngineeringStation.CreateInstance<EngineeringStation>(rootReference),
                SceneObjectType.CannonTower => CannonTower.CreateInstance<CannonTower>(rootReference),
                SceneObjectType.SunCity => SunCity.CreateInstance<SunCity>(rootReference),
                SceneObjectType.Occupy => Occupy.CreateInstance<Occupy>(rootReference),

                // 资源对象
                SceneObjectType.Pasture => Pasture.CreateInstance<Pasture>(rootReference),
                SceneObjectType.TimberMill => TimberMill.CreateInstance<TimberMill>(rootReference),
                SceneObjectType.CoalMine => CoalMine.CreateInstance<CoalMine>(rootReference),
                SceneObjectType.IronMine => IronMine.CreateInstance<IronMine>(rootReference),

                // 怪物对象
                SceneObjectType.WildAnimal => WildAnimal.CreateInstance<WildAnimal>(rootReference),
                SceneObjectType.Boss => Boss.CreateInstance<Boss>(rootReference),

                // 部队对象
                SceneObjectType.Formation => FormationObject.CreateInstance<FormationObject>(rootReference),
                SceneObjectType.Soldier => FormationObject.CreateInstance<SoldierObject>(rootReference),
                SceneObjectType.Leader => FormationObject.CreateInstance<Leader>(rootReference),

                _ => throw new System.ArgumentException($"Unknown scene object type: {objectType}")
            };
            return sceneObject;
        }

        /// <summary>
        /// 删除地图对象
        /// </summary>
        public void RemoveSceneObject(long kingdomId, long sceneObjectId)
        {
            if (kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                if (sceneObjectMap.TryGetValue(sceneObjectId, out var sceneObject))
                {
                    sceneObjectMap.Remove(sceneObjectId);
                    // 删除可见性
                    VisibilityManager.Instance.Remove(LayerNameStr, sceneObject);
                    // 删除场景对象
                    GameObject.Destroy(sceneObject.gameObject);
                }
            }
        }

        /// <summary>
        /// 清除指定王国的所有场景对象
        /// </summary>
        public void ClearKingdomObjects(long kingdomId)
        {
            if (kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                sceneObjectMap.Clear();
                kingdomSceneObjects.Remove(kingdomId);
            }
        }

        /// <summary>
        /// 更新场景对象, 场景对象属性被改变后需要调用，比如说有些资源被人开始采集等
        /// </summary>
        public void UpdateSceneObject(long kingdomId, SceneObjectInfo sceneObjectInfo)
        {
            if (sceneObjectInfo == null)
            {
                Log.Warning("尝试更新空的场景对象信息");
                return;
            }

            if (!kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                sceneObjectMap = new Dictionary<long, SceneObject>();
                kingdomSceneObjects[kingdomId] = sceneObjectMap;
            }

            // 检查对象是否存在
            if (sceneObjectMap.TryGetValue(sceneObjectInfo.Id, out var sceneObject))
            {
                // 更新对象信息
                sceneObject.SceneObjectInfo = sceneObjectInfo;

                // 如果位置发生变化，更新位置
                if (sceneObject.SceneObjectInfo.Position != sceneObjectInfo.Position)
                {
                    MoveObjectToPosition(kingdomId, sceneObjectInfo.Id, MathUtility.Get3DPositionToInt(sceneObjectInfo.Position));
                }

                // 触发对象更新事件
                sceneObject.MarkAsDirty();
            }
            else
            {
                // 如果对象不存在，创建新对象
                CreateSceneObject(kingdomId, sceneObjectInfo);
                Log.Debug($"王国[{kingdomId}]场景对象[{sceneObjectInfo.Id}]不存在，已创建新对象");
            }
        }

        /// <summary>
        /// 移动对象到给定的tile位置
        /// </summary>
        public void MoveObjectToPosition(long kingdomId, long sceneObjectId, UnityEngine.Vector3Int tilePosition)
        {
            if (kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap) &&
                sceneObjectMap.TryGetValue(sceneObjectId, out var sceneObject))
            {
                // 实现移动逻辑
                // ...
            }
        }

        /// <summary>
        /// 获取场景对象
        /// </summary>
        public SceneObject GetSceneObject(long kingdomId, long sceneObjectId)
        {
            return kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap) &&
                   sceneObjectMap.TryGetValue(sceneObjectId, out var sceneObject)
                ? sceneObject
                : null;
        }

        /// <summary>
        /// 判断是否有场景对象
        /// </summary>
        public bool HasSceneObject(long kingdomId, long sceneObjectId)
        {
            return kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap) &&
                   sceneObjectMap.TryGetValue(sceneObjectId, out _);
        }

        public void UpdateObjectsByScaleLevel(MapLODLevel level)
        {
            if (kingdomSceneObjects == null) return;

            foreach (var kingdomObjects in kingdomSceneObjects.Values)
            {
                foreach (var sceneObject in kingdomObjects.Values)
                {
                    switch (level)
                    {
                        case MapLODLevel.Highest:
                        case MapLODLevel.High:
                            sceneObject.ShowObjectView();
                            break;

                        case MapLODLevel.Medium:
                            sceneObject.ShowIconView();
                            break;

                        case MapLODLevel.Low:
                        default:
                            sceneObject.HideView();
                            break;
                    }
                }
            }
        }

        private int GetTotalObjectCount()
        {
            int count = 0;
            foreach (var kingdomObjects in kingdomSceneObjects.Values)
            {
                count += kingdomObjects.Count;
            }
            return count;
        }


        public void UpdateSceneObjectPosition(long kingdomId, SceneObjectInfo sceneObjectInfo)
        {
            if (!kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap) ||
                !sceneObjectMap.TryGetValue(sceneObjectInfo.Id, out var sceneObject))
            {
                return;
            }

            // 更新对象信息
            sceneObject.SceneObjectInfo = sceneObjectInfo;
            
            // 更新在可见性管理器中的位置
            VisibilityManager.Instance.UpdateItemPosition(LayerNameStr, new Vector2(sceneObjectInfo.Position.x, sceneObjectInfo.Position.y), sceneObject);

            if (showDebugInfo)
            {
                Debug.Log($"Updated {sceneObjectInfo.MapObjectEntity.ObjectType} position to {sceneObjectInfo.Position}");
            }
        }

        /// <summary>
        /// 创建部队
        /// </summary>
        public FormationObject CreateFormation(long kingdomId, SceneObjectInfo formationInfo, FormationConfig config)
        {
            // 1. 创建编队场景对象
            var formationObject = CreateSceneObjectInternal(kingdomId, formationInfo) as FormationObject;
            if (formationObject == null) return null;

            // 2. 创建领袖
            var leaderObject = CreateFormationMember(
                kingdomId,
                formationInfo.Id,
                1,
                config.LeaderConfigId,
                formationInfo.Position,
                formationInfo.Level
            );

            // 3. 创建士兵
            var soldiers = new List<MovableObject>();
            for (int i = 0; i < config.SoldierCount; i++)
            {
                var soldier = CreateFormationMember(
                    kingdomId,
                    formationInfo.Id,
                    i + 2,
                    config.SoldierConfigId,
                    formationInfo.Position,
                    formationInfo.Level
                );
                soldiers.Add(soldier);
            }

            // 4. 设置编队
            formationObject.SetupFormation(config.FormationType, leaderObject, soldiers);

            return formationObject;
        }

        /// <summary>
        /// 创建编队成员
        /// </summary>
        private MovableObject CreateFormationMember(long kingdomId, long formationId, int memberIndex, int configId, Vector3 position, int level)
        {
            var memberInfo = new SceneObjectInfo
            {
                Id = GenerateTemporaryObjectId(formationId, memberIndex),
                MapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(configId),
                Position = position,
                Level = level
            };

            return CreateSceneObjectInternal(kingdomId, memberInfo, isTemporary: true) as MovableObject;
        }


        /// <summary>
        /// 删除部队
        /// </summary>
        public void RemoveFormation(long kingdomId, long formationId)
        {
            if (kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjectMap))
            {
                // 获取所有相关的临时对象ID
                var temporaryIds = GetFormationMemberIds(kingdomId, formationId);

                // 移除所有成员
                foreach (var tempId in temporaryIds)
                {
                    RemoveSceneObject(kingdomId, tempId);
                }

                // 最后移除编队对象
                RemoveSceneObject(kingdomId, formationId);
            }
        }


        /// <summary>
        /// 获得给定编队的成员ID， 因为增加量是从1开始，超出后就没有了，所以可以用这个方法判断
        /// </summary>
        private List<long> GetFormationMemberIds(long kingdomId, long formationId)
        {
            var ids = new List<long>();
            // 领袖ID (index = 1)
            ids.Add(GenerateTemporaryObjectId(formationId, 1));
            // 士兵ID (index >= 2)
            for (int i = 2; i <= 100; i++)
            {
                var tempId = GenerateTemporaryObjectId(formationId, i);
                if (HasSceneObject(kingdomId, tempId))
                {
                    ids.Add(tempId);
                }
                else
                {
                    break;
                }
            }
            return ids;
        }
    }
}
