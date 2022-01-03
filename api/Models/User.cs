using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //User account
    //Enforce Uniqueness of external auth providers
    [Index(nameof(GoogleId), IsUnique = true)]
    [Index(nameof(FacebookId), IsUnique = true)]
    public class User : IEntity
    {
        [Key]
        //Internal API
        public int Id { get; set; }
        public string GoogleId { get; set; }
        public string FacebookId { get; set; }
        public float? Weight { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string LastName { get; set; }
        public uint DistanceLast30Days { get; set; }
        public uint DistanceTotal { get; set; }
        public List<Run> Runs { get; set; } //user trainings
        public List<Route> Routes { get; set; } //saved user routes 

        public List<Friend> Friends { get; set; } //saved user routes 
        public List<Friend> ReverseFriends { get; set; } //saved user routes 

    }
}