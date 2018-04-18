using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NoSqlMapper.Query;

namespace NoSqlMapper
{
    public class NsCollection<T> where T : class
    {
        public NsDatabase Database { get; }
        public string Name { get; }

        internal NsCollection(NsDatabase database, string name)
        {
            Database = database;
            Name = name;
        }

        private string _idPropertyName;
        private ObjectIdType _typeOfObjectId;
        private TypeReflector _typeReflector;

        private void ReflectModelType()
        {
            _typeReflector = TypeReflector.Create<T>();

            var idKey = _typeReflector.Properties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "id") == 0);
            if (idKey.Key == null)
                idKey = _typeReflector.Properties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "uniqueid") == 0);
            if (idKey.Key == null)
                idKey = _typeReflector.Properties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "objectid") == 0);

            if (idKey.Key != null &&
                _typeReflector.Properties[idKey.Key].PropertyType != typeof(int) &&
                _typeReflector.Properties[idKey.Key].PropertyType != typeof(Guid))
                throw new InvalidOperationException("Id property must be of type Guid or Int32");

            _idPropertyName = idKey.Key;
            if (_idPropertyName != null)
            {
                //default ObjectIdType.Guid
                if (_typeReflector.Properties[_idPropertyName].PropertyType == typeof(int))
                    _typeOfObjectId = ObjectIdType.Int;
            }
        }

        internal async Task EnsureTableAsync()
        {
            ReflectModelType();

            await Database.Connection.SqlDatabaseProvider.EnsureTableAsync(Database, Name, _typeOfObjectId);
        }

        [NotNull]
        public async Task EnsureIndexAsync([NotNull] string field, bool unique = false, bool ascending = false)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            await Database.Connection.SqlDatabaseProvider.EnsureIndexAsync(Database, Name, field, unique, ascending);
        }

        [ItemNotNull]
        public async Task<T> InsertAsync([NotNull] T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            var newlyCreatedId =
                await Database.Connection.SqlDatabaseProvider.InsertAsync(Database, Name, json, GetObjectId(document));

            SetObjectId(document, newlyCreatedId);

            return document;
        }

        [NotNull]
        public async Task UpsertAsync([NotNull] T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            await Database.Connection.SqlDatabaseProvider.UpsertAsync(Database, Name, json, GetObjectId(document));
        }

        [NotNull]
        public async Task UpdateAsync([NotNull] T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            await Database.Connection.SqlDatabaseProvider.UpdateAsync(Database, Name, json, GetObjectId(document));
        }

        [ItemNotNull]
        public async Task<T[]> FindAsync([NotNull] Query.Query query, SortDescription[] sorts = null, int skip = 0,
            int take = int.MaxValue)
        {
            Validate.NotNull(query, nameof(query));

            var documents =
                await Database.Connection.SqlDatabaseProvider.FindAsync(Database, Name, _typeReflector, query, sorts, skip, take);

            return documents.Select(_ =>
            {
                var documentObject = Database.Connection.JsonSerializer.Deserialize<T>(_.Json);
                SetObjectId(documentObject, _.Id);
                return documentObject;
            }).ToArray();
        }

        [ItemCanBeNull]
        public async Task<T> FindAsync(object id)
        {
            Validate.NotNull(id, nameof(id));

            var document =
                await Database.Connection.SqlDatabaseProvider.FindAsync(Database, Name, id);

            if (document == null)
                return null;

            var documentObject = Database.Connection.JsonSerializer.Deserialize<T>(document.Json);
            SetObjectId(documentObject, document.Id);
            return documentObject;
        }

        [NotNull]
        public async Task<int> CountAsync([NotNull] Query.Query query)
        {
            Validate.NotNull(query, nameof(query));

            return await Database.Connection.SqlDatabaseProvider.CountAsync(Database, Name, _typeReflector, query);
        }

        private object GetObjectId(T document)
        {
            object objectId = null;
            if (_idPropertyName != null)
            {
                objectId = _typeReflector.Properties[_idPropertyName].GetValue(document);

                if (_typeOfObjectId == ObjectIdType.Guid &&
                    ((Guid)objectId) == Guid.Empty)
                    objectId = null;
                else if (_typeOfObjectId == ObjectIdType.Int &&
                         ((int)objectId) == 0)
                    objectId = null;
            }

            return objectId;
        }

        private void SetObjectId(T document, object id)
        {
            if (_idPropertyName == null)
                return;

            _typeReflector.Properties[_idPropertyName].SetValue(document, id);
        }
    }

}
