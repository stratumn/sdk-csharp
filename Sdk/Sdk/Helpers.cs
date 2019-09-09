using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Security;
using Stratumn.Sdk.Model.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Stratumn.Sdk.Model.Misc;
using Newtonsoft.Json.Linq;
using System.Collections;
using Stratumn.CanonicalJson.Helpers;
using System.Reflection;
using System.Text.RegularExpressions;
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
                string ACCOUNT_RELEASE_URL = "https://account.stratumn.com";
                string TRACE_RELEASE_URL = "https://trace.stratumn.com";
                string  MEDIA_RELEASE_URL = "https://media.stratumn.com";
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

        public static Dictionary<string, Property<FileWrapper>> ExtractFileWrappers<T>(T data)
        {
            return ExtractObjects<T, FileWrapper>(data, (X) => FileWrapper.isFileWrapper(X), (Y) =>
            {
                return FileWrapper.FromObject((object)Y);
            });
        }

        /// <summary>
        /// Extract all file records from some data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, Property<FileRecord>> ExtractFileRecords<T>(T data)
        {

            return ExtractObjects<T, FileRecord>(data, (X) => FileRecord.IsFileRecord(X), (Y) =>
            {
                return FileRecord.FromObject((object)Y);
            });
        }


        private static Regex KeyIndex = new Regex("\\[(\\d+)\\]|\\((\\d+)\\)$");

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private static int extractIndexFromKey(String key) throws com.stratumn.sdk.TraceSdkException
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
                          //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
                          //ORIGINAL LINE: java.util.Map<?,?> map = com.stratumn.chainscript.utils.JsonHelper.objectToMap(propertyElement.getValue());
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
                                //in java there is currently no way of changing the type of a field. A field has to be of type identifiable
                                System.Reflection.FieldInfo field = parent.GetType().GetTypeInfo().GetDeclaredField(key);

                                if (field.FieldType.IsAssignableFrom(typeof(System.Collections.IDictionary)))
                                { //convert the value to map~

                                }
                                else
                                {
                                    if (!propertyElement.Value.GetType().IsAssignableFrom(field.FieldType))
                                    {
                                        throw new TraceSdkException("Field " + key + " of type " + field.FieldType + " is not assignable from " + propertyElement.Value.GetType());
                                    }
                                    field.SetValue(parent, propertyElement.Value);
                                }
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



        private static void ExtractObjectsImpl<T, V>(dynamic data, object parent, string path, IDictionary<string, Property<V>> idToObjectMap, System.Predicate<T> predicate, System.Func<T, V> reviver) where V : Identifiable
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
                        ExtractObjectsImpl<T, V>(element.Value, data, path.Length == 0 ? element.Key : path + "." + element.Key, idToObjectMap, predicate, reviver);
                    }
                }
                else if (data is JArray)
                { //loop over indexes .
                    int idx = 0;
                    foreach (var value in ((JArray)data))
                    {
                        ExtractObjectsImpl<T, V>(value, data, path + "[" + idx + "]", idToObjectMap, predicate, reviver);
                        idx++;
                    }
                }
            }
            else if (data.GetType().IsArray)
            {
                // if it is an array, iterate through each element  extract objects recursively
                int idx = 0;
                foreach (var value in data)
                {
                    ExtractObjectsImpl(value, data, path + "[" + idx + "]", idToObjectMap, predicate, reviver);
                    idx++;
                }
            }
            else if (data.GetType().IsAssignableFrom(typeof(System.Collections.ICollection)))
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
            else if (data is System.Collections.IDictionary)
            {
                foreach (KeyValuePair<string, object> element in ((IDictionary<string, object>)data).SetOfKeyValuePairs())
                {
                    ExtractObjectsImpl((T)element.Value, data, path.Length == 0 ? element.Key : path + "." + element.Key, idToObjectMap, predicate, reviver);
                }
            }
            else
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                if (data is object && !data.GetType().FullName.StartsWith("System.", StringComparison.Ordinal))
                {
                    // if it is an object, iterate through each entry
                    // and extract objects recursively
                    Type clazz = data.GetType();
                    System.Reflection.FieldInfo[] fields = clazz.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.FieldType.IsPrimitive || field.FieldType.IsEnum  /*|| field.FieldType.isSyntheic*/ || field.IsStatic)
                        {
                            continue;
                        }

                        T value;
                        try
                        {
                            value = (T)field.GetValue(data);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        ExtractObjectsImpl(value, data, path.Length == 0 ? field.Name : path + "." + field.Name, idToObjectMap, predicate, reviver);
                    }

                }
            }

        }


        private static Dictionary<String, Property<V>> ExtractObjects<T, V>(dynamic data, Predicate<T> predicate, Func<T, V> reviver) where V : Identifiable
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
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }


    }
}
