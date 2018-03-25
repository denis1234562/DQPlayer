using System;
using System.Linq;
using System.Windows;

namespace DQPlayer.Helpers.DialogHelpers
{
    public sealed class WindowDialogHelper<T>
        where T : Window, new()
    {
        private static readonly object _padlock = new object();

        private static readonly Lazy<WindowDialogHelper<T>> _instance =
            new Lazy<WindowDialogHelper<T>>(() => new WindowDialogHelper<T>());
        public static WindowDialogHelper<T> Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance.Value;
                }
            }
        }

        private T _windowInstance;

        private WindowDialogHelper()
        {
            
        }

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
