using System;

namespace xTrade
{
    public class Client:IComparable
    {
        public string ClName { get; set; }
        public int ID { get; set; }
        public string Country { get; set; }
        public string Language { get; set;  }

        public int CompareTo(object obj)
        {
            var tmp = (Client) obj;

            return tmp.ID.CompareTo(ID);
        }
    }
}