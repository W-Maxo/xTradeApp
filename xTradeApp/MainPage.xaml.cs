using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace xTrade
{
    public partial class MainPage
    {
        ProgressIndicator _prog;
        bool _isNewPageInstance;
        private InfData Inf { get; set; }
        private ReqClass _reqToSend;
        private int _lastOperation;

        public MainPage()
        {
            InitializeComponent();

            Inf=new InfData();

            var appl = Application.Current as App;

            if (appl != null)
            {
                appl.Inf = Inf;
                appl.PrCl=new ProdClass();
                appl.PrCl.LoadLinqTovars();
            }

            LoadLinqReq();
            linqReq.SelectionChanged += ReqSelectionChanged;
            linqReqClosed.SelectionChanged += ReqSelectionChanged;

            LoadLinqArr();
            linqArr.SelectionChanged += ReqArrSelectionChanged;
            
            _isNewPageInstance = true;

            TiltEffect.TiltableItems.Add(typeof(StackPanel));
            TiltEffect.TiltableItems.Add(typeof(Border));
        }

        void ReqSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpLls = (LongListSelector)sender;

            var tmpTv = (ReqClass)tmpLls.SelectedItem;

            if (null != tmpTv)
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    app.Nreq = tmpTv;
                    NavigationService.Navigate(new Uri("/ViewReq.xaml?New=0", UriKind.Relative));
                }
            }
        }

        void NewReqSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpLls = (LongListSelector)sender;

            var tmpTv = (ReqClass)tmpLls.SelectedItem;

            if (null == tmpTv) return;

            var app = Application.Current as App;

            if (app == null) return;

            app.Nreq = tmpTv;

            NavigationService.Navigate(tmpTv.Posted
                                           ? new Uri("/ViewReq.xaml?New=0", UriKind.Relative)
                                           : new Uri("/ViewReq.xaml?New=1&Edit=1", UriKind.Relative));
        }

        void ReqArrSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpLls = (LongListSelector)sender;

            var tmpTv = (ReqClass)tmpLls.SelectedItem;

            if (null != tmpTv)
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    app.Nreq = tmpTv;
                    NavigationService.Navigate(new Uri("/ViewReq.xaml?New=0", UriKind.Relative));
                }
            }
        }

        private bool LoadLinqReq()
        {
            try
            {
                var reqs = ReqListClass.GetReqListClass(Inf, "Requests.xml");

                if (reqs != null)
                {
                    NewReqListClass.RemoveExisting(reqs);

                    var reqsByCategory = from req in reqs
                                         group req by req.Category
                                         into c
                                         orderby c.Key descending
                                         select new PublicGrouping<DateTime, ReqClass>(c);

                    linqReq.ItemsSource = reqsByCategory;
                }

                var reqsClosed = ReqListClass.GetReqListClass(Inf, "RequestsClosed.xml");

                if (reqsClosed != null)
                {
                    NewReqListClass.RemoveExisting(reqsClosed);

                    var reqsByCategory = from req in reqsClosed
                                         group req by req.Category
                                             into c
                                             orderby c.Key descending
                                             select new PublicGrouping<DateTime, ReqClass>(c);

                    linqReqClosed.ItemsSource = reqsByCategory;
                }

                LoadLinqNewReq();

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка загрузки файла!", MessageBoxButton.OK);
                return false;
            }
        }

        private bool LoadLinqNewReq()
        {
            try
            {
                linqNewReq.SelectionChanged += null;

                var reqs = NewReqListClass.GetNewReqList(Inf);

                if (reqs != null)
                {
                    var reqsByCategory = from req in reqs
                                                          group req by req.Category
                                                          into c
                                                          orderby c.Key descending
                                                          select new PublicGrouping<DateTime, ReqClass>(c);

                    linqNewReq.ItemsSource = reqsByCategory;

                    linqNewReq.SelectionChanged += NewReqSelectionChanged;
                }

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка загрузки файла!", MessageBoxButton.OK);
                return false;
            }
        }

        private void SendTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                _reqToSend = button.DataContext as ReqClass;

                if (_reqToSend != null)
                {
                    if (MessageBox.Show(string.Format("Отправить заказ: {0} ({1})?", _reqToSend.Title, _reqToSend.Description), "Отправка заказа", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        var app = Application.Current as App;
                        if (app == null)
                        {
                            AppbarSettingsClick(sender, e);
                            return;
                        }

                        UpdateClass client;

                        try
                        {
                            client = new UpdateClass(ServerName, PortNumber);
                        }
                        catch (Exception)
                        {
                            NavigationService.Navigate(new Uri("/settings.xaml", UriKind.RelativeOrAbsolute));
                            return;
                        }

                        client.ResponseReceived += AcGotMove;

                        var serializer = new XmlSerializer(typeof(ReqClass));

                        using (var file = new MemoryStream())
                        {
                            TextWriter writer = new StreamWriter(file);

                            serializer.Serialize(writer, _reqToSend);

                            file.Position = 0;

                            var xfileData = new byte[file.Length];
                            file.Read(xfileData, 0, xfileData.Length);

                            string tp = string.Format("SendRec:{0}", xfileData.Length);

                            string sstr = string.Format("xTradeMobility\tUser={0}\tPass={1}\tType={2}", UserName, Pass, tp);

                            client.SendData(sstr, xfileData);

                            _lastOperation = 2;


                            SystemTray.SetIsVisible(this, true);
                            SystemTray.SetOpacity(this, 0.5);
                            SystemTray.SetBackgroundColor(this, Colors.Black);
                            SystemTray.SetForegroundColor(this, Colors.White);

                            _prog = new ProgressIndicator { IsVisible = true, IsIndeterminate = true, Text = "Отправка..." };

                            mainPivotApp.Title = string.Empty;

                            linqNewReq.IsEnabled = false;

                            SystemTray.SetProgressIndicator(this, _prog);
                        }              
                    }
                }
            }

            Focus();
        }

        private bool LoadLinqArr()
        {
            var arrreqs = ReqListClass.GetReqListClass(Inf, "RequestsArr.xml");

            if (arrreqs != null && Inf != null)
            {
                var reqsByCategory = from req in arrreqs
                                     group req by ((ClientsClass)Inf.Clients[req.IDClient]).ClientName
                                         into c
                                         orderby c.Key descending
                                         select new PublicGrouping<string, ReqClass>(c);

                linqArr.ItemsSource = reqsByCategory;

                return true;
            }
            
            linqArr.ItemsSource = null;
            return false;
        }

        private void NewTaskAppBarButtonClick(object sender, EventArgs e)
        {
            var app = Application.Current as App;
            if (app != null)
            {
                app.Nreq = null;
            }

            NavigationService.Navigate(new Uri("/ViewReq.xaml?New=1", UriKind.Relative));
        }

        private void ClientsAppBarButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewClients.xaml", UriKind.Relative));
        }

        private void ProdAppBarButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ProdPage.xaml", UriKind.Relative));
        }

        private void AppbarSettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/settings.xaml", UriKind.RelativeOrAbsolute));
        }

        private void AboutItemClick(object sender, EventArgs e)
        {
            linqReq.IsEnabled = false;
            linqNewReq.IsEnabled = false;
            linqArr.IsEnabled = false;

            var about = new AboutPrompt();
            about.Completed += AboutCompleted;

            about.Show("Kolekirov Nikolay", null, "Agefer@gmail.com");
        }

        void AboutCompleted(object sender, PopUpEventArgs<object, PopUpResult> e)
        {
            linqReq.IsEnabled = true;
            linqNewReq.IsEnabled = true;
            linqArr.IsEnabled = true;
        }

        private void UpdateAppBarButton_OnClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Обновить данные?", "Обновление", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var app = Application.Current as App;
                if (app == null)
                {
                    AppbarSettingsClick(sender, e);
                    return;
                }

                UpdateClass client;

                try
                {
                    client = new UpdateClass(ServerName, PortNumber);
                }
                catch (Exception)
                {
                    NavigationService.Navigate(new Uri("/settings.xaml", UriKind.RelativeOrAbsolute));
                    return;
                }
                
                client.ResponseReceived += AcGotMove;

                string sstr = string.Format("xTradeMobility\tUser={0}\tPass={1}\tType={2}", UserName, Pass, "GetData");
                client.SendData(sstr, null);

                SystemTray.SetIsVisible(this, true);
                SystemTray.SetOpacity(this, 0.5);
                SystemTray.SetBackgroundColor(this, Colors.Black);
                SystemTray.SetForegroundColor(this, Colors.White);

                _lastOperation = 1;

                _prog = new ProgressIndicator {IsVisible = true, IsIndeterminate = true, Text = "Обновление..."};

                mainPivotApp.Title = string.Empty;

                SystemTray.SetProgressIndicator(this, _prog);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    app.ApplicationDataObjectChanged += BasepageApplicationDataObjectChanged;
                    app.ApplicationDataObjectExchange += BasepageApplicationDataObjectExchange;

                    if (app.HostName != null)
                    {
                        UpdateDependencyProperties();
                    }
                    else
                    {
                        app.GetDataAsync();
                    }
                }
            }
            else
            {
                LoadLinqNewReq();
            }

            _isNewPageInstance = false;
        }

        void BasepageApplicationDataObjectExchange(object sender, EventArgs e)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                Exchange();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(Exchange);
            }
        }

        private void Exchange()
        {
            var app = Application.Current as App;
            if (app != null)
            {

            }
        }

        void BasepageApplicationDataObjectChanged(object sender, EventArgs e)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                UpdateDependencyProperties();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(UpdateDependencyProperties);
            }
        }

        private void UpdateDependencyProperties()
        {
            var app = Application.Current as App;
            if (app != null)
            {
                ServerName = app.HostName;
                PortNumber = app.PortNumber;
                UserName = app.UserName;
                Pass = app.Pass;
            }
        }

        public static readonly DependencyProperty ServerNameProperty = DependencyProperty.RegisterAttached("ServerName", typeof(string), typeof(string), new PropertyMetadata(string.Empty));

        private string ServerName
        {
            get { return (string)GetValue(ServerNameProperty); }
            set { SetValue(ServerNameProperty, value); }
        }

        public static readonly DependencyProperty PortNumberProperty = DependencyProperty.RegisterAttached("PortNumber", typeof(int), typeof(int), new PropertyMetadata(0));

        private int PortNumber
        {
            get { return (int)GetValue(PortNumberProperty); }
            set { SetValue(PortNumberProperty, value); }
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.RegisterAttached("UserName", typeof(string), typeof(string), new PropertyMetadata(string.Empty));

        private string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public static readonly DependencyProperty PassProperty = DependencyProperty.RegisterAttached("Pass", typeof(string), typeof(string), new PropertyMetadata(string.Empty));

        private string Pass
        {
            get { return (string)GetValue(PassProperty); }
            set { SetValue(PassProperty, value); }
        }

        void AcGotMove(object sender, ResponseReceivedEventArgs e)
        {
            if (e.IsError)
            {
                ReportMoveError(); 
            }
            else
            {
                if (0 == String.CompareOrdinal(e.Response, "rcSuccess"))
                {
                    switch (_lastOperation)
                    {
                        case 1:
                            {
                                Inf = new InfData();

                                var appl = Application.Current as App;

                                if (appl != null)
                                {
                                    appl.Inf = Inf;
                                    appl.PrCl.LoadLinqTovars();
                                }

                                if (LoadLinqReq() && LoadLinqNewReq() && LoadLinqArr())
                                {
                                    ReportUpdateSuccess();
                                }
                                else ReportMoveError();
                            }
                            break;

                        case 2:
                            {
                                _reqToSend.Posted = true;
                                NewReqListClass.UpdateItemInNewReqList(_reqToSend);
                                linqNewReq.IsEnabled = true;
                                LoadLinqNewReq();
                                ReportUpdateSuccess();
                            }
                            break;
                    }
                }

            }
        }

        private void ReportMoveError()
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                reportMoveError();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(reportMoveError);
            }
        }

        private void reportMoveError()
        {
            mainPivotApp.Title = "Trading Agent";
            SystemTray.SetIsVisible(this, false);

            string msg = string.Empty;

            switch (_lastOperation)
            {
                case 1: msg ="Ошибка обновления"; break;
                case 2: {msg ="Ошибка отправления";
                    linqNewReq.IsEnabled = true;
                    break;}
            }

            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK);
        }

        private void ReportUpdateSuccess()
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                reportUpdateSuccess();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(reportUpdateSuccess);
            }
        }

        private void reportUpdateSuccess()
        {
            mainPivotApp.Title = "Trading Agent";
            SystemTray.SetIsVisible(this, false);

            string msg = string.Empty;

            switch (_lastOperation)
            {
                case 1: msg = "Обновление успешно завершено"; break;
                case 2: msg = "Отправка успешна завершена"; break;
            }

            MessageBox.Show(msg, string.Empty, MessageBoxButton.OK);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (MessageBox.Show("Завершить работу с программой?", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }     
        }
    }
}