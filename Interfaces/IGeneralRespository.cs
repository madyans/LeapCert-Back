using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Interfaces
{
    public interface IGeneralRespository
    {
        Task<IResponses> GetGenders();
    }
}