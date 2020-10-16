using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models
{
    public class Filter
    {
        public List<string> Projects { get; set; } = new List<string>();
        public List<string> Documents { get; set; } = new List<string>();
        public List<string> Methods { get; set; } = new List<string>();
    }
}
