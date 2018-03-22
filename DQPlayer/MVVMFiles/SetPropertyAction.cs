using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;

namespace DQPlayer.MVVMFiles
{
    public class SetPropertyAction : TriggerAction<FrameworkElement>
    {
        public string PropertyName
        {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty PropertyNameProperty
            = DependencyProperty.Register("PropertyName", typeof(string),
                typeof(SetPropertyAction));

        public object PropertyValue
        {
            get => GetValue(PropertyValueProperty);
            set => SetValue(PropertyValueProperty, value);
        }

        public static readonly DependencyProperty PropertyValueProperty
            = DependencyProperty.Register("PropertyValue", typeof(object),
                typeof(SetPropertyAction));

        public object TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        public static readonly DependencyProperty TargetObjectProperty
            = DependencyProperty.Register("TargetObject", typeof(object),
                typeof(SetPropertyAction));

        protected override void Invoke(object parameter)
        {
            object target = TargetObject ?? AssociatedObject;
            PropertyInfo propertyInfo = target.GetType().GetProperty(
                PropertyName,
                BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            propertyInfo.SetValue(target, PropertyValue);
        }
    }
}