using System;
using System.Configuration;
using System.Windows.Controls;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PagesListView.SelectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

            switch (PagesListView.SelectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage();
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage();
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this);
        }
    }
}