using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Staat
    {
        public double Budget { get; set; }
        public double Einkommenssteuer { get; set; }
        public double Unternehmenssteuer { get; set; }
        public double Mehrwertsteuer { get; set; }

        public double Sozialhilfe { get; set; }

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
                if (gewinn <= 0) continue;

                double steuer = gewinn * Unternehmenssteuer;
                firma.Kapital -= steuer;
                Budget += steuer;
            }
        }

        public void BudgetErhoehen(double steuer)
        {
            Budget += steuer;
        }

        public void SozialhilfeAnArbeitslose(List<Mensch> personen)
        {
            personen.Where(p => p.Arbeitgeber == null).ToList().ForEach(p =>
            {
                p.Geld += Sozialhilfe;
                Budget -= Sozialhilfe;
            });
        }
    }
}
