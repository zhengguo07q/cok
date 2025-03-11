using Country.V1;

namespace GameLogic.Country.Model.Event
{
    public abstract class MapEventBase
    {
        public long EventId { get; set; }
        public float Duration { get; set; }
        public long SourceObjectId { get; set; }
        public long TargetObjectId { get; set; }
        public MapEventType MapEventType { get; set; }

        public abstract void Process();
        public abstract void Cancel();

        
    }
} 