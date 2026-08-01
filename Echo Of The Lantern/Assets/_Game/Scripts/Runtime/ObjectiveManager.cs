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

        private void Start()
        {
            BeaconCountChanged?.Invoke(ActivatedBeacons, _requiredBeacons);
        }

        public static ObjectiveManager Resolve()
        {
            if (Instance != null)
            {
                return Instance;
            }

            Instance = FindFirstObjectByType<ObjectiveManager>(FindObjectsInactive.Include);

            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject("ObjectiveManager");
            Instance = go.AddComponent<ObjectiveManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public void RegisterBeaconActivated()
        {
            if (AreAllBeaconsActivated)
            {
                return;
            }

            ActivatedBeacons = Mathf.Clamp(ActivatedBeacons + 1, 0, _requiredBeacons);
            Debug.Log($"[ObjectiveManager] Beacon activated: {ActivatedBeacons}/{_requiredBeacons}", this);

            BeaconCountChanged?.Invoke(ActivatedBeacons, _requiredBeacons);

            UIManager ui = UIManager.Resolve();
            if (ui != null)
            {
                ui.SetBeaconProgress(ActivatedBeacons, _requiredBeacons);
            }

            if (AreAllBeaconsActivated)
            {
                ShrineCanBeCompleted = true;
                Debug.Log("[ObjectiveManager] All beacons activated. Shrine unlocked.", this);

                AllBeaconsActivated?.Invoke();
                ShrineUnlocked?.Invoke();
            }
        }

        public void ResetObjectives()
        {
            ActivatedBeacons = 0;
            ShrineCanBeCompleted = false;

            Debug.Log("[ObjectiveManager] Objectives reset.", this);

            BeaconCountChanged?.Invoke(ActivatedBeacons, _requiredBeacons);

            UIManager ui = UIManager.Resolve();
            if (ui != null)
            {
                ui.SetBeaconProgress(ActivatedBeacons, _requiredBeacons);
            }
        }
    }
}