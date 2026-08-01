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

        private bool _isBound;

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

            _isBound = true;
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
                GameStateManager.Instance.GameWon += ShowWin;
                GameStateManager.Instance.GameLost += ShowLose;
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
                GameStateManager.Instance.GameWon -= ShowWin;
                GameStateManager.Instance.GameLost -= ShowLose;
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
            if (_objectiveText == null)
            {
                GameObject obj = GameObject.Find("ObjectiveText");
                if (obj != null)
                {
                    _objectiveText = obj.GetComponent<Text>();
                }
            }

            if (_promptText == null)
            {
                GameObject obj = GameObject.Find("PromptText");
                if (obj != null)
                {
                    _promptText = obj.GetComponent<Text>();
                }
            }

            if (_winPanel == null)
            {
                _winPanel = GameObject.Find("WinPanel");
            }

            if (_losePanel == null)
            {
                _losePanel = GameObject.Find("LosePanel");
            }

            _isBound = _objectiveText != null || _promptText != null || _winPanel != null || _losePanel != null;
        }

        public void SetInteractionPrompt(string prompt)
        {
            if (_promptText != null)
            {
                _promptText.text = prompt;
            }
        }

        public void SetBeaconProgress(int activated, int required)
        {
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

        private void ShowWin()
        {
            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
            }
        }

        private void ShowLose()
        {
            if (_losePanel != null)
            {
                _losePanel.SetActive(true);
            }
        }
    }
}