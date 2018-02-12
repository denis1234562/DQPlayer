using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DQPlayer.Helpers.Adorners;
using DQPlayer.MVVMFiles.ViewModels;

namespace DQPlayer.MVVMFiles.Views
{
    public partial class PlayList
    {
        public PlayListViewModel ViewModel => DataContext as PlayListViewModel;

        private GridViewColumnHeader listViewSortCol;
        private SortAdorner listViewSortAdorner;

        public PlayList()
        {
            InitializeComponent(); 
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void lvMediaFilesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = (GridViewColumnHeader) sender;
            var sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvListView.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;
            if (Equals(listViewSortCol, column) && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }
}
