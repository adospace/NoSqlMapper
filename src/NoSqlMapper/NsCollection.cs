using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
        private Dictionary<string, PropertyInfo> _documentProperties;

        private void ReflectModelType()
        {
            _documentProperties = typeof(T)
                .GetProperties()
                .Where(pi => pi.CanRead && pi.CanWrite)
                .ToDictionary(_ => _.Name, _ => _);

            var idKey = _documentProperties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "id") == 0);
            if (idKey.Key == null)
                idKey = _documentProperties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "uniqueid") == 0);
            if (idKey.Key == null)
                idKey = _documentProperties.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "objectid") == 0);

            if (idKey.Key != null &&
                _documentProperties[idKey.Key].PropertyType != typeof(int) &&
                _documentProperties[idKey.Key].PropertyType != typeof(Guid))
                throw new InvalidOperationException("Id property must be of type Guid or Int32");

            _idPropertyName = idKey.Key;
            if (_idPropertyName != null)
            {
                //default ObjectIdType.Guid
                if (_documentProperties[_idPropertyName].PropertyType == typeof(int))
                    _typeOfObjectId = ObjectIdType.Int;
            }
        }

        internal async Task EnsureTableAsync()
        {
            ReflectModelType();

            await Database.Connection.SqlDatabaseProvider.EnsureTableAsync(Database.Name, Name, _typeOfObjectId);
        }

        public async Task EnsureIndexAsync([NotNull] string field, bool unique = false, bool ascending = false)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            await Database.Connection.SqlDatabaseProvider.EnsureIndexAsync(Database.Name, Name, field, unique, ascending);
        }

        public async Task<T> InsertAsync(T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            var newlyCreatedId =
                await Database.Connection.SqlDatabaseProvider.InsertAsync(Database.Name, Name, json, GetObjectId(document));

            SetObjectId(document, newlyCreatedId);

            return document;
        }

        public async Task UpsertAsync(T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            await Database.Connection.SqlDatabaseProvider.UpsertAsync(Database.Name, Name, json, GetObjectId(document));
        }

        public async Task UpdateAsync(T document)
        {
            Validate.NotNull(document, nameof(document));

            var json = Database.Connection.JsonSerializer.Serialize(document, _idPropertyName);

            await Database.Connection.SqlDatabaseProvider.UpdateAsync(Database.Name, Name, json, GetObjectId(document));
        }

        //private NsDocument CreateNsDocument(T document)
        //{
        //    Validate.NotNull(document, nameof(document));

        //    if (document is IDictionary<string, object> dictionary)
        //    {
        //        return CreateNsDocumentFromDictionary(dictionary);
        //    }

        //    var nsDocument = CreateNsDocumentFromDictionary(
        //        _documentProperties
        //            .ToDictionary(_ => _.Key, _ => _.Value.GetValue(document)));

        //    return nsDocument;
        //}


        //private NsDocument CreateNsDocumentFromDictionary(IDictionary<string, object> sourceDictionary)
        //{
        //    var dictionary = sourceDictionary;

        //    object objectId = null;
        //    if (_idPropertyName != null)
        //    {
        //        objectId = dictionary[_idPropertyName];

        //        dictionary.Remove(_idPropertyName);

        //        if (_typeOfObjectId == typeof(Guid) &&
        //            ((Guid) objectId) == Guid.Empty)
        //            objectId = null;
        //        else if (_typeOfObjectId == typeof(int) &&
        //                 ((int) objectId) == 0)
        //            objectId = null;
        //    }

        //    return new NsDocument(dictionary) { Id = objectId };
        //}

        private object GetObjectId(T document)
        {
            object objectId = null;
            if (_idPropertyName != null)
            {
                objectId = _documentProperties[_idPropertyName].GetValue(document);

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

            _documentProperties[_idPropertyName].SetValue(document, id);
        }
    }

}
