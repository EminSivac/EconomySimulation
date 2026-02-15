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

        public void updatePrice()
        {
            Preis *= 1 + (Nachfrage - Angebot) / Angebot * 0.1;
        }

    }
}
