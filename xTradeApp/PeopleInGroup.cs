using System.Collections.Generic;

namespace xTrade
{
    public class PeopleInGroup : List<Person>
    {
        public PeopleInGroup(string category)
        {
            Key = category;
        }

        public string Key { get; set; }
    }
}
