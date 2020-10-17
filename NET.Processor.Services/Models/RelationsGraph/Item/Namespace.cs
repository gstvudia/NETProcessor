﻿using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Namespace : Item
    {
        public Namespace(int id, string name, TextSpan span) : base(id, name, span)
        {
        }
    }
}
