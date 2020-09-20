using System.Threading.Tasks;

namespace NETProcessor.API.Data
{
    public class SolutionRepository : ISolutionRepository
    {
        private readonly Context _context;

        public SolutionRepository(Context context)
        {
            _context = context;
        }


    }
}
