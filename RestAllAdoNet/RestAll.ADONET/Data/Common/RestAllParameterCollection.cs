﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
#nullable disable
namespace RESTAll.Data.Common
{
    public class RestAllParameterCollection : DbParameterCollection, IDataParameterCollection
    {
        private List<RestAllParameter> _Params = new List<RestAllParameter>();


        public override void Clear()
        {
            _Params.Clear();
        }

        public override bool Contains(object value)
        {
            return IndexOf(value) >= 0;
        }

        public override bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        public override int IndexOf(object value)
        {
            return _Params.FindIndex(a => a.Value == value);
        }

        public override int IndexOf(string parameterName)
        {
            return _Params.FindIndex(a => _cultureAwareCompare(a.ParameterName, parameterName) == 0);
        }

        public override void Insert(int index, object value)
        {
            var param = value as RestAllParameter;
            if (param == null)
            {
                throw new ArgumentException("value to insert must be of type SalesforceParameter", "value");
            }
            _Params.Insert(index, param);

        }

        public override void Remove(object value)
        {
            var param = value as RestAllParameter;
            if (param == null)
            {
                throw new ArgumentException("value to remove must be of type SalesforceParameter", "value");
            }
            _Params.Remove(param);
        }

        public override void RemoveAt(int index)
        {
            _Params.RemoveAt(index);
        }

        public override IEnumerator GetEnumerator()
        {
            return _Params.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            return _Params[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _Params.FirstOrDefault(a => a.ParameterName == parameterName);
        }

        public override void CopyTo(Array array, int index)
        {
            var arr = array as RestAllParameter[];
            if (arr == null)
            {
                throw new ArgumentException("array must be of type SalesforceParameter[]", "array");
            }
            _Params.CopyTo(arr, index);
        }

        public override int Count
        {
            get { return _Params.Count; }
        }

        public override bool IsFixedSize
        {
            get { return false; }
        }

        public override object SyncRoot
        {
            get { return null; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsSynchronized
        {
            get { return true; }
        }

        public override void RemoveAt(string parameterName)
        {
            RemoveAt(IndexOf(parameterName));
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            var param = value as RestAllParameter;
            if (param == null)
            {
                throw new ArgumentException("value must be of type RESTAllParameter", "value");
            }
            _Params[index] = param;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var param = value as RestAllParameter;
            if (param == null)
            {
                throw new ArgumentException("value must be of type RESTAllParameter", "value");
            }
            _Params[IndexOf(parameterName)] = param;
        }

        public override int Add(object value)
        {
            return Add((RestAllParameter)value);
        }

        public int Add(RestAllParameter value)
        {
            if (value.CanAddToCommand())
            {
                var nextIndex = _Params.Count;
                _Params.Add(value);
                return nextIndex;
            }
            throw new ArgumentException("parameter must be named");
        }

        public int Add(string parameterName, DbType type)
        {
            return Add(new RestAllParameter(parameterName, type));
        }

        public int Add(string parameterName, object value)
        {
            return Add(new RestAllParameter(parameterName, value));
        }

        public int Add(string parameterName, DbType dbType, string sourceColumn)
        {
            return Add(new RestAllParameter(parameterName, dbType, sourceColumn));
        }

        public override void AddRange(Array values)
        {
            var paramaters = values.Cast<RestAllParameter>().ToList();
            bool canAdd = paramaters.Aggregate(true, (current, crmParameter) => current && crmParameter.CanAddToCommand());

            if (canAdd)
            {
                _Params.AddRange(paramaters);
            }
            else
            {
                throw new ArgumentException("one or more of the parameters in the array are not valid to add to a command. Are they all named?");
            }
        }

        private int _cultureAwareCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
        }
    }
}
