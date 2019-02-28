using Microsoft.Bot.Builder.FormFlow;
using System;

namespace CarStoreBot.Forms
{
    [Serializable]
    public class VehicleForm
    {
        [Prompt("Please, what's your vehicle's chassis.")]
        public string Chassis { get; set; }

        [Prompt("Please, what's your vehicle's plate.")]
        public string Plate { get; set; }


        internal static IForm<VehicleForm> BuildPlate()
        {
            return new FormBuilder<VehicleForm>()
                .Field(nameof(Plate))
                .Build();
        }

        internal static IForm<VehicleForm> BuildChassis()
        {
            return new FormBuilder<VehicleForm>()
                .Field(nameof(Chassis))
                .Build();
        }
    }
}