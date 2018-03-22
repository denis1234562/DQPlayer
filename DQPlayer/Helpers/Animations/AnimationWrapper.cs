using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DQPlayer.Helpers.Animations
{
    public class AnimationWrapper : IEquatable<AnimationWrapper>
    {
        public delegate void AnimationCompleted(object sender, EventArgs e, UIElement element);

        public Func<AnimationTimeline> АnimationInitializer { get; }
        public DependencyProperty TargetProperty { get; }
        public string Name { get; }

        public AnimationCompleted OnCompleted { get; }

        public AnimationWrapper(string name, Func<AnimationTimeline> animationInitializer,
            DependencyProperty targetProperty,
            AnimationCompleted onCompleted)
        {
            Name = name;
            АnimationInitializer = animationInitializer;
            TargetProperty = targetProperty;
            OnCompleted = onCompleted;
        }

        public AnimationWrapper(string name, Func<AnimationTimeline> animationInitializer,
            DependencyProperty targetProperty)
            : this(name, animationInitializer, targetProperty, null)
        {
        }

        #region Equality members

        public bool Equals(AnimationWrapper other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(OnCompleted, other.OnCompleted) && Equals(АnimationInitializer, other.АnimationInitializer) &&
                   Equals(TargetProperty, other.TargetProperty) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AnimationWrapper)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = OnCompleted != null ? OnCompleted.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (АnimationInitializer != null ? АnimationInitializer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TargetProperty != null ? TargetProperty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
