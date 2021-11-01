using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Repositories;

namespace Api.Tests.Mocks
{
    public class MockUserRepo : IUserRepository
    {
        List<User> store = new List<User>();

        public async Task Add(User entity)
        {
            await Task.Run(() => store.Add(entity));
        }

        public async Task Delete(int id)
        {
            await Task.Run(() => store.Remove(
                store.Find(x => x.Id == id)
            ));
        }

        public async Task<User> Get(int id)
        {
            return store.Find(x => x.Id == id);
        }

        public async Task<List<User>> GetAll()
        {
            return store;
        }

        public async Task Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
