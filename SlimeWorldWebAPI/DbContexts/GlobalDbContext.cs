using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SlimeWorldWebAPI.Configs;

namespace SlimeWorldWebAPI.DbContexts
{
    public class GlobalDbContext
    {
        private MongoClient MongoClient { get; set; }

        public IMongoDatabase MongoDatabase { get; set; }

        private readonly IOptions<AppSettings> _appSettings;

        public GlobalDbContext(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentException(nameof(appSettings));
            MongoClient = new MongoClient(new MongoUrl(_appSettings.Value.MongoDbConnectionString));
            MongoDatabase = MongoClient.GetDatabase(_appSettings.Value.MongoDatabase);
        }

        public async Task<BsonDocument> GetOneByQueryAsync(IMongoCollection<BsonDocument> collection, BsonDocument query) =>
            (await collection.FindAsync(query)).FirstOrDefault();

        public async Task<IList<BsonDocument>> GetAllByQueryAsync(IMongoCollection<BsonDocument> collection, BsonDocument query, int limit = 0) =>
            await (await collection.FindAsync(query, new FindOptions<BsonDocument>
            {
                Limit = limit
            })).ToListAsync();

        public async Task InsertAsync(IMongoCollection<BsonDocument> collection, BsonDocument document) =>
            await collection.InsertOneAsync(document);

        public async Task UpdateAsync(IMongoCollection<BsonDocument> collection, BsonDocument query, BsonDocument document) =>
            await collection.UpdateOneAsync(query,
                new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument("$set", document)));

        public async Task DeleteOneAsync(IMongoCollection<BsonDocument> collection, BsonDocument query) =>
            await collection.DeleteOneAsync(query);

        public async Task DeleteManyAsync(IMongoCollection<BsonDocument> collection, BsonDocument query) =>
            await collection.DeleteManyAsync(query);
    }
}