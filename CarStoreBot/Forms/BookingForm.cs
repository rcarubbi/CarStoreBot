using CarStoreBot.Factories;
using CarStoreBot.Integration.Models;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Threading.Tasks;

namespace CarStoreBot.Forms
{
    [Serializable]
    public class BookingForm
    {
        [Prompt("Inform the desired date (dd/MM/yyyy)")]
        public DateTime DesiredDate { get; set; }

        [Prompt("Now inform the best time for you (HH:mm)")]
        public DateTime DesiredTime { get; set; }

        public Store Store { get; set; }

        [Prompt("Now type the plate of your vehicle")]
        public string Plate { get; set; }

        public Vehicle Vehicle { get; set; }

        public static IForm<BookingForm> BuildForm()
        {
            var form = new FormBuilder<BookingForm>()
                .Field(nameof(Plate), validate: (state, plate) =>
                {
                    var result = new ValidateResult();

                    var service = VehicleSearchServiceFactory.Create();
                    state.Vehicle = service.SearchByPlate(plate.ToString());
                    if (state.Vehicle == null)
                    {
                        result.Feedback = "I couldn't find your vehicle... type the plate again please";
                        result.IsValid = false;
                    }
                    else
                    {
                        result.Value = plate.ToString();
                        result.IsValid = true;
                    }
                    return Task.FromResult(result);
                })
                   .Field(nameof(DesiredDate), 
                    active: (a) => a.DesiredDate == default(DateTime), 
                    validate: (state, desiredDate) =>
                    {
                        var result = new ValidateResult();
                        var date = Convert.ToDateTime(desiredDate);
                        if (date.Date <= DateTime.Today)
                        {
                            result.Feedback = "You have to book at least one day before, please select another date";
                            result.IsValid = false;
                        }
                        else
                        {
                            result.Value = desiredDate;
                            result.IsValid = true;
                        }
                        return Task.FromResult(result);
                    })
                   .Field(nameof(DesiredDate), 
                    active: (a) => a.DesiredTime == default(DateTime), 
                    validate: (state, desiredTime) =>
                    {
                        var result = new ValidateResult();
                        var time = Convert.ToDateTime(desiredTime);
                        if (time.Minute != 0 && time.Minute != 30)
                        {
                            result.Feedback = "You can schedule only every half hour, i.e. 10:00 or 10:30";
                            result.IsValid = false;
                        }
                        else if (time < DateTime.Today.AddHours(9) || time > DateTime.Today.AddHours(17).AddMinutes(30))
                        {
                            result.Feedback = "You can schedule just between the business time from 09:00 AM to 5:30 PM";
                            result.IsValid = false;
                        }
                        else
                        {
                            result.Value = desiredTime;
                            result.IsValid = true;
                        }
                        return Task.FromResult(result);
                    })
                   .Build();

            return form;
        }
    }
}