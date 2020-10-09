using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Functions.Models
{
    public class MethodReference
    {
        public string MethodCalled { get; set; }
        public string MethodCaller { get; set; }
        public string MethodCallerClass { get; set; }
    }
}
