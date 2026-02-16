using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Firma
    {
        public string Name;

        public double Kapital;

        public double Produktion => Mitarbeiter.Count * 0.5;

        public double Kosten => Mitarbeiter.Count * 0.5;

        public List<Mensch> Mitarbeiter = new();

        public double LohnProMitarbeiter = 20;

        public Firma(string name, double kapital)
        {
            Name = name;
            Kapital = kapital;
        }

        public double BerechneProduktion()
        {
            return Mitarbeiter.Count * 2;
        }
    }
}
