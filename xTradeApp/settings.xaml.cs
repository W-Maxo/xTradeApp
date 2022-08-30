using System;
using System.Globalization;
using System.Windows;

namespace xTrade
{

    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();

            var app = Application.Current as App;

            if (app != null)
            {
                txtServerName.Text = app.HostName ?? string.Empty;

                txtPortNumber.Text = app.PortNumber.ToString(CultureInfo.InvariantCulture);

                txtUserName.Text = app.UserName ?? string.Empty;

                passwordUser.Password = app.Pass ?? string.Empty;
            }
        }

        private void AppbarSaveClick(object sender, EventArgs e)
        {
            var app = Application.Current as App;
            if (app != null)
            {
                app.HostName = txtServerName.Text;
                app.PortNumber = Convert.ToInt32(txtPortNumber.Text);

                app.UserName = txtUserName.Text;
                app.Pass = passwordUser.Password;
            }

            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void AppBarCancelButtonClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
