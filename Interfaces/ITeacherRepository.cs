using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Dtos.Class;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Interfaces
{
    public interface ITeacherRepository
    {
        Task<IResponses> GetAllClasses(int id);
        Task<IResponses> CreateClass([FromBody] CreateClassDto dto);
    }
}