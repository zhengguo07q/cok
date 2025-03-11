using System.Collections.Generic;
using Country.V1;
using GameBase;
using GameProto;
using TEngine;

namespace GameLogic.Country.Model.Event
{
    public class MapEventManager : Singleton<MapEventManager>
    {
        private Dictionary<float, MapEventBase> _activeEvents = new Dictionary<float, MapEventBase>();

        public void HandleMapEvent(MapEvent mapEvent)
        {
            switch (mapEvent.MapEventType)
            {
                case MapEventType.Collect:
                    ProcessFormationMove(mapEvent);
                    break;
                // 其他事件类型的处理...
                default:
                    Log.Warning($"未处理的事件类型: {mapEvent.MapEventType}");
                    break;
            }
        }

        private void ProcessFormationMove(MapEvent mapEvent)
        {
            var formationMoveEvent = new FormationMoveEvent
            {
                EventId = mapEvent.Id,
                SourceObjectId = mapEvent.SourceId,
                TargetObjectId = mapEvent.TargetId,
                MapEventType = mapEvent.MapEventType,
                Duration = mapEvent.CreatedAt
            };

            // 如果已存在相同ID的事件，先取消它
            if (_activeEvents.TryGetValue(mapEvent.Id, out var existingEvent))
            {
                existingEvent.Cancel();
                _activeEvents.Remove(mapEvent.Id);
            }

            // 处理新事件
            formationMoveEvent.Process();
            _activeEvents.Add(mapEvent.Id, formationMoveEvent);
        }

        public void CancelEvent(float eventId)
        {
            if (_activeEvents.TryGetValue(eventId, out var mapEvent))
            {
                mapEvent.Cancel();
                _activeEvents.Remove(eventId);
            }
        }

        public void ClearAllEvents()
        {
            foreach (var mapEvent in _activeEvents.Values)
            {
                mapEvent.Cancel();
            }
            _activeEvents.Clear();
        }
    }
} 