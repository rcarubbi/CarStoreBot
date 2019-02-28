using CarStoreBot.Integration.Interfaces;
using CarStoreBot.Integration.Interfaces.Fakes;
using CarStoreBot.Integration.Models;
using System.Collections.Generic;

namespace CarStoreBot.Factories
{
    public class VehicleSearchServiceFactory
    {
        internal static Dictionary<string, Vehicle> VehiclesByPlate = new Dictionary<string, Vehicle>()
        {
            {
                "abc1234",
                new Vehicle()
                {
                    Color = "Black",
                    Model = "Agile",
                    Plate = "ABC-1234",
                    Chassis = "9BFZD5596EB684794",
                }
            },
            {
                "def4567",
                new Vehicle()
                {
                    Color = "Silver",
                    Model = "Cruze",
                    Plate = "DEF-4567",
                    Chassis = "9BFZD5596EB684793",
                    Warranties = new List<Warranty>
                    {
                        new Warranty()
                        {
                            Title = "BTSA 034/15 14M01 - Troca se necessário",
                            InsuredParts = new List<string>
                            {
                                "Jogo da embreagem da caixa de mudanças (DP66)",
                                "Jogo de vedação da transmissão",
                                "Fluído para transmissão",
                                "Placa traseira do volante do motor (motor 1.6)",
                            },
                            Diagnostics = new List<string>
                            {
                                "Para reparos decorrentes de vazamento dos retentores do eixo de entrada",
                                    "Outro diagnóstico analisar EG*"
                            },
                            Guarantee =  "More than 2 years or 160k KM (what happens first)"
                        },
                            new Warranty
                        {
                            Title = "BTSA 033/16 14M02  - Troca se necessário",
                            InsuredParts = new List<string>
                            {
                                "Módulo de Controle da Transmissão (TCM)"
                            },
                            Diagnostics = new List<string>
                            {
                                "Quando identificado pelo DN intermitência gradual de comunicação entre o módulo de controle TCM e a transmissão"
                            },
                            Guarantee =  "Mais 7 anos ou 240 mil (o que primeiro ocorrer)"
                        },
                        new Warranty
                        {
                            Title = "BTSA 034/16 15B22 - Reprogramação",
                            InsuredParts = new List<string>
                            {
                                "Módulo de Controle da Transmissão (TCM)"
                            },
                            Diagnostics = new List<string>
                            {
                                "Quando identificado pelo DN intermitência gradual de comunicação entre o módulo de controle TCM e a transmissão"
                            },
                            Guarantee =  "Mais 7 anos ou 240 mil (o que primeiro ocorrer)"
                        },
                    }
                }
            }
        };

        internal static Dictionary<string, Vehicle> VehiclesByChassis = new Dictionary<string, Vehicle>()
        {
            {"9BFZD5596EB684794", VehiclesByPlate["abc1234"] },
            {"9BFZD5596EB684793", VehiclesByPlate["def4567"] }
        };

        internal static IVechicleSearchService Create()
        {
            return new StubIVechicleSearchService()
            {
                SearchByPlateString = (plate) =>
                {
                    var key = plate.Replace("-", string.Empty).ToLower();
                    return VehiclesByPlate.ContainsKey(key) ? VehiclesByPlate[key] : null;
                },
                SearchByChassisString = (chassis) => VehiclesByChassis.ContainsKey(chassis) ? VehiclesByChassis[chassis] : null
            };
        }
    }
}
