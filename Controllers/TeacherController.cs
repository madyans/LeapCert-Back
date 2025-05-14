using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Dtos.Class;
using leapcert_back.Helper;
using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private ITeacherRepository _teacherRepository;

        public TeacherController(ITeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;
        }

        [Authorize]
        [HttpGet("getAllClasses/{id}")]
        public async Task<IActionResult> GetAllClasses(int id)
        {
            var result = await _teacherRepository.GetAllClasses(id);

            if (!result.Flag) return ResponseHelper.HandleError(this, result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("createClass")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassDto dto)
        {
            var result = await _teacherRepository.CreateClass(dto);

            if (!result.Flag) return ResponseHelper.HandleError(this, result);

            return Ok(result);
        }
    }
}