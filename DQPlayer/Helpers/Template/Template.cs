﻿using System;
using System.Collections.Generic;

namespace DQPlayer.Helpers.Template
{
    public class Template<TSource, TBase>
        where TSource : TBase, new()
    {
        private readonly List<Action<TSource>> _actions;

        public Template()
        {
            _actions = new List<Action<TSource>>();
        }

        public Template<TSource, TBase> WithArgument(Action<TSource> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            _actions.Add(action);
            return this;
        }

        public TSource Clone()
        {
            return CloneAndOverride(new TSource());
        }

        public TSource CloneAndOverride(TSource source)
        {
            foreach (var action in _actions)
            {
                action.Invoke(source);
            }
            return source;
        }
    }

    public class Template<TSource>
        where TSource : new()
    {
        private readonly List<Action<TSource>> _actions;

        public Template()
        {
            _actions = new List<Action<TSource>>();
        }

        public Template<TSource> WithArgument(Action<TSource> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            _actions.Add(action);
            return this;
        }

        public TSource Clone()
        {
            return CloneAndOverride(new TSource());
        }

        public TSource CloneAndOverride(TSource source)
        {
            foreach (var action in _actions)
            {
                action.Invoke(source);
            }
            return source;
        }
    }
}
