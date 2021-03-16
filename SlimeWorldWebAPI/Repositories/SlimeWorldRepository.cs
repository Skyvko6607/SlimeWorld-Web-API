using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SlimeWorldWebAPI.Configs;
using SlimeWorldWebAPI.DataObjects;
using SlimeWorldWebAPI.DbContexts;
using SlimeWorldWebAPI.Exceptions;
using SlimeWorldWebAPI.Models;
using SlimeWorldWebAPI.Repositories.Interfaces;

namespace SlimeWorldWebAPI.Repositories
{
    public class SlimeWorldRepository : AsyncRepository<SlimeWorld>, ISlimeWorldRepository
    {
        private readonly SlimeWorldDataObject _slimeWorldDataObject;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMongoCollection<BsonDocument> _collection;

        public SlimeWorldRepository(GlobalDbContext dbContext,
            SlimeWorldDataObject slimeWorldDataObject,
            IOptions<AppSettings> appSettings) : base(dbContext)
        {
            _slimeWorldDataObject = slimeWorldDataObject ?? throw new ArgumentException(nameof(slimeWorldDataObject));
            _appSettings = appSettings ?? throw new ArgumentException(nameof(appSettings));
            _collection = DbContext.MongoDatabase.GetCollection<BsonDocument>("worlds");
        }

        public async Task<IList<SlimeWorld>> GetWorlds()
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var query = await DbContext.GetAllByQueryAsync(collection, new BsonDocument());
            if (query == null)
            {
                return null;
            }

            var bucket = new GridFSBucket(DbContext.MongoDatabase, new GridFSBucketOptions
            {
                BucketName = "worlds"
            });

            var slimeWorlds = new List<SlimeWorld>();

            foreach (var document in query)
            {
                var name = document["name"].AsString;
                try
                {
                    var bytes = await bucket.DownloadAsBytesByNameAsync(name);
                    var slimeWorld = new SlimeWorld
                    {
                        Id = document["_id"].AsObjectId,
                        Name = name,
                        WorldBytes = bytes,
                        Locked = document["locked"].AsInt64
                    };
                    slimeWorlds.Add(slimeWorld);

                    _slimeWorldDataObject.SlimeWorlds.AddOrUpdate(name, s => slimeWorld, (s, world) => slimeWorld);
                }
                catch
                {
                    Console.WriteLine("Failed to get world " + name);
                }
            }

            return slimeWorlds;
        }

        public async Task<List<KeyValuePair<string, long>>> GetBiggestWorlds()
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var query = await DbContext.GetAllByQueryAsync(collection, new BsonDocument());
            if (query == null)
            {
                return null;
            }

            var bucket = new GridFSBucket(DbContext.MongoDatabase, new GridFSBucketOptions
            {
                BucketName = "worlds"
            });

            var sizeMap = new Dictionary<string, long>();
            var docs = await bucket.FindAsync(new BsonDocument());
            await docs.ForEachAsync(info =>
            {
                if (!sizeMap.ContainsKey(info.Filename))
                {
                    sizeMap.Add(info.Filename, info.Length);
                }
            });

            var sortedMap = sizeMap.ToList();
            sortedMap.Sort((pair, valuePair) => valuePair.Value.CompareTo(pair.Value));
            return sortedMap;
        }

        public async Task<IList<string>> GetWorldNames()
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var result = await collection.FindAsync(new BsonDocument());
            var worldNames = new List<string>();
            await result.ForEachAsync(document => { worldNames.Add(document["name"].AsString); });

            return worldNames;
        }

        public async Task<SlimeWorld> GetByIdAsync(ObjectId id)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var slimeWorld = _slimeWorldDataObject.SlimeWorlds.Values.FirstOrDefault(world => world.Id == id);
            if (slimeWorld != null)
            {
                slimeWorld.LastUpdate = new DateTime();
                return slimeWorld;
            }

            var query = await DbContext.GetOneByQueryAsync(collection, new BsonDocument("_id", id));
            if (query == null)
            {
                return null;
            }

            return await GetByWorldNameAsync(query["name"].AsString);
        }

        public async Task<SlimeWorld> GetByWorldNameAsync(string worldName)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var slimeWorld = _slimeWorldDataObject.SlimeWorlds.FirstOrDefault(pair => pair.Key == worldName).Value;
            if (slimeWorld != null)
            {
                slimeWorld.LastUpdate = new DateTime();
                return slimeWorld;
            }

            var query = await DbContext.GetOneByQueryAsync(collection, new BsonDocument("name", worldName));
            if (query == null)
            {
                throw new UnknownWorldException(worldName);
            }

            var bucket = new GridFSBucket(DbContext.MongoDatabase, new GridFSBucketOptions
            {
                BucketName = "worlds"
            });
            var bytes = await bucket.DownloadAsBytesByNameAsync(worldName);

            slimeWorld = new SlimeWorld
            {
                Id = query["_id"].AsObjectId,
                Name = query["name"].AsString,
                WorldBytes = bytes,
                Locked = query["locked"].AsInt64
            };

            _slimeWorldDataObject.SlimeWorlds.AddOrUpdate(worldName, s => slimeWorld, (s, world) => slimeWorld);
            return slimeWorld;
        }

        public async Task<SlimeWorld> InsertWorldByNameAsync(string worldName, byte[] worldBytes, long time)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            if (_slimeWorldDataObject.SlimeWorlds.FirstOrDefault(pair => pair.Key == worldName).Value != null)
            {
                throw new WorldAlreadyExistException(worldName);
            }

            var query = await DbContext.GetOneByQueryAsync(collection, new BsonDocument("name", worldName));
            if (query != null)
            {
                throw new WorldAlreadyExistException(worldName);
            }

            var slimeWorld = new SlimeWorld
            {
                Name = worldName,
                WorldBytes = worldBytes,
                Locked = time
            };

            _slimeWorldDataObject.SlimeWorlds.AddOrUpdate(worldName, s => slimeWorld, (s, world) => slimeWorld);
            _slimeWorldDataObject.SlimeWorldsDelayedSave.AddOrUpdate(worldName, s => slimeWorld,
                (s, world) => slimeWorld);

            return slimeWorld;
        }

        public async Task<SlimeWorld> UpdateByNameAsync(string worldName, byte[] worldBytes, long time)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return null;
            }

            var slimeWorld = _slimeWorldDataObject.SlimeWorlds.FirstOrDefault(pair => pair.Key == worldName).Value;
            if (slimeWorld == null)
            {
                var query = await DbContext.GetOneByQueryAsync(collection, new BsonDocument("name", worldName));
                if (query == null)
                {
                    return await InsertWorldByNameAsync(worldName, worldBytes, time);
                }

                var bucket = new GridFSBucket(DbContext.MongoDatabase, new GridFSBucketOptions
                {
                    BucketName = "worlds"
                });
                var bytes = await bucket.DownloadAsBytesByNameAsync(worldName);

                slimeWorld = new SlimeWorld
                {
                    Id = query["_id"].AsObjectId,
                    Name = query["name"].AsString,
                    WorldBytes = bytes,
                    Locked = query["locked"].AsInt64
                };
            }

            slimeWorld.LastUpdate = new DateTime();

            slimeWorld.WorldBytes = worldBytes;

            _slimeWorldDataObject.SlimeWorlds.AddOrUpdate(worldName, s => slimeWorld, (s, world) => slimeWorld);
            _slimeWorldDataObject.SlimeWorldsDelayedSave.AddOrUpdate(worldName, s => slimeWorld,
                (s, world) => slimeWorld);

            return slimeWorld;
        }

        public async Task DeleteByNameAsync(string worldName)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return;
            }

            await DbContext.DeleteOneAsync(collection, new BsonDocument("name", worldName));
            _slimeWorldDataObject.SlimeWorlds.TryRemove(worldName, out _);
            _slimeWorldDataObject.SlimeWorldsDelayedSave.TryRemove(worldName, out _);
        }

        public async Task SaveChangesAsync()
        {
            DateTime date = new DateTime();
            date = date.AddMinutes(10);
            foreach (var pair in new ConcurrentDictionary<string, SlimeWorld>(_slimeWorldDataObject.SlimeWorlds))
            {
                var lastUpdate = pair.Value.LastUpdate;
                if (date.CompareTo(lastUpdate) == -1)
                {
                    _slimeWorldDataObject.SlimeWorlds.TryRemove(pair.Key, out _);
                }
            }

            Console.WriteLine("!!!!!!!!!!!!! Saving worlds... !!!!!!!!!!!!!");
            if (_slimeWorldDataObject.SlimeWorldsDelayedSave.IsEmpty)
            {
                Console.WriteLine("World list empty");
                return;
            }

            var collection = GetCollection();
            if (collection == null)
            {
                Console.WriteLine("Collection null");
                return;
            }

            try
            {
                var bucket = new GridFSBucket(DbContext.MongoDatabase, new GridFSBucketOptions
                {
                    BucketName = "worlds"
                });

                var worldNames = _slimeWorldDataObject.SlimeWorldsDelayedSave.Keys.ToList();

                var docs = await bucket.FindAsync(new BsonDocument("filename",
                    new BsonDocument("$in", new BsonArray(worldNames))));

                var infoList = new List<GridFSFileInfo>();
                await docs.ForEachAsync(info => infoList.Add(info));

                infoList.ForEach(async info =>
                {
                    try
                    {
                        await bucket.RenameAsync(info.Id, info.Filename + "_backup", CancellationToken.None);
                        await bucket.DeleteAsync(info.Id, CancellationToken.None);
                    }
                    catch
                    {
                        // ignored
                    }
                });

                var slimeWorlds = _slimeWorldDataObject.SlimeWorldsDelayedSave.Values.ToList();
                foreach (var slimeWorld in slimeWorlds)
                {
                    try
                    {
                        await bucket.UploadFromBytesAsync(slimeWorld.Name, slimeWorld.WorldBytes);

                        var queryFind =
                            await DbContext.GetOneByQueryAsync(collection, new BsonDocument("name", slimeWorld.Name));
                        if (queryFind == null)
                        {
                            await DbContext.InsertAsync(collection, new BsonDocument("name", slimeWorld.Name)
                            {
                                new BsonElement("locked", slimeWorld.Locked)
                            });
                        }
                        else if (DateTime.Now.Millisecond - slimeWorld.Locked > _appSettings.Value.WorldMaxLockTime)
                        {
                            await DbContext.UpdateAsync(collection, new BsonDocument("name", slimeWorld.Name),
                                new BsonDocument("name", slimeWorld.Name)
                                {
                                    new BsonElement("locked", slimeWorld.Locked)
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Failed to update files for " + slimeWorld.Name);
                    }
                }


                infoList.ForEach(async info =>
                {
                    try
                    {
                        await bucket.DeleteAsync(info.Filename + "_backup", CancellationToken.None);
                    }
                    catch
                    {
                        // ignored
                    }
                });

                _slimeWorldDataObject.SlimeWorldsDelayedSave.Clear();
                Console.WriteLine("Saved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task UpdateLockAsync(string worldName, long time)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return;
            }

            var slimeWorldDto = await GetByWorldNameAsync(worldName);
            if (slimeWorldDto == null)
            {
                return;
            }

            var slimeWorldObj = _slimeWorldDataObject.SlimeWorlds.FirstOrDefault(pair => pair.Key == worldName);
            if (slimeWorldObj.Value == null)
            {
                return;
            }

            var slimeWorld = slimeWorldObj.Value;
            slimeWorld.Locked = time;
            slimeWorld.LastUpdate = new DateTime();
        }

        public async Task UnlockWorldAsync(string worldName)
        {
            var collection = GetCollection();
            if (collection == null)
            {
                return;
            }

            var slimeWorld = await GetByWorldNameAsync(worldName);
            if (slimeWorld == null)
            {
                return;
            }

            slimeWorld.Locked = 0;
        }

        public override IMongoCollection<BsonDocument> GetCollection()
        {
            return _collection;
        }
    }
}