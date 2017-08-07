using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;

namespace RSSFeedReader
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _url = "https://nyaa.si/?page=rss";      //default path
        private SyndicationFeed _feed;

        public MainWindow()
        {
            InitializeComponent();
            CreateFeed();
        }

        private void CreateFeed()                               //create syndicationfeed from given url
        {
            try
            {
                using (var reader = XmlReader.Create(_url))
                {
                    _feed = SyndicationFeed.Load(reader);
                    if (_feed != null)
                    {
                        LstFeedItems.ItemsSource = _feed.Items.Take(11);
                        LstFeedItems.FontWeight = FontWeights.Bold;
                        TxtUrl.Text = _feed.Title.Text;
                    }
                    else
                    {
                        MessageBox.Show("RSS feed is empty or not valid.");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to load feed. Check your internet connection.");
                throw;
            }
        }

        private void RefBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateFeed();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Update(object sender, SelectionChangedEventArgs e)                 //listbox selection change event
        {
            if (e.AddedItems.Count == 0)
            {
                SeedersTextBox.Text = null;
                LeechersTextBlock.Text = null;
                DownloadsTextBlock.Text = null;
                return;
            }

            var item = e.AddedItems[0] as SyndicationItem;
            if (item == null) return;

            var selectedData =
                item.ElementExtensions.Where(x => x.OuterName == "seeders")
                    .Select(y => y.GetObject<XElement>().Value)
                    .ToList();
            SeedersTextBox.Text = selectedData[0];
            selectedData =
                item.ElementExtensions.Where(x => x.OuterName == "leechers")
                    .Select(y => y.GetObject<XElement>().Value)
                    .ToList();
            LeechersTextBlock.Text = selectedData[0];
            selectedData =
                item.ElementExtensions.Where(x => x.OuterName == "downloads")
                    .Select(y => y.GetObject<XElement>().Value)
                    .ToList();
            DownloadsTextBlock.Text = selectedData[0];
        }

        private void DlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var item = LstFeedItems.SelectedItems[0] as SyndicationItem;
            if (item != null) Process.Start(item.Links[0].Uri.ToString());
            e.Handled = true;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (QueryTextBox.Text != null)
                _url = "https://nyaa.si/?page=rss&q=" + QueryTextBox.Text + "&c = 0_0 & f = 0";
            CreateFeed();
        }

        private void OnLoad(object sender, RoutedEventArgs e)                               //search textbox default text
        {
            QueryTextBox.FontStyle = FontStyles.Italic;
            QueryTextBox.FontSize = 12;
            QueryTextBox.Text = "Search for torrents";
        }

        private void Focused(object sender, KeyboardFocusChangedEventArgs e)
        {
            QueryTextBox.FontStyle = FontStyles.Normal;
            QueryTextBox.FontSize = 12;
            QueryTextBox.Text = null;
        }

        private void SelectedItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == null) return;
            var item =
                ItemsControl.ContainerFromElement(LstFeedItems, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null) item.FontWeight = FontWeights.Normal;
        }

        private void QueryTextBox_EnterUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            SearchBtn_Click(null, null);
        }
    }
}
