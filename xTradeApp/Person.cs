using System.Collections.ObjectModel;
using System.Globalization;

namespace xTrade
{
    public class Person
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Bal { get; set; }
        public string ImageUrl { get; set; }

        public ObservableCollection<ClientsPointsClass> PToDoItems { get; set; }

        public Person()
        {
            PToDoItems = new ObservableCollection<ClientsPointsClass>();
        }

        public static string GetFirstNameKey(Person person)
        {
            char key = char.ToLower(person.FirstName[0]);

            if ((key < 'a' || key > 'z') && (key < 'а' || key > 'я'))
            {
                key = '#';
            }

            return key.ToString(CultureInfo.InvariantCulture);
        }

        public static int CompareByFirstName(object obj1, object obj2)
        {
            var p1 = (Person) obj1;
            var p2 = (Person) obj2;

            int result = System.String.CompareOrdinal(p1.FirstName, p2.FirstName);
            if (result == 0)
            {
                result = System.String.CompareOrdinal(p1.LastName, p2.LastName);
            }

            return result;
        }
    }
}
