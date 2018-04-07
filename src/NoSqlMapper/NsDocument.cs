using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoSqlMapper
{
    public class NsDocument : Dictionary<string, object>
    {
        public object Id { get; set; }

        //public void SetObjectId(object id)
        //{
        //    Validate.NotNull(id, nameof(id));

        //    if (PropertyIdKey == null)
        //        return;

        //    if (SourceDocument is IDictionary<string, object> sourceDictionary)
        //        sourceDictionary[PropertyIdKey] = id;
        //    else
        //    {
        //        var sourceDocument = SourceDocument;
        //        sourceDocument.GetType()
        //            .GetProperty(PropertyIdKey, BindingFlags.Instance | BindingFlags.Public)
        //            ?.SetValue(sourceDocument, id);
        //    }
        //}

        //private WeakReference _sourceObjectRef;
        //public object SourceDocument
        //{
        //    get => _sourceObjectRef.Target;
        //    private set => _sourceObjectRef = new WeakReference(value);
        //}

        //public string PropertyIdKey { get; private set; }

        public NsDocument()
        {

        }

        public NsDocument(IDictionary<string, object> dictionary)
            :base(dictionary)
        {

        }

        //public static NsDocument Create<T>(T document)
        //{
        //    Validate.NotNull(document, nameof(document));

        //    if (document is IDictionary<string, object> dictionary)
        //    {
        //        return CreateFromDictionary(dictionary);
        //    }

        //    var documentProperties = document.GetType()
        //        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        //        .Where(pi => pi.CanRead && pi.CanWrite)
        //        .ToList();

        //    var nsDocument = CreateFromDictionary(
        //        documentProperties
        //            .ToDictionary(_ => _.Name, _ => _.GetValue(document)));

        //    nsDocument.SourceDocument = document;

        //    return nsDocument;
        //}

        //public static NsDocument CreateFromDictionary(IDictionary<string, object> sourceDictionary)
        //{
        //    var dictionary = sourceDictionary.ToDictionary(_ => _.Key, _ => _.Value);
        //    var idKey = dictionary.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "id") == 0);
        //    if (idKey.Key == null)
        //        idKey = dictionary.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "uniqueid") == 0);
        //    if (idKey.Key == null)
        //        idKey = dictionary.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Compare(_.Key, "objectid") == 0);

        //    object objectId = null;
        //    if (idKey.Key != null)
        //    {
        //        objectId = dictionary[idKey.Key];
                
        //        dictionary.Remove(idKey.Key);
        //    }

        //    return new NsDocument(dictionary) { Id = objectId, SourceDocument = sourceDictionary, PropertyIdKey = idKey.Key };
        //}
        
    }
}
