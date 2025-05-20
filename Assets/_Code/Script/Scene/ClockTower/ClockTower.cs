using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ivayami.Save;
using Ivayami.Audio;

namespace Ivayami.Scene
{
    public sealed class ClockTower : MonoBehaviour
    {
        [SerializeField] private bool _debugLogs;
        [SerializeField] private ClockTowerEventRules[] _events;

        private List<ClockTowerEventRules> _currentValidRules = new List<ClockTowerEventRules>();
        private Coroutine _eventIntervalCoroutine;
        private ClockTowerEventActive _currentEventActive;
        private SoundEffectTrigger _sound;

        private struct ClockTowerEventActive
        {
            public readonly ClockTowerEventRules Rules;
            public ClockTowerEvent Instance;
            public static ClockTowerEventActive Empty = new ClockTowerEventActive();

            public ClockTowerEventActive(ClockTowerEventRules rules)
            {
                Rules = rules;
                Instance = null;
            }
        }

        private void Start()
        {
            if (!SaveSystem.Instance || SaveSystem.Instance.Progress == null) return;
            _sound = GetComponentInChildren<SoundEffectTrigger>();
            SavePoint.onSaveGame.AddListener(UpdateCurrentEventsList);
            UpdateCurrentEventsList();
            StartNewEvent();
        }

        private void OnEnable()
        {
            if (!SaveSystem.Instance || SaveSystem.Instance.Progress == null) return;
            if (_currentValidRules.Count > 0) StartNewEvent();
        }

        private void OnDisable()
        {
            if (_eventIntervalCoroutine != null)
            {
                StopCoroutine(_eventIntervalCoroutine);
                _eventIntervalCoroutine = null;
            }
            if(_currentEventActive.Instance) _currentEventActive.Instance.InterruptEvent();
        }

        private void OnDestroy()
        {
            SavePoint.onSaveGame.RemoveListener(UpdateCurrentEventsList);
        }

        private void UpdateCurrentEventsList()
        {
            _currentValidRules.Clear();
            for (int i = 0; i < _events.Length; i++)
            {
                if (_events[i].IsCurrentlyValid()) _currentValidRules.Add(_events[i]);
            }
        }

        private void DestroyCurrentEvent()
        {
            if (_currentEventActive.Instance)
            {
                Destroy(_currentEventActive.Instance.gameObject);
                _currentEventActive = ClockTowerEventActive.Empty;
            }
        }

        private void StartNewEvent()
        {
            _eventIntervalCoroutine = StartCoroutine(EventIntervalCoroutine());
        }

        private IEnumerator EventIntervalCoroutine()
        {
            _currentEventActive = new ClockTowerEventActive(_currentValidRules[Random.Range(0, _currentValidRules.Count)]);
            float time = Random.Range(_currentEventActive.Rules.EventIntervalToStartRange.Min, _currentEventActive.Rules.EventIntervalToStartRange.Max);
            WaitForSeconds delay = new WaitForSeconds(time);
            if(_debugLogs) Debug.Log($"Event {_currentEventActive.Rules.name} selected, will start after {time} seconds");
            yield return delay;
            _currentEventActive.Instance = Instantiate(_currentEventActive.Rules.ClockTowerEvent.gameObject, transform).GetComponent<ClockTowerEvent>();
            _currentEventActive.Instance.OnEndEvent += DestroyCurrentEvent;
            _currentEventActive.Instance.OnEndEvent += StartNewEvent;
            _currentEventActive.Instance.OnInterruptEvent += DestroyCurrentEvent;            
            _currentEventActive.Instance.StartEvent(_currentEventActive.Rules.EventDuration, _debugLogs);
            _sound.Play();
            _eventIntervalCoroutine = null;
        }
    }
}