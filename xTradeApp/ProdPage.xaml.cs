using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;

namespace xTrade
{
    public partial class ProdPage
    {
        private readonly List<LongListSelector> _pitems;
        private TovarClass _tmpTv;
        private bool _showNotRem = true;
        private bool _allowAdd;

        public ProdPage()
        {
            InitializeComponent();

            //ApplicationBar.Mode = ApplicationBarMode.Minimized;

            _pitems = new List<LongListSelector>();

            TiltEffect.TiltableItems.Add(typeof(StackPanel));
            TiltEffect.TiltableItems.Add(typeof(Border));
            
        }

        private void LoadLinqTovars(Pivot pivotApp)
        {
            try
            {
                using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (appStorage.FileExists("Tovars.xml"))
                    {
                        using (var file = appStorage.OpenFile("Tovars.xml", FileMode.Open))
                        {
                            _pitems.Clear();

                            var ci = new CultureInfo("en-US");

                            var listHeaderTemplate = (DataTemplate)Resources["tovarListHeader"];
                            var groupHeaderTemplate = (DataTemplate)Resources["tovarGroupHeader"];
                            var groupItemTemplate = (DataTemplate)Resources["TovarGroupItemHeader"];
                            var itemTemplate = (DataTemplate)Resources["tovarItemTemplate"];

                            using (var reader = XmlReader.Create(file))
                            {
                                while (reader.Read())
                                {
                                    if (0 != String.CompareOrdinal(reader.Name, "Gr")) continue;
                                    var nameAttr = reader.GetAttribute("GrNm");

                                    var tovars = new List<TovarClass>();

                                    var newItem = new PivotItem { Header = nameAttr };

                                    var newList = new LongListSelector
                                    {
                                        ListHeaderTemplate = listHeaderTemplate,
                                        GroupHeaderTemplate = groupHeaderTemplate,
                                        GroupItemTemplate = groupItemTemplate,
                                        ItemTemplate = itemTemplate,
                                        //IsBouncy = true,
                                        Background = new SolidColorBrush(Colors.Transparent)
                                    };

                                    var tpreader = reader.ReadSubtree();

                                    while (tpreader.Read())
                                    {
                                        if (0 != String.CompareOrdinal(reader.Name, "Tp")) continue;
                                        var tnameAttr = reader.GetAttribute("TpNm");

                                        if (String.IsNullOrEmpty(nameAttr)) continue;
                                        var readerSubtree = tpreader.ReadSubtree();

                                        while (readerSubtree.Read())
                                        {
                                            if (readerSubtree.Name != "Item") continue;

                                            string rem = readerSubtree.GetAttribute("Rem") ?? "0";

                                            if (_showNotRem || 0 != int.Parse(rem))
                                            {
                                                string items = string.Empty;
                                                string vID = string.Empty;

                                                try
                                                {

                                                    items = readerSubtree.GetAttribute("Desc");
                                                    vID = readerSubtree.GetAttribute("TvID");
                                                    var nimp = readerSubtree.GetAttribute("CntinP");
                                                    var pr1 = readerSubtree.GetAttribute("Price1");
                                                    var pr2 = readerSubtree.GetAttribute("Price2");
                                                    var cdtv = readerSubtree.GetAttribute("Code");

                                                    var tmptvr = new TovarClass
                                                                     {
                                                                         Category = tnameAttr,
                                                                         Remains = int.Parse(rem),
                                                                         Name = items,
                                                                         TvID = vID != null ? int.Parse(vID) : 0,
                                                                         NimP = nimp != null ? int.Parse(nimp) : 0,
                                                                         Title = items,
                                                                         CodeTv = cdtv != null ? int.Parse(cdtv) : 0

                                                                     }; 


                                                    tmptvr.CostP.Add(pr1 != null ? double.Parse(pr1, ci) : 0);
                                                    tmptvr.CostP.Add(pr2 != null ? double.Parse(pr2, ci) : 0);

                                                    tmptvr.Description = "Цена: " +
                                                                         tmptvr.CostP[0] +
                                                                         " (" +
                                                                         tmptvr.CostP[1] +
                                                                         "); Ост." +
                                                                         rem +
                                                                         "; Кол-во в уп.:" +
                                                                         tmptvr.NimP.ToString(
                                                                             CultureInfo.InvariantCulture);


                                                    tovars.Add(tmptvr);
                                                }
                                                catch (Exception exc)
                                                {
                                                    MessageBox.Show(exc.Message + " (" + items + " id:" + vID + ")", "Ошибка загрузки файла!", MessageBoxButton.OK);
                                                }
                                            }
                                        }
                                    }

                                    var tovarsByCategory = from tovar in tovars
                                                           group tovar by tovar.Category
                                                               into c
                                                               orderby c.Key
                                                               select new PublicGrouping<string, TovarClass>(c);

                                    newList.ItemsSource = tovarsByCategory;

                                    _pitems.Add(newList);

                                    if (_allowAdd)
                                        newList.SelectionChanged += TovarSelectionChanged;

                                    newItem.Content = newList;

                                    pivotApp.Items.Add(newItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка загрузки файла!", MessageBoxButton.OK);
            }
        }

        void TovarSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpLls = (LongListSelector)sender;

            _tmpTv = (TovarClass)tmpLls.SelectedItem;

            if (null != _tmpTv)
            {
                var input = new InputPrompt();

                foreach (var pitem in _pitems)
                {
                    pitem.IsEnabled = false;
                }

                input.Completed += InputCompleted;
                input.IsCancelVisible = true;
                input.Title = "Количество:";
                input.Message = _tmpTv.Title;

                input.Value = "1";

                var iscope = new InputScope {Names = {new InputScopeName {NameValue = InputScopeNameValue.Number}}};

                input.InputScope = iscope;

                input.Show();
            }
        }

        void InputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            foreach (var pitem in _pitems)
            {
                pitem.IsEnabled = true;
            }

            if (!e.PopUpResult.Equals(PopUpResult.Cancelled))
            {
               var app = Application.Current as App;

                if (app != null)
                {
                    app.SelProd = new ReqItem { TvID = _tmpTv.TvID,
                                                CodeTv = _tmpTv.CodeTv,
                                                CurrencyID = 1,
                                                CostTv = _tmpTv.CostP[0],
                                                Count = int.Parse(e.Result)};
                    
                }
            }
        }

        private void AppBarCancelButtonClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string idParam;
            if (NavigationContext.QueryString.TryGetValue("Rem", out idParam))
            {
                int rem = Int32.Parse(idParam);

                if (0 == rem)
                    _showNotRem = false;
            }

            if (NavigationContext.QueryString.TryGetValue("Add", out idParam))
            {
                int alladd = Int32.Parse(idParam);

                if (1 == alladd)
                    _allowAdd = true;
            }

            LoadLinqTovars(prodPivotApp);
        }
    }
}