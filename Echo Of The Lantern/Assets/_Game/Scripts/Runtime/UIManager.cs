using UnityEngine;
using UnityEngine.UI;

namespace EchoOfTheLantern.Runtime
{
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Optional Scene References")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private Text _promptText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;

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

        public static UIManager Resolve()
        {
            if (Instance != null)
            {
                return Instance;
            }

            Instance = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject("UIManager");
            Instance = go.AddComponent<UIManager>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public void Bind(Text objectiveText, Text promptText, GameObject winPanel, GameObject losePanel)
        {
            _objectiveText = objectiveText;
            _promptText = promptText;
            _winPanel = winPanel;
            _losePanel = losePanel;

            HideEndPanels();
        }

        private void OnEnable()
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (objectiveManager != null)
            {
                objectiveManager.BeaconCountChanged += OnBeaconCountChanged;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.GameWon += ShowWinPanel;
                GameStateManager.Instance.GameLost += ShowLosePanel;
                GameStateManager.Instance.GameRestarted += HideEndPanels;
            }
        }

        private void OnDisable()
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (objectiveManager != null)
            {
                objectiveManager.BeaconCountChanged -= OnBeaconCountChanged;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.GameWon -= ShowWinPanel;
                GameStateManager.Instance.GameLost -= ShowLosePanel;
                GameStateManager.Instance.GameRestarted -= HideEndPanels;
            }
        }

        private void Start()
        {
            TryAutoBindFromScene();
            RefreshObjectiveText();
            HideEndPanels();
        }

        public void TryAutoBindFromScene()
        {
            if (_objectiveText != null && _promptText != null && _winPanel != null && _losePanel != null)
            {
                return;
            }

            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform t = allTransforms[i];
                if (t == null)
                {
                    continue;
                }

                if (_objectiveText == null && t.name == "ObjectiveText")
                {
                    _objectiveText = t.GetComponent<Text>();
                }
                else if (_promptText == null && t.name == "PromptText")
                {
                    _promptText = t.GetComponent<Text>();
                }
                else if (_winPanel == null && t.name == "WinPanel")
                {
                    _winPanel = t.gameObject;
                }
                else if (_losePanel == null && t.name == "LosePanel")
                {
                    _losePanel = t.gameObject;
                }

                if (_objectiveText != null && _promptText != null && _winPanel != null && _losePanel != null)
                {
                    break;
                }
            }
        }

        public void SetInteractionPrompt(string prompt)
        {
            TryAutoBindFromScene();

            if (_promptText != null)
            {
                _promptText.text = prompt;
            }
        }

        public void SetBeaconProgress(int activated, int required)
        {
            TryAutoBindFromScene();

            if (_objectiveText != null)
            {
                _objectiveText.text = $"Beacons: {activated}/{required}";
                _objectiveText.color = Color.white;
            }
        }

        public void FlashObjectiveProgress()
        {
            RefreshObjectiveText();
        }

        public void ShowWinPanel()
        {
            TryAutoBindFromScene();

            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
                Debug.Log("[UIManager] Win panel shown.", this);
            }
            else
            {
                Debug.LogWarning("[UIManager] Win panel not found.", this);
            }
        }

        public void ShowLosePanel()
        {
            TryAutoBindFromScene();

            if (_losePanel != null)
            {
                _losePanel.SetActive(true);
                Debug.Log("[UIManager] Lose panel shown.", this);
            }
            else
            {
                Debug.LogWarning("[UIManager] Lose panel not found.", this);
            }
        }

        public void HideEndPanels()
        {
            if (_winPanel != null)
            {
                _winPanel.SetActive(false);
            }

            if (_losePanel != null)
            {
                _losePanel.SetActive(false);
            }
        }

        private void RefreshObjectiveText()
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (_objectiveText != null && objectiveManager != null)
            {
                _objectiveText.text = $"Beacons: {objectiveManager.ActivatedBeacons}/3";
            }
        }

        private void OnBeaconCountChanged(int activated, int required)
        {
            SetBeaconProgress(activated, required);
        }
    }
}