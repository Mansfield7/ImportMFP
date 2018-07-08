using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportMFP.Classes
{
    public static class Importer
    {
        public static MFPWeightData ImportFromFile(string fileName)
        {
            MFPWeightData weightData = new MFPWeightData();

            weightData.WeightEntries.Add(new MFPWeightEntry(DateTime.Now.AddDays(2), 175.5));

            return weightData;
        }
    }
}
