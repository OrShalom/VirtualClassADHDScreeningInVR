using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model
{
    public class DisturbanceMetadata
    {
        public DisturbanceMetadata(List<int> timesInSec, string type)
        {
            TimesInSec = timesInSec;
            Type = type;
        }

        public List<int> TimesInSec { get; set; }
        public string Type { get; set; }
    }
}
