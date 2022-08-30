using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace xTrade
{
    public class AllPeople : IEnumerable<Person>
    {
        private static Dictionary<int, Person> _personLookup;
        private static AllPeople _instance;

        public static AllPeople Current
        {
            get
            {
                return _instance ?? (_instance = new AllPeople());
            }
        }

        public Person this[int index]
        {
            get
            {
                Person person;
                _personLookup.TryGetValue(index, out person);
                return person;
            }
        }

        #region IEnumerable<Person> Members

        public IEnumerator<Person> GetEnumerator()
        {
            EnsureData();
            return _personLookup.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            EnsureData();
            return _personLookup.Values.GetEnumerator();
        }

        #endregion

        private void EnsureData()
        {
            _personLookup = new Dictionary<int, Person>();

            var app = Application.Current as App;

            if (app != null)
            {
                int cnt = 0;

                if (app.Inf.Clients != null)
                    foreach (var client in app.Inf.Clients.Values)
                    {
                        var tmpcc = (ClientsClass) client;

                        var person = new Person
                        {
                            Address = tmpcc.Address,
                            FirstName = tmpcc.ClientName,
                            ID = cnt,
                            LastName = "",
                            Tel = tmpcc.Telephone,
                            Bal = tmpcc.Balance.ToString(CultureInfo.InvariantCulture),
                            ImageUrl = "/Images/Person.jpg" };


                        foreach (ClientsPointsClass cpc in tmpcc.PCl)
                        {
                            person.PToDoItems.Add(cpc);
                        }

                        _personLookup[cnt] = person;
                        cnt++;
                    }
            }
        }
    }
}
