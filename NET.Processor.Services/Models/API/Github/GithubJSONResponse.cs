using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.API
{
    public class GithubJSONResponse
    {
        public class Root 
        {
            public Items[] items;
        }

        public class Items
        {
            public string html_url { get; set; }
        }
    }
}
