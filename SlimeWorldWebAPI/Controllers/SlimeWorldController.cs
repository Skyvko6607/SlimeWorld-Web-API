using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SkyBlockWebAPI.DTO;
using SkyBlockWebAPI.Filters;
using SkyBlockWebAPI.Services.Interfaces;

namespace SkyBlockWebAPI.Controllers
{
    [TypeFilter(typeof(ApiTokenFilter))]
    public class SlimeWorldController : ControllerBase
    {
        private ISlimeWorldService SlimeWorldService { get; set; }

        public SlimeWorldController(ISlimeWorldService slimeWorldService)
        {
            SlimeWorldService = slimeWorldService ?? throw new ArgumentException(nameof(slimeWorldService));
        }

        [HttpGet("getWorldByName")]
        public async Task<IActionResult> GetWorldByName(string worldName)
        {
            if (worldName == null)
            {
                return BadRequest();
            }

            var slimeWorld = await SlimeWorldService.GetWorldByName(worldName);
            if (slimeWorld == null)
            {
                return BadRequest();
            }

            return Ok(slimeWorld);
        }

        [HttpGet("getWorldNames")]
        public async Task<IActionResult> GetWorldNames()
        {
            return Ok(await SlimeWorldService.GetWorldNames());
        }

        [HttpGet("getBiggestWorlds")]
        public async Task<IActionResult> GetBiggestWorlds()
        {
            return Ok(await SlimeWorldService.GetBiggestWorlds());
        }

        [HttpPost("insertWorld")]
        public async Task<IActionResult> InsertWorld([FromBody] SlimeWorldDTO slimeWorldDto)
        {
            if (slimeWorldDto == null || slimeWorldDto.Name == null)
            {
                return BadRequest();
            }

            try
            {
                await SlimeWorldService.InsertWorldByNameAsync(slimeWorldDto.Name, slimeWorldDto);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("saveWorld")]
        public async Task<IActionResult> SaveWorld([FromBody] SlimeWorldDTO slimeWorldDto)
        {
            if (slimeWorldDto == null || slimeWorldDto.Name == null)
            {
                return BadRequest();
            }

            try
            {
                await SlimeWorldService.UpdateByNameAsync(slimeWorldDto.Name, slimeWorldDto);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("deleteWorld")]
        public async Task<IActionResult> DeleteWorlds(string worldName)
        {
            if (worldName == null)
            {
                return BadRequest();
            }

            try
            {
                await SlimeWorldService.DeleteWorld(worldName);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("updateLock")]
        public async Task<IActionResult> UpdateLock(string worldName, long time)
        {
            if (worldName == null)
            {
                return BadRequest();
            }

            try
            {
                await SlimeWorldService.UpdateLock(worldName, time);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("unlockWorld")]
        public async Task<IActionResult> UnlockWorld(string worldName)
        {
            if (worldName == null)
            {
                return BadRequest();
            }

            try
            {
                await SlimeWorldService.UnlockWorldAsync(worldName);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet("isWorldLocked")]
        public async Task<IActionResult> IsWorldLocked(string worldName)
        {
            if (worldName == null)
            {
                return BadRequest();
            }

            var slimeWorld = await SlimeWorldService.GetWorldByName(worldName);
            if (slimeWorld == null)
            {
                return BadRequest();
            }

            return Ok(slimeWorld.Locked);
        }
    }
}
