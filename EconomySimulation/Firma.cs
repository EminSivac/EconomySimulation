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

        public double Produktion;

        public double Kosten;

        public Firma(string name, double kapital, double produktion, double kosten)
        {
            Name = name;
            Kapital = kapital;
            Produktion = produktion;
            Kosten = kosten;
        }
    }
}
