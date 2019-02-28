using CarStoreBot.Integration.Interfaces;
using CarStoreBot.Integration.Interfaces.Fakes;
using CarStoreBot.Integration.Models;
using System;
using System.Collections.Generic;

namespace CarStoreBot.Factories
{
    public class TicketSearchServiceFactory
    {
        internal static Dictionary<string, Ticket> Tickets = new Dictionary<string, Ticket>
        {
            {
                "1234",
                new Ticket
                {
                    Vehicle = VehicleSearchServiceFactory.VehiclesByPlate["abc1234"],
                    Store = SearchStoreServiceFactory.Stores[1],
                    Status = "Transmission components have been replaced",
                    FinishDate = DateTime.Today.AddDays(5)
                }
            }
        };

        internal static ITicketSearchService Create()
        {
            return new StubITicketSearchService
            {
                SearchString = (ticket) => Tickets.ContainsKey(ticket) ? Tickets[ticket] : null
            };
        }
    }
}