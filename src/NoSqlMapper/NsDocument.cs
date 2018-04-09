using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoSqlMapper
{
    public class NsDocument
    {
        public object Id { get; private set; }

        public string Json { get; private set; }

        public NsDocument(object id, string json)
        {
            Validate.NotNull(id, nameof(id));
            Validate.NotNullOrEmptyOrWhiteSpace(json, nameof(json));

            Id = id;
            Json = json;
        }
    }
}
