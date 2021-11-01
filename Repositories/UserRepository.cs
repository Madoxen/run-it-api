using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories
{
    public interface IUserRepository : IRepository<User>
    {

    }
    public class UserRepository : EfCoreRepository<User, ApiContext>, IUserRepository
    {
        public UserRepository(ApiContext context) : base(context)
        {
            
        }
    }
}