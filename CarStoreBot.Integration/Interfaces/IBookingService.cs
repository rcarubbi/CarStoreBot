using CarStoreBot.Integration.Models;
using System;
using System.Collections.Generic;

namespace CarStoreBot.Integration.Interfaces
{
    public interface IBookingService
    {
        List<Appointment> SearchAppointments(string email);

        BookingResult TryBook(int storeId, DateTime desiredDateTime);
    }
}
