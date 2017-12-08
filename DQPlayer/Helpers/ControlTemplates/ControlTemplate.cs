using System;
using System.Collections.Generic;
using System.Windows;

namespace DQPlayer.Helpers.ControlTemplates
{
    public class ControlTemplate<TSource>
        where TSource : UIElement
    {
        private readonly List<Action<TSource>> _controlActions;

        public ControlTemplate()
        {
            _controlActions = new List<Action<TSource>>();
        }

        public ControlTemplate<TSource> WithArgument(Action<TSource> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            _controlActions.Add(action);
            return this;
        }

        public TSource Clone()
        {
            TSource control = Activator.CreateInstance<TSource>();
            foreach (var action in _controlActions)
            {
                action.Invoke(control);
            }
            return control;
        }

        public TSource CloneAndOverride(TSource source)
        {
            foreach (var action in _controlActions)
            {
                action.Invoke(source);
            }
            return source;
        }
    }
}
