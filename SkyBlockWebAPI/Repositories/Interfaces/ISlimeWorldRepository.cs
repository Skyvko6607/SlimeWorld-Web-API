using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SkyBlockWebAPI.Models;

namespace SkyBlockWebAPI.Repositories.Interfaces
{
    public interface ISlimeWorldRepository : IAsyncRepository<SlimeWorld>
    {
        Task<IList<SlimeWorld>> GetWorlds();
        Task<IList<string>> GetWorldNames();
        Task<List<KeyValuePair<string, long>>> GetBiggestWorlds();
        Task<SlimeWorld> GetByIdAsync(ObjectId id);
        Task<SlimeWorld> GetByWorldNameAsync(string worldName);
        Task<SlimeWorld> InsertWorldByNameAsync(string worldName, byte[] worldBytes, long time);
        Task<SlimeWorld> UpdateByNameAsync(string worldName, byte[] worldBytes, long time);
        Task DeleteByNameAsync(string worldName);
        Task SaveChangesAsync();
        Task UpdateLockAsync(string worldName, long time);
        Task UnlockWorldAsync(string worldName);
    }
}
