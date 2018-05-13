using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using DQPlayer.Annotations;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.ObjectPooling;

namespace DQPlayer.Helpers.Animations
{
    public class AnimationManager
    {
        private readonly ObservableDictionary<string, AnimationWrapper> _animationCache;
        private readonly HashSet<KeyValuePair<UIElement, AnimationWrapper>> _elementToAnimationCache;
        private readonly IDictionary<string, ObjectPooler<AnimationTimeline>> _animationObjectPoolers;

        public AnimationManager()
        {
            _animationCache = new ObservableDictionary<string, AnimationWrapper>();
            _elementToAnimationCache = new HashSet<KeyValuePair<UIElement, AnimationWrapper>>();
            _animationObjectPoolers = new Dictionary<string, ObjectPooler<AnimationTimeline>>();

            _animationCache.CollectionChanged += AnimationCache_OnCollectionChanged;
        }

        private void AnimationCache_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var addedAnimation = (KeyValuePair<string, AnimationWrapper>) e.NewItems[0];
                AddObjectPooler(addedAnimation);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removedAnimation = (KeyValuePair<string, AnimationWrapper>) e.OldItems[0];
                _animationObjectPoolers.Remove(removedAnimation.Key);
            }
        }

        private void AddObjectPooler(KeyValuePair<string, AnimationWrapper> animationWrapper)
        {
            _animationObjectPoolers.Add(animationWrapper.Key,
                new ObjectPooler<AnimationTimeline>(
                    new PooledObject<AnimationTimeline>(animationWrapper.Value.АnimationInitializer), 100,
                    PoolRefillMethod.WholePool));
        }

        public void BeginAnimation(
            [NotNull] string name, 
            [NotNull] UIElement element,
            TimeSpan? beginTime,
            Duration duration)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }
            BeginAnimationImpl(animationWrapper, element, animation =>
            {
                animation.BeginTime = beginTime;
                animation.Duration = duration;
            });
        }

        public void BeginAnimation(string name, UIElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }

            BeginAnimationImpl(animationWrapper, element);
        }

        private void BeginAnimationImpl(
            AnimationWrapper animationWrapper, 
            UIElement element, 
            Action<AnimationTimeline> modifiers = null)
        {
            var animation = _animationObjectPoolers[animationWrapper.Name].GetObject();
            modifiers?.Invoke(animation);
            if (animationWrapper.OnCompleted != null)
            {
                var kvp = new KeyValuePair<UIElement, AnimationWrapper>(element, animationWrapper);
                if (_elementToAnimationCache.Add(kvp))
                {
                    animation.Completed += (sender, args) => animationWrapper.OnCompleted(sender, args, element);
                }
            }
            element.BeginAnimation(animationWrapper.TargetProperty, animation);
        }

        public void CancelAnimation([NotNull]string name, [NotNull] UIElement element)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }
            element.BeginAnimation(animationWrapper.TargetProperty, null);
        }

        public bool AddAnimation([NotNull] AnimationWrapper animation)
        {
            if (animation == null) throw new ArgumentNullException(nameof(animation));

            if (_animationCache.ContainsKey(animation.Name))
            {
                return false;
            }
            _animationCache.Add(animation.Name, animation);
            return true;
        }

        public AnimationWrapper GetAnimation([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _animationCache.TryGetValue(name, out var animation) ? animation : null;
        }

        public bool RemoveAnimation([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _animationCache.Remove(name);
        }

        public bool RemoveAnimation([NotNull] AnimationWrapper animation)
        {
            if (animation == null) throw new ArgumentNullException(nameof(animation));

            return RemoveAnimation(animation.Name);
        }
    }
}
