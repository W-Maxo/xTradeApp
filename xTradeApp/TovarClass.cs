using System.Collections.Generic;

namespace xTrade
{
    public class TovarClass
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int TvID { get; set; }

        public int CodeTv { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public int NimP { get; set; }
        public List<double> CostP { get; set; }
        public int Remains { get; set; }

        public TovarClass()
        {
            CostP = new List<double>();
        }
    }
}
