using System.Collections.Concurrent;
using SkyBlockWebAPI.Models;

namespace SkyBlockWebAPI.DataObjects
{
    public class SlimeWorldDataObject
    {
        public ConcurrentDictionary<string, SlimeWorld> SlimeWorlds { get; set; } = new ConcurrentDictionary<string, SlimeWorld>();
        public ConcurrentDictionary<string, SlimeWorld> SlimeWorldsDelayedSave { get; set; } = new ConcurrentDictionary<string, SlimeWorld>();
    }
}
