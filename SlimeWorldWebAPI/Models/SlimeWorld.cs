using System;

namespace SlimeWorldWebAPI.Models
{
    public class SlimeWorld : BaseEntity
    {
        public string Name { get; set; }
        public byte[] WorldBytes { get; set; }
        public long Locked { get; set; }
        public DateTime LastUpdate { get; set; } = new DateTime();
    }
}
