using CarStoreBot.Integration.Models;

namespace CarStoreBot.Integration.Interfaces
{
    public interface ITicketSearchService
    {
        Ticket Search(string ticketNumber);
    }
}
