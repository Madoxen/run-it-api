using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Services;
using Api.Utils;
using System;

namespace Api.Tests.Mocks
{
    public class MockRouteService : ServiceBase, IRouteService
    {

        public MockRouteService(List<Route> routes, List<User> users)
        {
            usersStore = users;
            routesStore = routes;
        }

        public MockRouteService()
        {
        }

        public List<Route> routesStore { get; set; } = new List<Route>();
        public List<User> usersStore { get; set; } = new List<User>();

        public async Task<Route> GetRouteById(int id)
        {
            return routesStore.Find(x => x.Id == id);
        }

        public async Task<ServiceResult<List<Route>>> GetUserRoutes(int userId)
        {
            var user = usersStore.Find(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");
            return user.Routes;
        }

        public async Task<ServiceResult> RemoveRouteById(int id)
        {
            Route route = routesStore.Find(x => x.Id == id);
            if (route == null)
                return NotFound("User not found");
            routesStore.Remove(route);
            return Success();
        }

        public async Task<ServiceResult> RemoveRoute(Route u)
        {
            routesStore.Remove(u);
            return Success();
        }

        public async Task<ServiceResult> UpdateRoute(Route route)
        {
            int routeIndex = routesStore.FindIndex(x => x.Id == route.Id);
            if (routeIndex < 0)
                return NotFound("User not found");
            routesStore[routeIndex] = route;
            return Success();
        }

        public async Task<ServiceResult> CreateRoute(Route route)
        {
            routesStore.Add(route);
            return Success();
        }
    }
}
