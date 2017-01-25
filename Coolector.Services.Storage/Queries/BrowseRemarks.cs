﻿using System.Collections.Generic;
using Coolector.Common.Types;

namespace Coolector.Services.Storage.Queries
{
    public class BrowseRemarks : PagedQueryBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
        public string ResolverId { get; set; }
        public string State { get; set; }
        public bool Latest { get; set; }
        public bool Disliked { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}