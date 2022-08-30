using System;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace xTrade
{
    public partial class PersonDetail : PhoneApplicationPage
    {
        public PersonDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string idParam;
            if (NavigationContext.QueryString.TryGetValue("ID", out idParam))
            {
                int id = Int32.Parse(idParam);
                DataContext = AllPeople.Current[id];
            }
        }
    }
}