using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //User account
    public class FriendRequest
    {
        public DateTimeOffset Date { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int FriendUserId { get; set; }
        public User FriendUser { get; set; }
    }
}