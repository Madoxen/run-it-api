using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    //User account
    public class Friend
    {
        public DateTimeOffset Date { get; set; }
        public int RequesterId { get; set; }
        public User Requester { get; set; }

        public int ReceiverId { get; set; }
        public User Receiver { get; set; }

        public AcceptanceStatus Status { get; set; }
    }

    public enum AcceptanceStatus
    {
        Requested = 0,
        Friends = 1,
    }
}