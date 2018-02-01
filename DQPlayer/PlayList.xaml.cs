using DQPlayer.MVVMFiles.Models.PlayList;
using DQPlayer.MVVMFiles.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DQPlayer
{
    /// <summary>
    /// Interaction logic for PlayList.xaml
    /// </summary>
    public partial class PlayList : Window
    {
        private PlayListViewModel ViewModel;

        public PlayList()
        {
            InitializeComponent();
            DataContextChanged += PlayList_DataContextChanged;     
        }

        private void PlayList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = (PlayListViewModel)DataContext;
            ViewModel.Loaded += ViewModel_Loaded;
        }

        private void ViewModel_Loaded(object obj)
        {
            ViewModel.FilesCollection.CollectionChanged += FilesCollection_CollectionChanged;
        }

        private void FilesCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {          
            var n = e.NewItems;
            FileInformation fl = (FileInformation)n[0];
            fl.PropertyChanged += Temp_PropertyChanged;
        }

        private void Temp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {                      
            var index = ViewModel.FilesCollection.IndexOf((FileInformation)sender);
            var item = (ListViewItem)lvListView.Items[index];
            item.Background = Brushes.Yellow;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
