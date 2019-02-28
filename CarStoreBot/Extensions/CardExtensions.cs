using AdaptiveCards;
using CarStoreBot.Integration.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace CarStoreBot.Extensions
{
    public static class CardExtensions
    {
        public static void AddAppointmentCard(this IMessageActivity instance, string nome, List<Appointment> appointments)
        {
            var facts = new List<AdaptiveCards.Fact>();

            foreach (var item in appointments)
            {
                facts.Add(new AdaptiveCards.Fact
                {
                    Title = $"{item.Vehicle.Model} {item.Vehicle.Color} - Plate: {item.Vehicle.Plate}",
                    Value = $"{item.DateTime.ToShortDateString()} at {item.DateTime.ToShortTimeString()} on store {item.Store.Name}"
                });
            }

            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock
                    {
                        Text = "Pending appointments", Weight = TextWeight.Bolder, Size = TextSize.Medium
                    },
                    new TextBlock {Text = $"**{nome}**", Wrap = true},
                    new Container
                    {
                        Items = new List<CardElement>()
                        {
                            new TextBlock
                            {
                                Text = "You have the following scheduled revisions:", Wrap = true
                            },
                            new FactSet {Facts = facts}
                        }
                    }
                }
            };

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            instance.Attachments.Add(attachment);
        }

        public static void AddWarrantyCard(this IMessageActivity instance, Vehicle vehicle)
        {
            var card = new AdaptiveCard();
            card.Body.Add(
                new Container
                {
                    Items = new List<CardElement> {
                        new TextBlock
                        {
                            Text = "Right! I found it. Your vehicle is included in the following warranty programs",
                            Weight = TextWeight.Bolder,
                            Size = TextSize.Medium
                        },
                        new Container
                        {
                            Separation = SeparationStyle.Strong,
                        }

                    }
                });


            foreach (var warranty in vehicle.Warranties)
            {
                ((card.Body[0] as Container)?.Items[1] as Container)?.Items.Add(AddWarranty(warranty));
            }

            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            instance.Attachments.Add(attachment);
        }

        private static CardElement AddWarranty(Warranty warranty)
        {
            var factSet = new FactSet
            {
                Separation = SeparationStyle.Strong,
                Facts = new List<AdaptiveCards.Fact>
                {
                    new AdaptiveCards.Fact
                    {
                        Title = "Title:",
                        Value = warranty.Title
                    }
                }
            };
            var firstPart = true;
            foreach (var part in warranty.InsuredParts)
            {
                factSet.Facts.Add(new AdaptiveCards.Fact
                {
                    Title = firstPart ? "Insured Parts:" : string.Empty,
                    Value = $"* {part}"
                });
                if (firstPart) firstPart = false;
            }

            var firstDiagnostic = true;
            foreach (var diagnostic in warranty.Diagnostics)
            {
                factSet.Facts.Add(new AdaptiveCards.Fact
                {
                    Title = firstDiagnostic ? "Diagnostic:" : string.Empty,
                    Value = diagnostic
                });
                if (firstDiagnostic) firstDiagnostic = false;
            }

            factSet.Facts.Add(new AdaptiveCards.Fact
            {
                Title = "Guarantee:",
                Value = warranty.Guarantee
            });

            return factSet;
        }


    }
}