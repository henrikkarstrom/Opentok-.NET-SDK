using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Broadcasting.Models
{
    public class StartRequestModel
    {
        public string Layout { get; set; }

        public string MaxDuration { get; set; }

        public string Resolution { get; set; }
    }
}
