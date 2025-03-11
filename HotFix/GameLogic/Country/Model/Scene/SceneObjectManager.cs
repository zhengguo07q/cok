using GameBase;
using GameBase.Scene;
using GameLogic.Country.Model;
using GameLogic.Country.View;
using GameProto;
using System.Collections.Generic;
using TEngine;

namespace GameLogic.Country
{
    public class SceneObjectManager : Singleton<SceneObjectManager>
    {
        // 按王国ID组织场景对象: <KingdomId, <ObjectId, MapObjectInfo>>
        private Dictionary<long, Dictionary<long, SceneObjectInfo>> _kingdomSceneObjects 
            = new Dictionary<long, Dictionary<long, SceneObjectInfo>>();

        /// <summary>
        /// 当前玩家所在的王国ID
        /// </summary>
        public long CurrentKingdomId { get; set; }


        /// <summary>
        /// 同步当前王国的场景对象
        /// </summary>
        public void SyncCurrentKingdomObjects()
        {
            if (CurrentKingdomId <= 0)
            {
                Log.Error("当前王国ID无效");
                return;
            }

            SyncKingdomObjects(CurrentKingdomId);
        }

        /// <summary>
        /// 添加场景对象
        /// </summary>
        public void AddSceneObject(long kingdomId, SceneObjectInfo mapObject)
        {
            if (!_kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects))
            {
                sceneObjects = new Dictionary<long, SceneObjectInfo>();
                _kingdomSceneObjects[kingdomId] = sceneObjects;
            }

            sceneObjects[mapObject.Id] = mapObject;
            
            // 创建场景对象实例
            var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
            if (scene != null)
            {
                scene.SceneObjectLayer.CreateSceneObject(kingdomId, mapObject);
            }
        }

        /// <summary>
        /// 更新场景对象
        /// </summary>
        public void UpdateSceneObject(long kingdomId, SceneObjectInfo mapObject)
        {
            if (_kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects))
            {
                sceneObjects[mapObject.Id] = mapObject;
                
                // 更新场景对象实例
                var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
                if (scene != null)
                {
                    scene.SceneObjectLayer.UpdateSceneObject(kingdomId, mapObject);
                }
            }
        }

        /// <summary>
        /// 移除场景对象
        /// </summary>
        public void RemoveSceneObject(long kingdomId, long mapObjectId)
        {
            if (_kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects) 
                && sceneObjects.Remove(mapObjectId))
            {
                // 移除场景对象实例
                var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
                if (scene != null)
                {
                    scene.SceneObjectLayer.RemoveSceneObject(kingdomId, mapObjectId);
                }
            }
        }

        /// <summary>
        /// 清除指定王国的所有场景对象
        /// </summary>
        public void ClearKingdomObjects(long kingdomId)
        {
            if (_kingdomSceneObjects.Remove(kingdomId))
            {
                // 清除场景对象实例
                var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
                if (scene != null)
                {
                    scene.SceneObjectLayer.ClearKingdomObjects(kingdomId);
                }
            }
        }

        /// <summary>
        /// 获取场景对象
        /// </summary>
        public SceneObjectInfo GetSceneObject(long kingdomId, long mapObjectId)
        {
            return _kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects) 
                && sceneObjects.TryGetValue(mapObjectId, out var mapObject) 
                ? mapObject 
                : null;
        }

        /// <summary>
        /// 获取指定王国的所有场景对象
        /// </summary>
        public IReadOnlyDictionary<long, SceneObjectInfo> GetKingdomSceneObjects(long kingdomId)
        {
            return _kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects) 
                ? sceneObjects 
                : new Dictionary<long, SceneObjectInfo>();
        }

        /// <summary>
        /// 同步指定王国的所有场景对象到当前场景
        /// </summary>
        public void SyncKingdomObjects(long kingdomId)
        {
            if (!_kingdomSceneObjects.TryGetValue(kingdomId, out var sceneObjects))
            {
                Log.Warning($"王国[{kingdomId}]没有场景对象数据");
                return;
            }

            var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
            if (scene == null)
            {
                Log.Error("当前不在国家场景中");
                return;
            }

            // 先清除当前场景中该王国的所有对象
            scene.SceneObjectLayer.ClearKingdomObjects(kingdomId);

            // 重新创建所有对象
            foreach (var mapObject in sceneObjects.Values)
            {
                scene.SceneObjectLayer.CreateSceneObject(kingdomId, mapObject);
            }

            Log.Debug($"同步王国[{kingdomId}]的{sceneObjects.Count}个场景对象到当前场景");
        }

        /// <summary>
        /// 同步所有王国的场景对象到当前场景
        /// </summary>
        public void SyncAllKingdomObjects()
        {
            var scene = SceneSwitchManager.Instance.GetCurrentScene<CountryScene>();
            if (scene == null)
            {
                Log.Error("当前不在国家场景中");
                return;
            }

            foreach (var kingdomId in _kingdomSceneObjects.Keys)
            {
                SyncKingdomObjects(kingdomId);
            }

            Log.Debug($"同步了{_kingdomSceneObjects.Count}个王国的场景对象");
        }

        private void NotifySceneUpdate(long kingdomId, SceneObjectInfo mapObject) { }
        private void NotifySceneRemove(long kingdomId, long mapObjectId) { }
        private void NotifySceneClear(long kingdomId) { }
    }
}