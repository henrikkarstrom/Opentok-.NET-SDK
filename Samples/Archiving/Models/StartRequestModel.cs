using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archiving.Models
{
    public class StartRequestModel
    {
        [JsonProperty("hasAudio")]
        public string HasAudio { get; set; }

        [JsonProperty("hasVideo")]
        public string HasVideo { get; set; }

        [JsonProperty("outputMode")]
        public string OutputMode { get; set; }
    }
}
