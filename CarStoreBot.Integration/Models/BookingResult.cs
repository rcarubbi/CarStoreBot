using System;
using System.Collections.Generic;

namespace CarStoreBot.Integration.Models
{
    public class BookingResult
    {
        public bool Success { get; set; }

        public List<DateTime> SuggestedTimes { get; set; }
    }
}
