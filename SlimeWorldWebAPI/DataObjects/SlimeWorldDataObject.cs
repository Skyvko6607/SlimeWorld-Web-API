using System.Collections.Concurrent;
using SlimeWorldWebAPI.Models;

namespace SlimeWorldWebAPI.DataObjects
{
    public class SlimeWorldDataObject
    {
        public ConcurrentDictionary<string, SlimeWorld> SlimeWorlds { get; set; } = new ConcurrentDictionary<string, SlimeWorld>();
        public ConcurrentDictionary<string, SlimeWorld> SlimeWorldsDelayedSave { get; set; } = new ConcurrentDictionary<string, SlimeWorld>();
        public ConcurrentDictionary<string, long> SlimeWorldsProcessing { get; set; } = new ConcurrentDictionary<string, long>();
    }
}
