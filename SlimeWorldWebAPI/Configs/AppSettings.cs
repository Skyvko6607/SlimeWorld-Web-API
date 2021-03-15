namespace SkyBlockWebAPI.Configs
{
    public class AppSettings
    {
        public string MongoDbConnectionString { get; set; }
        public string MongoDatabase { get; set; }
        public long WorldMaxLockTime { get; set; }
        public long WorldLockTime { get; set; }
        public string ApiToken { get; set; }
    }
}
