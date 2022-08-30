using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace xTrade
{
    public partial class NewTaskPage
    {
        public NewTaskPage()
        {
            InitializeComponent();

            buddies.SelectionChanged += PersonSelectionChanged;
        }

        void PersonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var person = buddies.SelectedItem as Person;
            if (person != null)
            {
                NavigationService.Navigate(new Uri("/PersonDetail.xaml?ID=" + person.ID, UriKind.Relative));
            }
        }

        private void AppBarCancelButtonClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void PhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
