using Country.V1;
using GameBase;
using GameLogic.Country.Model;
using GameLogic.Country.Model.Event;
using GameLogic.Task;
using GameProto;
using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLogic.Country
{
    public sealed class CountryRepo : Singleton<CountryRepo>
    {
        MapService.MapServiceClient _client;


        protected override void Init()
        {
         //   _client = new MapService.MapServiceClient(NetClient.Instance.AuthCallInvoker);
        }

        /// <summary>
        /// 同步王国
        /// </summary>
        public async void SyncKingdoms()
        {
            var request = new SyncKingdomsRequest { };
            await NetHelper.CallStream<SyncKingdomsResponse>(
                () => _client.SyncKingdoms(request),
                response =>
                {
                    KingdomManager.Instance.SetKingdomList(response.Kingdoms);
                }
            );
        }

        /// <summary>
        /// 获得王国内所有场景对象
        /// </summary>
        /// <param name="kingdomId"></param>
        public async void ListMapObjects(long kingdomId)
        {
         //   var request = new ListMapObjectsRequest { KingdomId = kingdomId };
          //  var (code, response) = await NetHelper.Call<ListMapObjectsResponse>(_client.ListMapObjectsAsync(request));
          //  if (NetHelper.Error(code))
           //     return;

            // 清除该王国的旧数据
            SceneObjectManager.Instance.ClearKingdomObjects(kingdomId);

            // 在编辑器模式下添加模拟数据
            CreateMockMapObjects(kingdomId);
            GameModule.Timer.AddTimer(args => {
                try 
                { 
                    CreateMockCollectEvent();
                }catch (Exception e)
                {
                    Log.Error(e);
                }
            }, 2);



            // #
            // 添加新的场景对象数据
            //  foreach (var protoMapObject in response.MapObjects)
            // {
            //    var mapObject = MapObjectInfo.FromPB(protoMapObject);
            //    if (mapObject != null)
            //    {
            //        SceneObjectManager.Instance.AddSceneObject(kingdomId, mapObject);
            //     }
            // }

            Log.Debug($"加载王国[{kingdomId}]场景对象完成");
        }
        private void CreateMockMapObjects(long kingdomId)
        {
            SceneObjectManager.Instance.CurrentKingdomId = 1;
            System.Random random = new System.Random();

            // 创建两个固定位置的 Warlord
            var warlord1 = new SceneObjectInfo
            {
                Id = 1001,
                MapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(1001), // 假设1001是Warlord的实体ID
                Position = new Vector3Int(-10, 0, 0),
                Name = "Warlord_East",
                Level = 5,
                LeagueId = 1,
                LeagueName = "Eastern Legion",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var warlord2 = new SceneObjectInfo
            {
                Id = 1002,
                MapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(1001),
                Position = new Vector3Int(10, 0, 0),
                Name = "Warlord_West",
                Level = 5,
                LeagueId = 2,
                LeagueName = "Western Legion",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 添加两个 Warlord 到场景
            SceneObjectManager.Instance.AddSceneObject(kingdomId, warlord1);
            SceneObjectManager.Instance.AddSceneObject(kingdomId, warlord2);

            // 定义建筑实体ID范围
            var entityTypes = new[]
            {
                (entityId: 1001, scope: 2),    // 普通建筑 2x2
                (entityId: 1002, scope: 3),    // 大型建筑 3x3
                (entityId: 1003, scope: 1),    // 资源点 1x1
                (entityId: 1004, scope: 2),    // 怪物 2x2
                (entityId: 1005, scope: 1),    // NPC 1x1
                (entityId: 1006, scope: 1),    // 装饰物 1x1
            };

            // 创建已占用位置的集合，用于避免重叠
            HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

            // 生成30个随机对象
            for (int i = 0; i < 60; i++)
            {
                // 随机选择建筑类型
                var entityType = entityTypes[random.Next(entityTypes.Length)];

                // 尝试找到一个有效的位置
                Vector3Int position;
                bool isValidPosition;
                int attempts = 0;
                const int maxAttempts = 100;

                do
                {
                    // 随机生成位置 (-50到50的范围)
                    position = new Vector3Int(
                        random.Next(-200, 200),
                        random.Next(-100, 100),
                        0
                    );

                    // 检查这个位置是否有效（不与其他建筑重叠）
                    isValidPosition = true;
                    for (int x = 0; x < entityType.scope; x++)
                    {
                        for (int y = 0; y < entityType.scope; y++)
                        {
                            var checkPos = new Vector3Int(
                                position.x + x,
                                position.y + y,
                                0
                            );
                            if (occupiedPositions.Contains(checkPos))
                            {
                                isValidPosition = false;
                                break;
                            }
                        }
                        if (!isValidPosition) break;
                    }

                    attempts++;
                } while (!isValidPosition && attempts < maxAttempts);

                // 如果找到有效位置，创建对象
                if (isValidPosition)
                {
                    // 标记占用的格子
                    for (int x = 0; x < entityType.scope; x++)
                    {
                        for (int y = 0; y < entityType.scope; y++)
                        {
                            occupiedPositions.Add(new Vector3Int(
                                position.x + x,
                                position.y + y,
                                0
                            ));
                        }
                    }

                    // 创建场景对象
                    var mapObject = new SceneObjectInfo
                    {
                        Id = 1000 + 3 + i,
                        MapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(entityType.entityId),
                        Position = position,
                        Name = $"Building_{i + 1}",
                        Level = random.Next(1, 6),
                        LeagueId = random.Next(1, 5),  // 随机联盟ID
                        LeagueName = $"League_{random.Next(1, 5)}", // 随机联盟名称
                        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    // 检查实体是否存在
                    if (mapObject.MapObjectEntity == null)
                    {
                        Log.Error($"地图对象实体不存在: {entityType.entityId}");
                        continue;
                    }

                    // 添加到场景管理器
                  //  SceneObjectManager.Instance.AddSceneObject(kingdomId, mapObject);
                }
            }

            Log.Debug($"创建了{30}个模拟场景对象");
        }

        /// <summary>
        /// 同步地图场景对象变化
        /// </summary>
        public async void SyncChangeMapObject(long kingdomId)
        {
            var request = new SyncChangeMapObjectRequest { KingdomId = kingdomId };
            await NetHelper.CallStream<SyncChangeMapObjectResponse>(
                () => _client.SyncChangeMapObject(request),
                response =>
                {
                    if (response.KingdomId != kingdomId)
                    {
                        Log.Error($"收到错误的王国ID: 期望{kingdomId}, 实际{response.KingdomId}");
                        return;
                    }

                    foreach (var change in response.ChangeMapObjects)
                    {
                        var mapObject = SceneObjectInfo.FromPB(change.MapObject);
                        if (mapObject == null) continue;

                        switch (change.ChangeType)
                        {
                            case ChangeType.Add:
                                SceneObjectManager.Instance.AddSceneObject(kingdomId, mapObject);
                                Log.Debug($"添加场景对象: {mapObject.Id}");
                                break;

                            case ChangeType.Delete:
                                SceneObjectManager.Instance.RemoveSceneObject(kingdomId, mapObject.Id);
                                Log.Debug($"删除场景对象: {mapObject.Id}");
                                break;

                            case ChangeType.Unspecified:
                            default:
                                Log.Warning($"未知的变更类型: {change.ChangeType}");
                                break;
                        }
                    }
                }
            );
        }

        /// <summary>
        /// 同步地图事件
        /// </summary>
        public async void SyncMapEvents()
        {
            var request = new SyncMapEventsRequest { };
            await NetHelper.CallStream<SyncMapEventsResponse>(
                () => _client.SyncMapEvents(request),
                response =>
                {
                    // foreach (var mapEvent in response.MapEvents)
                    // {
                    //     MapEventManager.Instance.HandleMapEvent(mapEvent);
                    // }
                }
            );
        }


        /// <summary>
        /// 创建模拟采集事件
        /// </summary>
        public void CreateMockCollectEvent()
        {
            var mockEvent = new MapEvent
            {
                Id = 1,
                MapEventType = MapEventType.Collect,
                SourceId = 1001,
                TargetId = 1002,
                CreatedAt = 1000,
            };

            // 通过事件管理器处理事件
            MapEventManager.Instance.HandleMapEvent(mockEvent);
            //foreach (var sceneObject in SceneObjectManager.Instance.GetKingdomSceneObjects(SceneObjectManager.Instance.CurrentKingdomId).Values) {
            //    if (sceneObject.Id == long.Parse("1001")) {
            //        continue;
            //    }
            //    var mockEvent1 = new MapEvent
            //    {
            //        Id = sceneObject.Id,
            //        MapEventType = MapEventType.Collect,
            //        SourceId = sceneObject.Id,
            //        TargetId = 1001,
            //        CreatedAt = 1000,
            //    };
            //    MapEventManager.Instance.HandleMapEvent(mockEvent1);
            //}

            Log.Debug($"创建模拟采集事件: 从 {mockEvent.Id}({mockEvent.Id}) 到 ({mockEvent.TargetId})");
        }

        /// <summary>
        /// 获得对象详细信息
        /// </summary>
        /// <param name="mapObjectId"></param>
        public async void GetMapObjectInfo(long mapObjectId)
        {
            var request = new GetMapObjectInfoRequest { MapObjectId = mapObjectId };
            var (code, response) = await NetHelper.Call<GetMapObjectInfoResponse>(_client.GetMapObjectInfoAsync(request));
            if (NetHelper.Error(code))
                return;

        }
    }
}
