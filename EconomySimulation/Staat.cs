using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Staat
    {
        public double Budget { get; private set; }
        public double Einkommenssteuer = 0.2; // 20% Einkommenssteuer
        public double Unternehmenssteuer = 0.15; // 15% Unternehmenssteuer
        public double Mehrwertsteuer = 0.19; // 19% Mehrwertsteuer

        public void EinkommenVersteuern(List<Mensch> personen)
        {
            foreach (var person in personen)
            {
                double steuer = person.Lohn * Einkommenssteuer;
                Budget += steuer;
                person.Geld -= steuer;
            }
        }

        public void UnternehmensVersteuern(List<Firma> firmen)
        {
            foreach (var firma in firmen)
            {
                double gewinn = firma.Kapital - firma.KapitalLetzterMonat;
                double steuer = gewinn * Unternehmenssteuer;
                firma.Kapital -= steuer;
                Budget += steuer;
            }
        }

        public void MehrwertsteuerErheben(List<Mensch> personen, Markt markt)
        {
                double umsatz = personen.Sum(p => p.Bedarf * markt.Preis);
                double steuer = umsatz * Mehrwertsteuer;
                Budget += steuer;
        }
    }
}
