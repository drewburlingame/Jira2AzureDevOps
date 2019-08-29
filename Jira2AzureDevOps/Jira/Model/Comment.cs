﻿using System;
using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class Comment
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("author")]
        public User Author { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("updateAuthor")]
        public User UpdateAuthor { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("jsdPublic")]
        public bool JsdPublic { get; set; }
    }
}