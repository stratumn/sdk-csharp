using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Misc
{
    public class Property<V > where V : Identifiable
    {

        public Property(String id, V value, String path, Object parent) :
                base()
        { 
            this._id = id;
            this._value = value;
            this._path = path;
            this._parent = parent;
        }

        private String _id;

        private V _value;

        private String _path;

        private Object _parent;
 

        public String Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public V Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public object Parent    
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public String Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public delegate R ValueBuilderDelegate< out R>(V arg);
    
        /// <summary>
        ///  Uses the delegate function to transform this property from value type to another
        ///  keeping other properties the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Valuebuilder"></param>
        /// <returns></returns>
        public Property<T> Transform<T>(ValueBuilderDelegate< T> Valuebuilder) where T:Identifiable
        {
           T value= Valuebuilder(this._value);
            return new Property<T>(this._id, value, this._path, this._parent); 
        }
    }
}
