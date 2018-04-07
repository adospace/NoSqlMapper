using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper
{
    public interface ILogProvider
    {
        void Log(string message);
    }
}
