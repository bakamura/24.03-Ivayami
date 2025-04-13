using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ivayami.Save;
using Ivayami.Audio;

namespace Ivayami.Scene
{
    public sealed class ClockTower : MonoBehaviour
    {
        [SerializeField] private ClockTowerEventRules[] _events;

        private List<ClockTowerEventRules> _currentValidRules = new List<ClockTowerEventRules>();
        private Coroutine _eventIntervalCoroutine;
        private ClockTowerEventActive _currentEventActive;
        private SoundEffectTrigger _sound;

        private struct ClockTowerEventActive
        {
            public ClockTowerEventRules Rules;
            public GameObject Instance;
            public static ClockTowerEventActive Empty = new ClockTowerEventActive();

            public ClockTowerEventActive(ClockTowerEventRules rules, GameObject instance)
            {
                Rules = rules;
                Instance = instance;
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
            _currentEventActive.Rules.ClockTowerEvent.InterruptEvent();
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
                Destroy(_currentEventActive.Instance);
                _currentEventActive = ClockTowerEventActive.Empty;
            }
        }

        private void StartNewEvent()
        {
            _eventIntervalCoroutine = StartCoroutine(EventIntervalCoroutine());
        }

        private IEnumerator EventIntervalCoroutine()
        {
            _currentEventActive = new ClockTowerEventActive(_currentValidRules[Random.Range(0, _currentValidRules.Count)], null);
            _currentEventActive.Rules.ClockTowerEvent.OnEndEvent += DestroyCurrentEvent;
            _currentEventActive.Rules.ClockTowerEvent.OnEndEvent += StartNewEvent;
            _currentEventActive.Rules.ClockTowerEvent.OnInterruptEvent += DestroyCurrentEvent;
            WaitForSeconds delay = new WaitForSeconds(Random.Range(_currentEventActive.Rules.EventIntervalToStartRange.Min, _currentEventActive.Rules.EventIntervalToStartRange.Max));
            yield return delay;
            GameObject instance = Instantiate(_currentEventActive.Instance, transform);
            _currentEventActive.Instance = instance;            
            _currentEventActive.Rules.ClockTowerEvent.StartEvent(_currentEventActive.Rules.EventDuration);
            _sound.Play();
            _eventIntervalCoroutine = null;
        }
    }
}