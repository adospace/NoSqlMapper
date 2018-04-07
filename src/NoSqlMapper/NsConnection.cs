using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public class NsConnection : IDisposable
    {
        private ISqlDatabaseProvider _sqlDatabaseProvider;
        public ISqlDatabaseProvider SqlDatabaseProvider
        {
            get => _sqlDatabaseProvider ?? throw new InvalidOperationException("SqlDatabaseProvider provider not specified");
            set
            {
                if (_sqlDatabaseProvider != null)
                    throw new InvalidOperationException("SqlDatabaseProvider provider already set");

                _sqlDatabaseProvider = value;
            }
        }

        private IJsonSerializer _jsonProvider;
        public IJsonSerializer JsonSerializer
        {
            get => _jsonProvider ?? throw new InvalidOperationException("JsonSerializer serializer not specified");
            set
            {
                if (_jsonProvider != null)
                    throw new InvalidOperationException("JsonSerializer already set");

                _jsonProvider = value;
            }
        }

        public ILogProvider LoggerProvider { get; set; }

        public async Task<NsDatabase> GetDatabaseAsync(string databaseName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));

            await SqlDatabaseProvider.EnsureConnectionAsync();

            await SqlDatabaseProvider.CreateDatabaseIfNotExistsAsync(databaseName);

            return new NsDatabase(this, databaseName);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    SqlDatabaseProvider?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NsConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
