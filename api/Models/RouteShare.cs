using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //User account
    public class RouteShare
    {
        public DateTimeOffset Date { get; set; }
        public Route Route { get; set; }
        public int RouteId { get; set; }
        public User SharedTo { get; set; }
        public int SharedToId { get; set; }
    }

}