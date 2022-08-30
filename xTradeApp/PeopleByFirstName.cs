using System.Collections.Generic;
using System.Globalization;

namespace xTrade
{
    public class PeopleByFirstName : List<PeopleInGroup>
    {
        private const string Groups = "#абвгдеёжзийклмнопрстуфхцчшщъыьэюяabcdefghijklmnopqrstuvwxyz";

        public PeopleByFirstName()
        {
            var people = new List<Person>(AllPeople.Current);
            people.Sort(Person.CompareByFirstName);

            var groups = new Dictionary<string, PeopleInGroup>();

            foreach (char c in Groups)
            {
                var group = new PeopleInGroup(c.ToString(CultureInfo.InvariantCulture));
                Add(group);
                groups[c.ToString(CultureInfo.InvariantCulture)] = group;
            }

            foreach (Person person in people)
            {
                groups[Person.GetFirstNameKey(person)].Add(person);
            }
        }
    }
}
