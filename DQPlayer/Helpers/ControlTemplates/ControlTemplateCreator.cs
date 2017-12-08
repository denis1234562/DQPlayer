using System;
using System.Windows;

namespace DQPlayer.Helpers.ControlTemplates
{
    public class ControlTemplateCreator<TSource>
        where TSource : UIElement
    {
        private readonly ControlTemplate<TSource> _sourceOfTemplate;

        public ControlTemplateCreator(ControlTemplate<TSource> sourceOfTemplate)
        {
            _sourceOfTemplate = sourceOfTemplate ?? throw new ArgumentNullException(nameof(sourceOfTemplate));
        }

        public TSource Clone()
        {
            return _sourceOfTemplate.Clone();
        }

        public TSource CloneAndOverride(TSource source)
        {
            return _sourceOfTemplate.CloneAndOverride(source);
        }
    }
}
