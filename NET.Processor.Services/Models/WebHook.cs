using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NET.Processor.Core.Models
{
    public class WebHook
    {
        public string RepositoryURL { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
