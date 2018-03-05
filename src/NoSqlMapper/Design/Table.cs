using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper.Design
{
    public class Table
    {
        public string Name { get; private set; }

        public List<Column> Columns { get; private set; } = new List<Column>();
    }
}
