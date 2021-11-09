using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public List<Run> Runs { get; set; }
    }
}