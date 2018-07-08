using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportMFP.Classes
{
    public class MFPWeightData
    {
        public IList<MFPWeightEntry> WeightEntries { get; set; }

        public MFPWeightData()
        {
            WeightEntries = new List<MFPWeightEntry>();
        }
    }

    public class MFPWeightEntry
    {
        public DateTime EntryDate { get; set; }

        public double Weight { get; set; }

        public MFPWeightEntry(DateTime entryDate, double weight)
        {
            EntryDate = EntryDate;
            Weight = weight;
        }

    }
}
