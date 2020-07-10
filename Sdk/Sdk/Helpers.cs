using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Security;
using Newtonsoft.Json.Linq;

using Stratumn.Sdk.Model.Client;
using Stratumn.Sdk.Model.Misc;
using Stratumn.CanonicalJson.Helpers;
using Stratumn.Chainscript.utils;

namespace Stratumn.Sdk
{
    public static class Helpers
    {
        public static Int64 GetTime()
        {
            Int64 retval = 0;
            var st = new DateTime(1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
            retval = (Int64)(t.TotalMilliseconds + 0.5);
            return retval;
        }
        public static Endpoints MakeEndpoints(Endpoints endpoints)
        {
            if (endpoints == null)
            {
                string ACCOUNT_RELEASE_URL = "https://account-api.stratumn.com";
                string TRACE_RELEASE_URL = "https://trace-api.stratumn.com";
                string MEDIA_RELEASE_URL = "https://media-api.stratumn.com";
                return new Endpoints(ACCOUNT_RELEASE_URL, TRACE_RELEASE_URL, MEDIA_RELEASE_URL);
            }
            if (endpoints.Account == null || endpoints.Trace == null || endpoints.Media == null)
            {
                throw new InvalidParameterException("The provided endpoints argument is not valid.");
            }
            return endpoints;
        }
        /// <summary>
        /// 
        /// Extract all filewrappers from some data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>

        public static Dictionary<string, Property<FileWrapper>> ExtractFileWrappers(object data)
        {
            return ExtractObjects<FileWrapper>(
                data,
                (X) => FileWrapper.isFileWrapper(X), (Y) =>
                {
                    return FileWrapper.FromObject((object)Y);
                }
            );
        }

        /// <summary>
        /// Extract all file records from some data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, Property<FileRecord>> ExtractFileRecords(object data)
        {
            return ExtractObjects<FileRecord>(
                data,
                (X) => FileRecord.IsFileRecord(X), (Y) =>
                {
                    return FileRecord.FromObject((object)Y);
                }
            );
        }
        private static Regex KeyIndex = new Regex("\\[(\\d+)\\]|\\((\\d+)\\)$");
        private static int extractIndexFromKey(string key)
        {
            //get the index of the value in the array or collection
            //read the index from the path  
            int index;
            string value = KeyIndex.Match(key).Groups[1].Value;
            if (value != null)
            {
                index = int.Parse(value);
            }
            else
            {
                throw new TraceSdkException("Index not found on path " + key);
            }
            return index;
        }
        /// <summary>
        /// Replaces on object with another based on the path provided both objects extend Identifiable interface
        /// fieds/arrays are defined using that interface
        /// </summary>
        /// <param name="fileRecordList"></param>

        public static void AssignObjects<V>(IList<Property<V>> propertyList) where V : Identifiable
        {
            foreach (Property<V> propertyElement in propertyList)
            {
                object parent = propertyElement.Parent;
                //read the key name from the path
                string[] pathElements = propertyElement.Path.Split('.');
                string key = pathElements[pathElements.Length - 1];
                if (parent is JContainer)
                {
                    if (parent is JObject)
                    {
                        ((JObject)parent)[key] = JsonHelper.ObjectToObject<JObject>(propertyElement.Value);
                    }
                    else if (parent is JArray)
                    {
                        int index = extractIndexFromKey(key);
                        ((JArray)parent).Insert(index, JsonHelper.ObjectToObject<JContainer>(propertyElement.Value));
                    }
                }
                else if (parent is System.Collections.IDictionary)
                {
                    ((IDictionary<string, object>)parent)[key] = propertyElement.Value;
                }
                else
                {
                    if (parent.GetType().IsArray)
                    {
                        int index = extractIndexFromKey(key);
                        //object could be an identifiable or it could be a deserialized map
                        if (typeof(System.Collections.IDictionary).IsAssignableFrom(parent.GetType().GetElementType()))
                        { //convert the value to map~                                              
                            IDictionary<string, object> map = JsonHelper.ObjectToMap(propertyElement.Value);
                            ((Array)parent).SetValue(map, index);
                        }
                        else
                        {
                            ((Array)parent).SetValue(propertyElement.Value, index);
                        }
                    }
                    else
                    {
                        if (parent is object)
                        {
                            try
                            {
                                //write the object to the field                                
                                System.Reflection.FieldInfo field = parent.GetType().GetTypeInfo().GetDeclaredField(key);

                                if (!field.FieldType.IsAssignableFrom(propertyElement.Value.GetType()))
                                {
                                    throw new TraceSdkException("Field " + key + " of type " + field.FieldType + " is not assignable from " + propertyElement.Value.GetType());
                                }
                                field.SetValue(parent, propertyElement.Value);
                            }
                            catch (Exception e)
                            {
                                throw new TraceSdkException("Failed to set one or more fields on the data object", e);
                            }
                        }
                    }
                }
            }
        }

        private static void ExtractObjectsImpl<  V>(object data, object parent, string path, IDictionary<string, Property<V>> idToObjectMap, System.Predicate<object> predicate, System.Func<object, V> reviver) where V : Identifiable
        {
            if (data == null)
            {
                return;
            }
            // if the predicate is true, then this data should be extracted
            if (predicate(data))
            {
                // apply reviver if provided to generate new Data
                V newData = reviver != null ? reviver(data) : (V)data;
                // add a new entry to the idToObject map
                idToObjectMap[newData.GetId()] = new Property<V>(newData.GetId(), newData, path, parent);
            }
            else if (data is JContainer)
            {
                if (data is JObject)
                { //loop over subelements
                    foreach (var element in ((JObject)data))
                    {
                        ExtractObjectsImpl<  V>(element.Value, data, path.Length == 0 ? element.Key : path + "." + element.Key, idToObjectMap, predicate, reviver);
                    }
                }
                else if (data is JArray)
                { //loop over indexes .
                    int idx = 0;
                    foreach (var value in ((JArray)data))
                    {
                        ExtractObjectsImpl<V>(value, data, path + "[" + idx + "]", idToObjectMap, predicate, reviver);
                        idx++;
                    }
                }
            }
            else if (data.GetType().IsArray )
            {
                // if it is an array, iterate through each element  extract objects recursively
                int idx = 0;
                foreach (var value in (object[])data)
                {
                    ExtractObjectsImpl(value, data, path + "[" + idx + "]", idToObjectMap, predicate, reviver);
                    idx++;
                }
            }
            else if (data is System.Collections.IDictionary)
            {
                foreach (KeyValuePair<string, object> element in ((IDictionary<string, object>)data).SetOfKeyValuePairs())
                {
                    ExtractObjectsImpl(element.Value, data, path.Length == 0 ? element.Key : path + "." + element.Key, idToObjectMap, predicate, reviver);
                }
            }
            else if (typeof(System.Collections.ICollection).IsAssignableFrom(data.GetType()))
            {
                ICollection col = (ICollection)data;
                // if it is a collection, iterate through each element extract objects recursively
                int idx = 0;
                foreach (var value in col)
                {
                    ExtractObjectsImpl(value, data, path + "(" + idx + ")", idToObjectMap, predicate, reviver);
                    idx++;
                }
            }
            else
            {
                // if it is an object, iterate through each entry
                // and extract objects recursively
                Type clazz = data.GetType();

                //if (data is object && !data.GetType().FullName.StartsWith("System.", StringComparison.Ordinal))
                if (clazz.IsClass && !data.GetType().Namespace.StartsWith("System"))
                {

                    System.Reflection.FieldInfo[] fields = clazz.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.FieldType.IsPrimitive || field.FieldType.IsValueType || field.FieldType.IsEnum || field.IsStatic)
                        {
                            continue;
                        }
                        object value = field.GetValue(data);
                   
                        ExtractObjectsImpl(value, data, path.Length == 0 ? field.Name : path + "." + field.Name, idToObjectMap, predicate, reviver);
                    }
                }
            }
        }


        private static Dictionary<String, Property<V>> ExtractObjects<V>(object data, Predicate<object> predicate, Func<object, V> reviver) where V : Identifiable
        {
            // create a new idToObject map
            Dictionary<String, Property<V>> idToObjectMap = new Dictionary<String, Property<V>>();

            // call the implementation
            ExtractObjectsImpl(data, null, "", idToObjectMap, predicate, reviver);
            // return the maps
            return idToObjectMap;
        }

        public static string GetMimeType(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case "bmp": return "image/bmp";
                case "csv": return "text/csv";
                case "doc": return "application/msword";
                case "docm": return "application/vnd.ms-word.document.macroEnabled.12";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "dot": return "application/msword";
                case "eml": return "message/rfc822";
                case "gif": return "image/gif";
                case "gz": return "application/x-gzip";
                case "ico": return "image/x-icon";
                case "jpeg": return "image/jpeg";
                case "jpg": return "image/jpeg";
                case "json": return "application/json";
                case "msg": return "application/vnd.ms-outlook";
                case "odt": return "application/vnd.oasis.opendocument.text";
                case "one": return "application/onenote";
                case "pages": return "application/octet-stream";
                case "pdf": return "application/pdf";
                case "png": return "image/png";
                case "pps": return "application/vnd.ms-powerpoint";
                case "ppsx": return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                case "ppt": return "application/vnd.ms-powerpoint";
                case "pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "rar": return "application/x-rar-compressed";
                case "rtf": return "application/rtf";
                case "tar": return "application/x-tar";
                case "tgz": return "application/x-compressed";
                case "tif": return "image/tiff";
                case "tiff": return "image/tiff";
                case "txt": return "text/plain";
                case "xls": return "application/vnd.ms-excel";
                case "xlsm": return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xml": return "text/xml";
                case "xps": return "application/vnd.ms-xpsdocument";
                case "zip": return "application/zip";
                case "zipx": return "application/zip";
            }
            return "application/unknown";
        }
    }
}
