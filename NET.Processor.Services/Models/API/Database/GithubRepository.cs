using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.API.Database
{
    public class GithubRepository
    {
        public ObjectId Id { get; set; }
        public Github github;

        public class Github
        {
            public Profile profile;
            public string token;
        }
        public class Profile
        {
            public ObjectId Id { get; set; }
            public string provider;
        }
    }
}
