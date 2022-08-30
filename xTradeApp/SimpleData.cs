namespace xTrade
{
    public class SimpleData
    {
        public int ID { get; set; }
        public string ClName { get; set; }

        override public string ToString()
        {
            return ClName;
        }
    }
}