using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace EconomySimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Firma firma1 = new Firma("HolzProdukt", 1000, 20, 7);

            Firma firma2 = new Firma("HolzProdukt2", 1500, 30, 6.5);

            List<Firma> firmen = new List<Firma>() { firma1, firma2 };

            List<Mensch> personen = new();

            for (int i = 0; i < 100; i++)
            {
                personen.Add(new Mensch
                {
                    Geld = 100,
                    Bedarf = 1,
                    PreisToleranz = 0.5
                });
            }

            Markt markt = new Markt()
            {
                BasisPreis = 50,
                Preis = 50
            };

            markt.Angebot = firmen.Sum(f => f.Produktion);

            markt.Nachfrage = personen.Sum(p =>
            {
                double kaufFaktor = 1 - (markt.Preis / markt.BasisPreis - 1) * p.PreisToleranz;

                kaufFaktor = Math.Clamp(kaufFaktor, 0, 1);
                return p.Bedarf * kaufFaktor;
            });

            AusgabeMarkt(markt);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Runde {i + 1}");

                // 1. Angebot aus Firmen
                markt.Angebot = firmen.Sum(f => f.Produktion);

                // 2. Nachfrage aus Personen
                markt.Nachfrage = personen.Sum(p =>
                {
                    double kaufFaktor =
                        1 - (markt.Preis / markt.BasisPreis - 1) * p.PreisToleranz;

                    kaufFaktor = Math.Clamp(kaufFaktor, 0, 1);
                    return p.Bedarf * kaufFaktor;
                });

                // 3. Preis anpassen
                markt.updatePrice();

                // 4. Firmen verkaufen zum neuen Preis
                FirmenVerkaufen(firmen, markt);

                // 5. Firmen reagieren
                FirmenReagieren(firmen, markt);

                AusgabeMarkt(markt);
            }

        }

        private static void AusgabeMarkt(Markt markt)
        {
            Console.WriteLine(
                $"Angebot: {Math.Round(markt.Angebot, 2)}" +
                $"\nNachfrage: {Math.Round(markt.Nachfrage, 2)}" +
                $"\nAlter Preis: {Math.Round(markt.BasisPreis, 2)}" +
                $"\nNeuer Preis: {Math.Round(markt.Preis, 2)}\n");
        }

        private static void FirmenVerkaufen(List<Firma> firmen, Markt markt)
        {
            foreach (var firma in firmen)
            {
                double einnahmen = firma.Produktion * markt.Preis;
                double kosten = firma.Produktion * firma.Kosten;

                firma.Kapital += einnahmen - kosten;
            }
        }

        private static void FirmenReagieren(List<Firma> firmen, Markt markt)
        {
            foreach (var firma in firmen)
            {
                double marktLage = markt.Nachfrage - markt.Angebot;

                if (marktLage > 0)
                    firma.Produktion *= 1.03;
                else
                    firma.Produktion *= 0.97;
            }
        }
    }
}
