using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper.Design
{
    public class Column
    {
        public string Name { get; private set; }
        public string TypeName { get; private set; }
        public bool HasIndex { get; private set; }
        public bool IsIndexUnique { get; private set; }
    }
}
