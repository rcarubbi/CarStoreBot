using CarStoreBot.Integration.Interfaces;
using CarStoreBot.Integration.Models;
using System;
using System.Collections.Generic;

namespace CarStoreBot.Factories
{
    public class AppointmentServiceFactory
    {
        internal static IBookingService Create()
        {
            return new CarStoreBot.Integration.Interfaces.Fakes.StubIBookingService()
            {
                SearchAppointmentsString = (email) =>
                {
                    if (email == "rcarubbi@gmail.com")
                    {
                        return new List<Appointment> {
                            new Appointment
                            {
                                DateTime = new DateTime(2017,10,10 ,10,0, 0),
                                Store = new Store
                                {
                                    Name = "Carrera Chevrolet - Villa Lobos",
                                    Address = "Rua Henri Bouchard, 77 - Vila Leopoldina, São Paulo - SP, 05319-070",
                                    Phones = "(11) 4002-1515",
                                    Latitude = -23.5399084,
                                    Longitude = -46.7303012
                                },
                                Vehicle = new Vehicle
                                {
                                    Color = "Preto",
                                    Model = "Agile",
                                    Plate = "ABC-1234"
                                }
                            }
                        };
                    }
                    else
                        return new List<Appointment>();
                },
                TryBookInt32DateTime = (storeId, desiredDateTime) =>
                {
                    if (desiredDateTime == new DateTime(2019, 10, 10, 10, 0, 0))
                    {
                        return new BookingResult
                        {

                            Success = false,
                            SuggestedTimes = new List<DateTime>
                            {
                                new DateTime(2017,10,09,16,0,0),
                                new DateTime(2017,10,09,17,0,0),
                                new DateTime(2017,10,10,09,0,0),
                                new DateTime(2017,10,10,11,0,0),
                                new DateTime(2017,10,10,13,0,0)
                            }
                        };
                    }
                    else
                    {
                        return new BookingResult
                        {
                            Success = true
                        };
                    }
                }
            };
        }
    }
}