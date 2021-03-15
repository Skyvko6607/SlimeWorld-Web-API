using System;
using MongoDB.Bson;
using MongoDB.Driver;
using SkyBlockWebAPI.DbContexts;
using SkyBlockWebAPI.Models;
using SkyBlockWebAPI.Repositories.Interfaces;

namespace SkyBlockWebAPI.Repositories
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
