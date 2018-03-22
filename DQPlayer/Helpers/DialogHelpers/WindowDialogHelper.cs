using System;
using System.Linq;
using System.Windows;

namespace DQPlayer.Helpers.DialogHelpers
{
    public class WindowDialogHelper<T>
        where T : Window, new()
    {
        private static WindowDialogHelper<T> _instance;
        public static WindowDialogHelper<T> Instance => _instance ?? (_instance = new WindowDialogHelper<T>());

        private T _windowInstance;

        public object DataContext
        {
            get => _windowInstance.DataContext;
            set => _windowInstance.DataContext = value;
        }

        public void ShowDialog()
        {
            CreateNewInstanceIfNull(() => !_windowInstance.IsLoaded);
            _windowInstance.ShowDialog();
        }

        public void Show()
        {
            CreateNewInstanceIfNull(() => !_windowInstance.IsLoaded);
            _windowInstance.Show();
        }

        public void Hide()
        {
            CreateNewInstanceIfNull();
            _windowInstance.Hide();
        }

        private void CreateNewInstanceIfNull(params Func<bool>[] predicates)
        {
            if (_windowInstance == null || predicates.Any(p => p.Invoke()))
            {
                _windowInstance = new T();
            }
        }
    }
}
