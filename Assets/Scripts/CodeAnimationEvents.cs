using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight, code-only animation event system for Unity's built-in Animator.
///
/// Provides an Animancer-like API:
///
///     var events = animationEvents.GetEvents("Attack");
///
///     events.Clear();
///     events.Add(0.25f, SomeMethod);
///     events.Add(0.5f, AnotherMethod);
///
/// Events are associated with an Animator state and are evaluated using
/// normalized animation time.
///
/// This does not require Animation Events to be added to AnimationClips.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CodeAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    // =====================================================================
    // Animation Event Sequence
    // =====================================================================

    /// <summary>
    /// A collection of callbacks associated with a particular Animator state.
    ///
    /// This is intentionally similar to Animancer's event sequence API.
    /// </summary>
    public sealed class EventSequence
    {
        private readonly List<TimedEvent> _events = new();

        /// <summary>
        /// Adds an event at the specified normalized time.
        ///
        /// 0 = beginning of animation
        /// 1 = end of animation
        /// </summary>
        public void Add(float normalizedTime, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            normalizedTime = Mathf.Clamp01(normalizedTime);

            _events.Add(new TimedEvent
            {
                normalizedTime = normalizedTime,
                callback = callback
            });
        }

        /// <summary>
        /// Adds an event at the specified normalized time.
        ///
        /// If repeating is true, the event fires every time the animation loops.
        /// Otherwise it fires once per playback.
        /// </summary>
        public void Add(
            float normalizedTime,
            Action callback,
            bool repeating)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            normalizedTime = Mathf.Clamp01(normalizedTime);

            _events.Add(new TimedEvent
            {
                normalizedTime = normalizedTime,
                callback = callback,
                repeating = repeating
            });
        }

        /// <summary>
        /// Removes all events from this sequence.
        ///
        /// Equivalent to:
        ///
        ///     events.Clear();
        ///
        /// in Animancer.
        /// </summary>
        public void Clear()
        {
            _events.Clear();
        }

        /// <summary>
        /// Returns the number of registered events.
        /// </summary>
        public int Count => _events.Count;

        /// <summary>
        /// Removes the event at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            _events.RemoveAt(index);
        }

        /// <summary>
        /// Resets the fired state of all one-shot events.
        /// </summary>
        internal void Reset()
        {
            foreach (var animationEvent in _events)
                animationEvent.hasFired = false;
        }

        /// <summary>
        /// Processes events between two normalized animation times.
        ///
        /// TimedEvent itself remains private, so implementation details
        /// aren't exposed outside this class.
        /// </summary>
        internal void InvokeEvents(
            float previousTime,
            float currentTime,
            bool stateChanged,
            bool wrappedLoop)
        {
            foreach (var animationEvent in _events)
            {
                bool crossed;

                if (stateChanged)
                {
                    // State just started.
                    //
                    // If the first Update happens after the event's
                    // normalized time, consider the event crossed.
                    crossed =
                        animationEvent.normalizedTime <= currentTime;
                }
                else if (wrappedLoop)
                {
                    // Animation looped.
                    //
                    // Example:
                    //
                    // previous = 0.90
                    // current  = 0.10
                    //
                    // Events at 0.95 and 0.05 were crossed.
                    crossed =
                        animationEvent.normalizedTime > previousTime ||
                        animationEvent.normalizedTime <= currentTime;
                }
                else
                {
                    // Normal forward playback.
                    crossed =
                        animationEvent.normalizedTime > previousTime &&
                        animationEvent.normalizedTime <= currentTime;
                }

                if (!crossed)
                    continue;

                // Non-repeating events only fire once per playback.
                if (!animationEvent.repeating &&
                    animationEvent.hasFired)
                {
                    continue;
                }

                animationEvent.hasFired = true;

                animationEvent.callback?.Invoke();
            }
        }
    }

    // =====================================================================
    // Internal Event Data
    // =====================================================================

    private sealed class TimedEvent
    {
        public float normalizedTime;
        public Action callback;

        /// <summary>
        /// If true, fires every time the animation loops.
        /// Otherwise fires once per playback.
        /// </summary>
        public bool repeating;

        public bool hasFired;
    }

    // =====================================================================
    // State Tracking
    // =====================================================================

    private sealed class LayerTracker
    {
        public int currentStateHash;

        public float previousRawTime;
        public float previousWrappedTime;

        public bool hasState;
    }

    private readonly Dictionary<int, LayerTracker> _layers = new();

    /// <summary>
    /// Each Animator state owns its own EventSequence.
    ///
    /// This is what gives us the Animancer-like:
    ///
    ///     var events = GetEvents("Attack");
    ///     events.Clear();
    ///     events.Add(...);
    /// </summary>
    private readonly Dictionary<(int layer, int stateHash), EventSequence>
        _eventSequences = new();

    // =====================================================================
    // Unity
    // =====================================================================

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_animator == null)
            return;

        int layerCount = _animator.layerCount;

        for (int layer = 0; layer < layerCount; layer++)
            ProcessLayer(layer);
    }

    // =====================================================================
    // Public API
    // =====================================================================

    /// <summary>
    /// Gets the event sequence associated with an Animator state.
    ///
    /// Example:
    ///
    ///     var events = animationEvents.GetEvents("Attack");
    ///
    ///     events.Clear();
    ///     events.Add(0.25f, Attack);
    /// </summary>
    public EventSequence GetEvents(
        string stateName,
        int layer = 0)
    {
        if (string.IsNullOrEmpty(stateName))
            throw new ArgumentException(
                "State name cannot be null or empty.",
                nameof(stateName));

        int stateHash = Animator.StringToHash(stateName);

        var key = (layer, stateHash);

        if (!_eventSequences.TryGetValue(key, out var sequence))
        {
            sequence = new EventSequence();
            _eventSequences.Add(key, sequence);
        }

        return sequence;
    }

    /// <summary>
    /// Removes the entire event sequence associated with a state.
    /// </summary>
    public void ClearEvents(
        string stateName,
        int layer = 0)
    {
        if (string.IsNullOrEmpty(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);

        _eventSequences.Remove((layer, stateHash));
    }

    /// <summary>
    /// Removes every registered event sequence.
    /// </summary>
    public void ClearAllEvents()
    {
        _eventSequences.Clear();
    }

    // =====================================================================
    // State Processing
    // =====================================================================

    private void ProcessLayer(int layer)
    {
        AnimatorStateInfo info =
            _animator.GetCurrentAnimatorStateInfo(layer);
    
        // IMPORTANT:
        // GetEvents("Attack") uses Animator.StringToHash("Attack"),
        // so we must use shortNameHash here rather than fullPathHash.
        int stateHash = info.shortNameHash;
    
        if (!_layers.TryGetValue(layer, out var tracker))
        {
            tracker = new LayerTracker();
            _layers.Add(layer, tracker);
        }
    
        bool stateChanged =
            !tracker.hasState ||
            tracker.currentStateHash != stateHash;
    
        if (stateChanged)
        {
            tracker.currentStateHash = stateHash;
            tracker.previousRawTime = 0f;
            tracker.previousWrappedTime = 0f;
            tracker.hasState = true;
    
            if (_eventSequences.TryGetValue(
                (layer, stateHash),
                out var sequence))
            {
                sequence.Reset();
            }
        }
    
        float rawTime = info.normalizedTime;
        float wrappedTime = Mathf.Repeat(rawTime, 1f);
    
        bool animationRestarted =
            !stateChanged &&
            rawTime < tracker.previousRawTime;
    
        if (animationRestarted)
        {
            if (_eventSequences.TryGetValue(
                (layer, stateHash),
                out var sequence))
            {
                sequence.Reset();
            }
        }
    
        if (_eventSequences.TryGetValue(
            (layer, stateHash),
            out var events))
        {
            bool wrappedLoop =
                !stateChanged &&
                !animationRestarted &&
                wrappedTime < tracker.previousWrappedTime;
    
            events.InvokeEvents(
                tracker.previousWrappedTime,
                wrappedTime,
                stateChanged || animationRestarted,
                wrappedLoop);
        }
    
        tracker.previousRawTime = rawTime;
        tracker.previousWrappedTime = wrappedTime;
    }
}