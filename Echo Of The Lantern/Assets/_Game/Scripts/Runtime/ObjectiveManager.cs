using System;
using UnityEngine;


namespace EchoOfTheLantern.Runtime
{
    public sealed class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager Instance { get; private set; }


        [Header("Objective Tuning")]
        [SerializeField, Min(1)] private int _requiredBeacons = 3;


        public int ActivatedBeacons { get; private set; }
        public bool AreAllBeaconsActivated => ActivatedBeacons >= _requiredBeacons;
        public bool ShrineCanBeCompleted { get; private set; }


        public event Action<int, int> BeaconCountChanged;
        public event Action AllBeaconsActivated;
        public event Action ShrineUnlocked;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }


            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        private void OnEnable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.GameRestarted += ResetObjectives;
                GameStateManager.Instance.GameStarted += ResetObjectives;
            }
        }


        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.GameRestarted -= ResetObjectives;
                GameStateManager.Instance.GameStarted -= ResetObjectives;
            }
        }


        public void RegisterBeaconActivated()
        {
            if (AreAllBeaconsActivated)
            {
                return;
            }


            ActivatedBeacons = Mathf.Clamp(ActivatedBeacons + 1, 0, _requiredBeacons);
            BeaconCountChanged?.Invoke(ActivatedBeacons, _requiredBeacons);


            if (AreAllBeaconsActivated)
            {
                ShrineCanBeCompleted = true;
                AllBeaconsActivated?.Invoke();
                ShrineUnlocked?.Invoke();
            }
        }


        public void ResetObjectives()
        {
            ActivatedBeacons = 0;
            ShrineCanBeCompleted = false;
            BeaconCountChanged?.Invoke(ActivatedBeacons, _requiredBeacons);
        }
    }
}
