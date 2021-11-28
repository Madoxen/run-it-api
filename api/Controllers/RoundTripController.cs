using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Handlers;
using Api.Payloads;
using System.Linq;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoundTripController : ControllerBase
    {
        [HttpGet]
        [Route("{long}&{lat}")]
        public async Task<MapPoint[]> Get(double longitude, double latitude)
        {
            return new MapPoint[]
            {
                new MapPoint(
                18.656330108642578,
                50.29251976074081
                ),
                new MapPoint(
                18.66100788116455,
                50.2892569959687
                ),
                new MapPoint(
                18.66525650024414,
                50.292108500193386
                ),
                new MapPoint(
                18.65903377532959,
                50.29405510204365
                )
            };
        }

    }
}
