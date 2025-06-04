using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers
{
    [Route("api/general")]
    [ApiController]
    public class GeneralControler : ControllerBase
    {
        private readonly IGeneralRespository _generalRepository;
        public GeneralControler(IGeneralRespository generalRespository)
        {
            _generalRepository = generalRespository;
        }

        [Authorize]
        [HttpGet("getAllGenders")]
        public async Task<IActionResult> GetGenders()
        {
            var result = await _generalRepository.GetGenders();

            if (!result.Flag) ResponseHelper.HandleError(this, result);

            return Ok(result);
        }

    }
}