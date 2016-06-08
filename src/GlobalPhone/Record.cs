using System;
using System.Linq;
using System.Collections;

namespace GlobalPhone
{
    /// <summary>
    /// Base class for different classes that reads the json data
    /// </summary>
    public class Record
    {
        private readonly object[] _data;
        private readonly IDictionary _hash;
        /// <summary>
        /// Determines whether this instance is an array or array like.
        /// </summary>
        protected bool IsArray(object value)
        {
            return value is IEnumerable && !(value is IDictionary) && !(value is string);
        }
        /// <summary>
        /// If the value is an array, returns an array of objects 
        /// </summary>
        protected object[] AsArray(object value)
        {
            if (!IsArray(value))
            {
                throw new Exception("Is not array: " + value.GetType().Name);
            }
            IEnumerable enumerable;
            if ((enumerable = value as IEnumerable) != null)
            {
                return enumerable.Cast<Object>().ToArray();
            }
            else
            {
                throw new Exception("Unknown type: " + value.GetType().Name);
            }
        }
        /// <summary>
        /// If the value is a dictionary returns a dictionary.
        /// </summary>
        protected IDictionary AsHash(object value)
        {
            IDictionary dictionary;
            if ((dictionary = value as IDictionary) != null)
            {
                return dictionary;
            }
            else
            {
                throw new Exception("Unknown type: " + value.GetType().Name);
            }
        }
        internal Record(object data)
        {
            // the data is either an array or a hash
            if (IsArray(data))
            {
                _data = AsArray(data);
            }
            else
            {
                _hash = AsHash(data);
            }
        }
        /// <summary>
        /// Get field value as array.
        /// </summary>
        protected object[] FieldAsArray(int index, string column)
        {
            return AsArray(_data != null ? _data[index] : _hash[column]);
        }
        /// <summary>
        /// Get field value as maybe an array.
        /// </summary>
        protected TRet[] FieldMaybeAsArray<T, TRet>(int index, string column, Func<T, TRet> block)
        {
            if (_data != null)
            {
                try
                {
                    if (_data.Length <= index)
                        return new TRet[0];
                    var val = _data[index];
                    return MaybeMapAsArray(block, val);
                }
                catch (Exception e)
                {
                    throw new Exception("Index: " + index, e);
                }
            }
            try
            {
                if (!_hash.Contains(column) || _hash[column] == null)
                    return new TRet[0];
                var val = _hash[column];
                return MaybeMapAsArray(block, val);
            }
            catch (Exception e)
            {
                throw new Exception("Column: " + column, e);
            }
        }

        private TRet[] MaybeMapAsArray<T, TRet>(Func<T, TRet> block, object val)
        {
            if (val != null && IsArray(val))
            {
                return AsArray(val).Cast<T>().Select(block).ToArray();
            }
            else
            {
                return new[] { block(((T)val)) };
            }
        }
        /// <summary>
        /// Get field value.
        /// </summary>
        protected T Field<T>(int index, string column, T fallback = default(T))
        {
            if (_data != null)
            {
                try
                {
                    if (_data.Length <= index)
                        return fallback;
                    var data = (T)_data[index];
                    return data;
                }
                catch (Exception e)
                {
                    throw new Exception("Index: " + index, e);
                }
            }

            try
            {
                if (!_hash.Contains(column)|| _hash[column]==null)
                    return fallback;
                var data = (T)_hash[column];
                return data;
            }
            catch (Exception e)
            {
                throw new Exception("Column: " + column, e);
            }
        }
        /// <summary>
        /// Get field value.
        /// </summary>
        protected TRet Field<T, TRet>(int index, string column, Func<T, TRet> block)
        {
            if (_data != null)
            {
                try
                {
                    if (_data.Length <= index)
                        return default(TRet);
                    var data = ((T)_data[index]);
                    return block(data);
                }
                catch (Exception e)
                {
                    throw new Exception("Index: " + index, e);
                }
            }
            try
            {
                if (!_hash.Contains(column) || _hash[column]==null)
                    return default(TRet);
                var data = (T)_hash[column];
                return block(data);
            }
            catch (Exception e)
            {
                throw new Exception("Column: " + column, e);
            }

        }
    }
}