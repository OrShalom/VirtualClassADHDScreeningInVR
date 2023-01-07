using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model
{
    public class SessionConfiguration
    {
        [JsonProperty("sessionLengthInMin")]
        public int SessionLengthInMin { get; set; } = 5;

        [JsonProperty("lettersDelayInSecInSec")]
        public int LettersDelayInSec { get; set; } = 1;

        [JsonProperty("disturbanceTimeRangeMin")]
        public int DisturbanceTimeRangeMin { get; set; } = 30;

        [JsonProperty("disturbanceTimeRangeMax")]
        public int DisturbanceTimeRangeMax { get; set; } = -1;

        [JsonProperty("amountOfShouldPress")]
        public int AmountOfShouldPress { get; set; } = 10;
    }
}
