using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper.Design
{
    public class Database
    {
        internal Database()
        {
        }

        public string Name { get; private set; }

        public List<Table> Tables { get; private set; } = new List<Table>();
        
    }
}
