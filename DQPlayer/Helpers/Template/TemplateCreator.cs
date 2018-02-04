﻿using System;

namespace DQPlayer.Helpers.Template
{
    public class TemplateCreator<TSource, TBase>
        where TSource : TBase, new()
    {
        private readonly Template<TSource, TBase> _sourceOfTemplate;

        public TemplateCreator(Template<TSource, TBase> sourceOfTemplate)
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

    public class TemplateCreator<TSource>
        where TSource : new()
    {
        private readonly Template<TSource> _sourceOfTemplate;

        public TemplateCreator(Template<TSource> sourceOfTemplate)
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
