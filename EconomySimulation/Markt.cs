using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomySimulation
{
    public class Markt
    {
        public double Angebot { get; set; }
        public double Nachfrage { get; set; }
        public double Preis { get; set; }

        public double BasisPreis { get; set; }

        public void updatePrice()
        {
            double zielPreis = BasisPreis * (Nachfrage / Math.Max(Angebot, 0.1));
            Preis += (zielPreis - Preis) * 0.4;
            //BasisPreis = Preis;
        }
    }
}
