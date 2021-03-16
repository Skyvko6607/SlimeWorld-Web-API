using System.Collections.Generic;
using System.Threading.Tasks;
using SlimeWorldWebAPI.DTO;

namespace SlimeWorldWebAPI.Services.Interfaces
{
    public interface ISlimeWorldService
    {
        Task<SlimeWorldDTO> GetWorldByName(string worldName);
        Task InsertWorldByNameAsync(string worldName, SlimeWorldDTO slimeWorld);
        Task UpdateByNameAsync(string worldName, SlimeWorldDTO slimeWorld);
        Task DeleteWorld(string worldName);
        Task UpdateLock(string worldName, long time);
        Task<IList<string>> GetWorldNames();
        Task<List<KeyValuePair<string, long>>> GetBiggestWorlds();
        Task UnlockWorldAsync(string worldName);
    }
}
