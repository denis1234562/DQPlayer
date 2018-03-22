using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace DQPlayer.Helpers.Animations
{
    public class AnimationManager
    {
        private readonly IDictionary<string, AnimationWrapper> _animationCache;
        private readonly HashSet<KeyValuePair<UIElement, AnimationWrapper>> _elementToAnimationCache;

        public AnimationManager()
        {
            _animationCache = new Dictionary<string, AnimationWrapper>();
            _elementToAnimationCache = new HashSet<KeyValuePair<UIElement, AnimationWrapper>>();
        }

        public void BeginAnimation(string name, UIElement element, TimeSpan? beginTime, Duration duration)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }
            var animation = animationWrapper.АnimationInitializer.Invoke();
            animation.BeginTime = beginTime;
            animation.Duration = duration;
            BeginAnimationImpl(animationWrapper, animation, element);
        }

        public void BeginAnimation(string name, UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }
            var animation = animationWrapper.АnimationInitializer.Invoke();

            BeginAnimationImpl(animationWrapper, animation, element);
        }

        private void BeginAnimationImpl(AnimationWrapper animationWrapper, AnimationTimeline animation, UIElement element)
        {
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

        public void CancelAnimation(string name, UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!_animationCache.TryGetValue(name, out var animationWrapper))
            {
                throw new KeyNotFoundException(nameof(name));
            }
            element.BeginAnimation(animationWrapper.TargetProperty, null);
        }

        public bool AddAnimation(AnimationWrapper animation)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }
            if (_animationCache.ContainsKey(animation.Name))
            {
                return false;
            }
            _animationCache.Add(animation.Name, animation);
            return true;
        }

        public AnimationWrapper GetAnimation(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _animationCache.TryGetValue(name, out var animation) ? animation : null;
        }

        public bool RemoveAnimation(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _animationCache.Remove(name);
        }

        public bool RemoveAnimation(AnimationWrapper animation)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }
            return RemoveAnimation(animation.Name);
        }
    }
}
