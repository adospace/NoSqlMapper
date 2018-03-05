using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper
{
    public class NoSqlCollection<T> where T : class
    {
        public string Name { get; }
        private readonly NoSqlDatabase _dbContext;

        internal NoSqlCollection(NoSqlDatabase dbContext, string name)
        {
            Name = name;
            _dbContext = dbContext;
        }

        public void EnsureIndex([NotNull] string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            


        }

        public void Insert(T document)
        {

        }

        
    }
}
