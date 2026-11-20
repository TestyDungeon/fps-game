#if UNITY_EDITOR
using System;
using UnityEngine;

namespace RealtimeCSG.UnityWrappers
{
    public class ObjectDispatcher
    {
        // Wrapped types
        public enum TransformTrackingType
        {
            GlobalTRS,
            LocalTRS,
            Hierarchy
        }

        public ObjectDispatcher()
        {
        }

        // Wrapped methods
        public void EnableTransformTracking<T>(TransformTrackingType trackingType) where T : UnityEngine.Object
        {
        }

        public void Dispose()
        {
        }

        public Component[] GetTransformChangesAndClear<T>(TransformTrackingType trackingType, bool v) where T : UnityEngine.Object
        {
            return Array.Empty<Component>();
        }
    }

}
#endif