namespace xTrade
{
    public class ClientsPointsClass
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}