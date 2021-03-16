using System;
using MongoDB.Bson;
using MongoDB.Driver;
using SlimeWorldWebAPI.DbContexts;
using SlimeWorldWebAPI.Models;
using SlimeWorldWebAPI.Repositories.Interfaces;

namespace SlimeWorldWebAPI.Repositories
{
    public abstract class AsyncRepository<T> : IAsyncRepository<T> where T: BaseEntity
    {
        public GlobalDbContext DbContext { get; set; }

        public AsyncRepository(GlobalDbContext dbContext)
        {
            this.DbContext = dbContext ?? throw new ArgumentException(nameof(dbContext));
        }

        public abstract IMongoCollection<BsonDocument> GetCollection();
    }
}
