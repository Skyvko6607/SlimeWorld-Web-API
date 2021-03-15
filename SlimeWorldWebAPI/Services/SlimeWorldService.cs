using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyBlockWebAPI.DTO;
using SkyBlockWebAPI.Repositories.Interfaces;
using SkyBlockWebAPI.Services.Interfaces;

namespace SkyBlockWebAPI.Services
{
    public class SlimeWorldService : ISlimeWorldService
    {
        private ISlimeWorldRepository SlimeWorldRepository { get; set; }

        public SlimeWorldService(ISlimeWorldRepository slimeWorldRepository)
        {
            SlimeWorldRepository = slimeWorldRepository ?? throw new ArgumentException(nameof(slimeWorldRepository));
        }

        public async Task<SlimeWorldDTO> GetWorldByName(string worldName)
        {
            var slimeWorld = await SlimeWorldRepository.GetByWorldNameAsync(worldName);
            if (slimeWorld == null)
            {
                return null;
            }

            return new SlimeWorldDTO
            {
                Name = slimeWorld.Name,
                WorldBytes = Convert.ToBase64String(slimeWorld.WorldBytes),
                Locked = slimeWorld.Locked
            };
        }

        public async Task InsertWorldByNameAsync(string worldName, SlimeWorldDTO slimeWorld) =>
            await SlimeWorldRepository.InsertWorldByNameAsync(worldName,
                Convert.FromBase64String(slimeWorld.WorldBytes), slimeWorld.Locked);

        public async Task UpdateByNameAsync(string worldName, SlimeWorldDTO slimeWorld) =>
            await SlimeWorldRepository.UpdateByNameAsync(worldName, Convert.FromBase64String(slimeWorld.WorldBytes),
                slimeWorld.Locked);

        public async Task DeleteWorld(string worldName) => await SlimeWorldRepository.DeleteByNameAsync(worldName);

        public async Task UpdateLock(string worldName, long time) =>
            await SlimeWorldRepository.UpdateLockAsync(worldName, time);

        public async Task<IList<string>> GetWorldNames() =>
            await SlimeWorldRepository.GetWorldNames();

        public async Task<List<KeyValuePair<string, long>>> GetBiggestWorlds() =>
            await SlimeWorldRepository.GetBiggestWorlds();

        public async Task UnlockWorldAsync(string worldName) => await SlimeWorldRepository.UnlockWorldAsync(worldName);
    }
}