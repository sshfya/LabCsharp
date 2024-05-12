using System;
using System.Collections;

namespace Lab1
{
    public abstract class V1Data: IEnumerable<DataItem>
    {
        public string Key { get; set; }
        public DateTime Date { get; set; }

        public V1Data(string k, DateTime t)
        {
            Key = k;
            Date = t;
        }
        public abstract double MaxDistance { get; }
        public abstract string ToLongString(string format);

        public override string ToString()
        {
            return "V1Data";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<DataItem> GetEnumerator();

        public abstract double Y1Average { get; }
        public abstract string ThisString { get; }
    }
}