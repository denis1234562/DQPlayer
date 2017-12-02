using System;
using DQPlayer.MVVMFiles.View;
using EnvDTE;

namespace DQPlayer
{
    public class WindowService : IWindowService
    {
        public PlayList PlayListView { get; }

        public WindowService()
        {
            PlayListView = new PlayList();
        }

        public void ShowWindow(object dataContext)
        {
            PlayListView.DataContext = dataContext;
            PlayListView.Hide();
        }
    }
}
