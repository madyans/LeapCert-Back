using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Context;
using leapcert_back.Interfaces;

namespace leapcert_back.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;

        public TeacherRepository(ApplicationDbContext context)
        {
            context = _context;
        }

        public Task<IResponses> GetAllClasses()
        {

            return null;
        }
    }
}