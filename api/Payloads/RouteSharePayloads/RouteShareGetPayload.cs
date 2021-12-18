using System;
using Api.Models;

namespace Api.Payloads
{
    public class RouteShareGetPayload : IModelPayload<RouteShare>
    {
        public int RouteId { get; set; }
        public int OwnedById { get; set; }
        public int SharedToId { get; set; }
        public string OwnerGivenName { get; set; }
        public string OwnerLastName { get; set; }
        public DateTimeOffset DateShared { get; set; }

        public RouteShareGetPayload()
        { }

        public RouteShareGetPayload(RouteShare model)
        {
            RouteId = model.RouteId;
            OwnedById = model.Route.UserId;
            SharedToId = model.SharedToId;
            OwnerGivenName = model.Route.User.GivenName;
            OwnerLastName = model.Route.User.LastName;
        }

        public virtual RouteShare CreateModel()
        {
            return new RouteShare()
            {
                RouteId = RouteId,
                SharedToId = SharedToId,
                Date = DateShared
            };
        }
    }
}