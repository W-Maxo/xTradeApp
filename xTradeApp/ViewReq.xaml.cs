using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace xTrade
{
    public partial class ViewReq
    {
        private bool NewReq { get; set; } 
        private bool _newreq;
        private bool _edit;
        private DateTime _dateCreation;
        private string _unqStr;
        private bool _xLoaded;
        private ObservableCollection<ReqItem> _allToDoItems;

        private ObservableCollection<ReqItem> AllToDoItems
        {
            get { return _allToDoItems; }
            set
            {
                _allToDoItems = value;
                NotifyPropertyChanged("AllToDoItems");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void Load()
        {
            var app = Application.Current as App;

            if (app != null)
            {
                if (app.Inf.Clients != null)
                {
                    var clientsLst = app.Inf.Clients.Values.Cast<ClientsClass>().OrderBy(c => c.ClientName).Select(c1 => new Client { ClName = c1.ClientName, ID = c1.IDClient }).ToList();
                    ClNamelistPicker.ItemsSource = clientsLst;
                    
                    if (app.Inf.ReqStatus != null)
                    {
                        List<SimpleData> tmpsdlst = NewReq ? app.Inf.ReqStatusNew.Values.Cast<SimpleData>().ToList() : app.Inf.ReqStatus.Values.Cast<SimpleData>().ToList();
                        
                        StatuslistPicker.ItemsSource = tmpsdlst;
                        
                        if (app.Inf.ReqPriority != null)
                        {
                            List<SimpleData> tmppriorlst = app.Inf.ReqPriority.Values.Cast<SimpleData>().ToList();
                            PrioritylistPicker.ItemsSource = tmppriorlst;
                            
                            if (app.Inf.Currency != null)
                            {
                                List<SimpleData> tmpcurlst = app.Inf.Currency.Values.Cast<SimpleData>().ToList();
                                CurencylistPicker.ItemsSource = tmpcurlst;
                                
                                if (app.Inf.Warehouse != null)
                                {
                                    List<SimpleData> tmpwrhlst = app.Inf.Warehouse.Values.Cast<SimpleData>().ToList();
                                    WarehouselistPicker.ItemsSource = tmpwrhlst;
                                    
                                    if (app.Inf.TypePayment != null)
                                    {
                                        List<SimpleData> tmptpmlst = app.Inf.TypePayment.Values.Cast<SimpleData>().ToList();
                                        TPaymentlistPicker.ItemsSource = tmptpmlst;
                                        
                                        var cr = (Application.Current as App).Nreq;
                                        if (cr != null)
                                        {
                                            _dateCreation = cr.DateCreation;
                                            _unqStr = cr.UnqStr;

                                            var num = clientsLst.First(client => client.ID == cr.IDClient);
                                            int index = clientsLst.IndexOf(num);

                                            if (index > -1)
                                            {
                                                ClNamelistPicker.SelectedIndex = index;

                                                var tmpclst = (ClientsClass)app.Inf.Clients[cr.IDClient];

                                                if (tmpclst.PCl.Count > 0)
                                                {
                                                    tPointPicker.ItemsSource = tmpclst.PCl;

                                                    var cpl = tmpclst.PCl.First(clientp => clientp.ID == cr.IDClientPoint);
                                                    index = tmpclst.PCl.IndexOf(cpl);

                                                    if (index > -1)
                                                    {
                                                        tPointPicker.SelectedIndex = index;
                                                    }
                                                }
                                                tPointPicker.IsEnabled = NewReq;

                                                if (app.Inf.ReqStatus.Values.Count > 0)
                                                {
                                                    var sd = tmpsdlst.First(s => s.ID == cr.ReqStatusID);
                                                    index = tmpsdlst.IndexOf(sd);

                                                    if (index > -1)
                                                    {
                                                        StatuslistPicker.SelectedIndex = index;
                                                    }
                                                }
                                                StatuslistPicker.IsEnabled = NewReq;

                                                if (app.Inf.ReqPriority.Values.Count > 0)
                                                {
                                                    var sd = tmppriorlst.First(s => s.ID == cr.PriorityID);
                                                    index = tmppriorlst.IndexOf(sd);

                                                    if (index > -1)
                                                    {
                                                        PrioritylistPicker.SelectedIndex = index;
                                                    }
                                                }
                                                PrioritylistPicker.IsEnabled = NewReq;

                                                if (app.Inf.Currency.Values.Count > 0)
                                                {
                                                    var sd = tmpcurlst.First(s => s.ID == cr.CurrencyID);
                                                    index = tmpcurlst.IndexOf(sd);

                                                    if (index > -1)
                                                    {
                                                        CurencylistPicker.SelectedIndex = index;
                                                    }
                                                }
                                                CurencylistPicker.IsEnabled = NewReq;

                                                if (app.Inf.Warehouse.Values.Count > 0)
                                                {
                                                    var sd = tmpwrhlst.First(s => s.ID == cr.WarehouseID);
                                                    index = tmpwrhlst.IndexOf(sd);

                                                    if (index > -1)
                                                    {
                                                        WarehouselistPicker.SelectedIndex = index;
                                                    }
                                                }
                                                WarehouselistPicker.IsEnabled = NewReq;

                                                if (app.Inf.TypePayment.Values.Count > 0)
                                                {
                                                    var sd = tmptpmlst.First(s => s.ID == cr.PaymentID);
                                                    index = tmptpmlst.IndexOf(sd);

                                                    if (index > -1)
                                                    {
                                                        TPaymentlistPicker.SelectedIndex = index;
                                                    }
                                                }
                                                TPaymentlistPicker.IsEnabled = NewReq;

                                                DiscountEdit.Text = cr.Discount.ToString(CultureInfo.InvariantCulture);
                                                DiscountEdit.IsEnabled = NewReq;

                                                DiscDatePicker.Value = cr.DateDelivery;
                                                DiscDatePicker.IsEnabled = NewReq;

                                                NoteEdit.Text = cr.Note;
                                                NoteEdit.IsEnabled = NewReq;

                                                foreach (var ri in cr.ReqItemList)
                                                {
                                                    ri.ItemName = app.PrCl.GetProdNameByID(ri.TvID);
                                                    ri.Btnen = NewReq;
                                                    AllToDoItems.Add(ri);
                                                }

                                                MainPv.Title = "Trading Agent - просмотр заказа";
                                            }
                                            ClNamelistPicker.IsEnabled = NewReq;
                                        }
                                        else
                                        {
                                            _newreq = true;

                                            MainPv.Title = "Trading Agent - новый заказ";

                                            DiscountEdit.Text = "0";

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ClNamelistPicker.SelectionChanged += delegate
                {
                    var tmpcl = (Client)ClNamelistPicker.SelectedItem;


                    if (tmpcl != null)
                    {
                        var tmpclst = (ClientsClass)app.Inf.Clients[tmpcl.ID];

                        tPointPicker.ItemsSource = tmpclst.PCl;
                    }

                    ChangeSelection();
                };

                tPointPicker.SelectionChanged += delegate { ChangeSelection(); };
                StatuslistPicker.SelectionChanged += delegate { ChangeSelection(); };
                PrioritylistPicker.SelectionChanged += delegate { ChangeSelection(); };
                CurencylistPicker.SelectionChanged += delegate { ChangeSelection(); };
                DiscountEdit.TextChanged += delegate { ChangeSelection(); };
                DiscDatePicker.ValueChanged += delegate { ChangeSelection(); };
                NoteEdit.TextChanged += delegate { ChangeSelection(); };
                WarehouselistPicker.SelectionChanged += delegate { ChangeSelection(); };
                TPaymentlistPicker.SelectionChanged += delegate { ChangeSelection(); }; 

                app.ApplicationDataObjectSelProd += BasepageApplicationDataObjectSelProd;

                if (_newreq) ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            }

            _xLoaded = true;
        }

        private void ChangeSelection()
        {
            if (_xLoaded)
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = _allToDoItems.Count > 0 && NewReq;
        }

        public ViewReq()
        {
            InitializeComponent();

            AllToDoItems = new ObservableCollection<ReqItem>();
            allToDoItemsListBox.ItemsSource = AllToDoItems;
        }

        void BasepageApplicationDataObjectSelProd(object sender, EventArgs e)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                SelProd();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(SelProd);
            }
        }

        private void SelProd()
        {
            var app = Application.Current as App;
            if (app == null) return;

            PlaySound("chimes.wav");

            if (app.SelProd != null)
                AllToDoItems.Add(new ReqItem { ItemName = app.PrCl.GetProdNameByID(app.SelProd.TvID),
                                               Count = app.SelProd.Count,
                                               TvID = app.SelProd.TvID,
                                               Btnen = true,
                                               CodeTv = app.SelProd.CodeTv,
                                               CurrencyID = app.SelProd.CurrencyID,
                                               CostTv = app.SelProd.CostTv
                });

            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = _allToDoItems.Count > 0;
        }

        private void SelAppBarButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ProdPage.xaml?Rem=0&Add=1", UriKind.Relative));
        }

        private static void PlaySound(string soundFile)
        {
            using (var stream = TitleContainer.OpenStream(soundFile))
            {
                var effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();

            }
        }

        private void DeleteTaskButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                var toDoForDelete = button.DataContext as ReqItem;

                AllToDoItems.Remove(toDoForDelete);
            }

            Focus();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = _allToDoItems.Count > 0;
        }

        private void AppbarSaveClick(object sender, EventArgs e)
        {
            var app = Application.Current as App;

            if (app != null)
            {
                var tmpRcl = new ReqClass();

                var tmpcl = (Client) ClNamelistPicker.SelectedItem;
                var tmpclst = (ClientsClass) app.Inf.Clients[tmpcl.ID];
                tmpRcl.IDClient = tmpclst.IDClient;

                var cpd = (ClientsPointsClass)tPointPicker.SelectedItem;
                tmpRcl.IDClientPoint = cpd!=null ? cpd.ID : -1;

                var sd = (SimpleData)StatuslistPicker.SelectedItem;
                tmpRcl.ReqStatusID = sd.ID;

                sd = (SimpleData)PrioritylistPicker.SelectedItem;
                tmpRcl.PriorityID = sd.ID;

                sd = (SimpleData)CurencylistPicker.SelectedItem;
                tmpRcl.CurrencyID = sd.ID;

                sd = (SimpleData)WarehouselistPicker.SelectedItem;
                tmpRcl.WarehouseID = sd.ID;

                sd = (SimpleData)TPaymentlistPicker.SelectedItem;
                tmpRcl.PaymentID = sd.ID;

                tmpRcl.Discount = double.Parse(DiscountEdit.Text);

                if (DiscDatePicker.Value != null) tmpRcl.DateDelivery = (DateTime) DiscDatePicker.Value;

                tmpRcl.Note = NoteEdit.Text;

                foreach (var req in AllToDoItems)
                {
                    tmpRcl.ReqItemList.Add(req);
                }

                if (NewReq && !_edit)
                {
                    tmpRcl.DateCreation = DateTime.Now;

                    string tomd5HashStr = string.Format("{0} {1} {2}", tmpRcl.DateCreation.ToString(CultureInfo.InvariantCulture), app.UserName, RandomString.NextString(new Random(), 25));

                    tmpRcl.UnqStr = MD5Core.GetHashString(tomd5HashStr).ToLower();
                    
                    NewReqListClass.AddItemToNewReqList(tmpRcl);
                }
                else
                {
                    tmpRcl.UnqStr = _unqStr;
                    tmpRcl.DateCreation = _dateCreation;
                    NewReqListClass.UpdateItemInNewReqList(tmpRcl);
                }

                app.Nreq = null;
                
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (NavigationService.CanGoBack)
            {
                if (_newreq || NewReq)
                {
                    if (MessageBox.Show("Выйти без сохранения?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        NavigationService.GoBack();
                    }
                    else e.Cancel = true;
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
            else e.Cancel = true;
        }

        private void AppBarCancelButtonClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                if (_newreq || NewReq)
                {
                    if (MessageBox.Show("Выйти без сохранения?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
        }

        private void AppBarDeleteButtonClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {    
                if (MessageBox.Show("Удалить заявку?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    NewReqListClass.DeleteReq(_unqStr);
                    NavigationService.GoBack();
                } 
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string idParam;
            if (NavigationContext.QueryString.TryGetValue("New", out idParam))
            {
                int nw = Int32.Parse(idParam);

                if (1 == nw)
                {
                    NewReq = true;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = true;


                    if (NavigationContext.QueryString.TryGetValue("Edit", out idParam))
                    {
                        int edt = Int32.Parse(idParam);

                        if (1 == edt) _edit = true;
                    } 
                }
             }

            if (!_xLoaded)
            {
                Load();
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
            }
        }
    }
}