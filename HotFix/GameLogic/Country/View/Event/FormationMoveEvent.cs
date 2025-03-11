using UnityEngine;
using System;
using TEngine;
using GameBase.Scene;
using GameLogic.Country.View;
using GameLogic.Country.View.Object;
using GameLogic.Country.View.Animation;
using GameLogic.Country.View.Formation;
using GameLogic.Country.View.AI;
using GameLogic.Country.View.AI.Formation;

namespace GameLogic.Country.Model.Event
{
    /// <summary>
    /// 采集事件
    /// </summary>
    public class FormationMoveEvent : MapEventBase
    {
        private string PathId => $"{MapEventType}_{EventId}";

        private FormationObject formation;

        private SceneObjectInfo sourceObject;
        private SceneObjectInfo targetObject;

        private bool isMoving;

        public override void Process()
        {
            var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
            if (scene == null)
            {
                Log.Error("当前不在国家场景中");
                return;
            }

            var sceneObjectManager = SceneObjectManager.Instance;
            sourceObject = sceneObjectManager.GetSceneObject(sceneObjectManager.CurrentKingdomId, SourceObjectId);
            targetObject = sceneObjectManager.GetSceneObject(sceneObjectManager.CurrentKingdomId, TargetObjectId);

            if (sourceObject == null || targetObject == null)
            {
                Log.Error($"找不到资源事件相关的场景对象: 源[{SourceObjectId}] 目标[{TargetObjectId}]");
                return;
            }
            
            // 1. 创建移动路径
            scene.PathLayer.CreatePath(
                PathId,
                sourceObject.Position,
                targetObject.Position
            );

            scene.PathLayer.SetPathColor(PathId, GetResourcePathColor());
            scene.PathLayer.SetPathSpeed(PathId, 1.0f);


            // 1. 创建编队信息
            var formationInfo = new SceneObjectInfo
            {
                Id = EventId,
                MapObjectEntity = ConfigSystem.Instance.Tables.TbMapObject.Get(1007), // 使用编队的配置
                Position = sourceObject.Position,
                Name = $"Formation_{EventId}",
                Level = 1,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 2. 创建编队配置
            var formationConfig = new FormationConfig
            {
                FormationType = AnimationDeploy.FormationType.Triangle,  // 三角形阵型
                LeaderConfigId = 2001,                  // 领袖配置ID
                SoldierConfigId = 2002,                 // 士兵配置ID
                SoldierCount = 2                        // 士兵数量
            };

            // 3. 创建编队对象
            formation = scene.SceneObjectLayer.CreateFormation(
                SceneObjectManager.Instance.CurrentKingdomId,
                formationInfo,
                formationConfig
            );

            if (formation != null)
            {
                // 使用HTN任务系统异步驱动编队移动
                isMoving = true;
                
                // 创建任务链：移动到目标 -> 停留2秒 -> 返回起点
                formation.AddTask(new FormationMoveTask(formation, targetObject.Position));
                formation.AddTask(new ActionTask(formation, 2.0f));
                formation.AddTask(new FormationMoveTask(formation, sourceObject.Position));
                
                Log.Info($"Formation {EventId} started moving with HTN tasks");
            }
        }


        private void OnMoveComplete()
        {
            // 移动完成后的处理
            Log.Info($"Formation {EventId} reached target position");

            // 可以在这里触发其他事件或回调
        }

        public void Update()
        {
            if (formation != null && isMoving)
            {
                // 检查任务是否全部完成
                if (formation.GetComponent<MovableObject>().IsTaskQueueEmpty())
                {
                    isMoving = false;
                    OnMoveComplete();
                }
            }
        }

        public override void Cancel()
        {
            if (formation != null)
            {
                var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
                if (scene != null)
                {
                    scene.PathLayer.RemovePath(PathId);
                    // 移除整个编队（包括领袖和士兵）
                    scene.SceneObjectLayer.RemoveFormation(
                        SceneObjectManager.Instance.CurrentKingdomId,
                        EventId
                    );
                }
                formation = null;
            }
        }

        private Color GetResourcePathColor()
        {
            return Color.white;
        }
    }
} 