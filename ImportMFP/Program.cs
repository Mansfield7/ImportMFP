using ImportMFP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportMFP
{
    class Program
    {
        static void Main(string[] args)
        {
            MFPWeightData weightData = Importer.ImportFromFile("data.csv");

            new Exporter().Export(weightData);
        }
    }
}
