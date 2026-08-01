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
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.BeaconCountChanged += OnBeaconCountChanged;
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
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.BeaconCountChanged -= OnBeaconCountChanged;
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
            if (_objectiveText != null && ObjectiveManager.Instance != null)
            {
                OnBeaconCountChanged(ObjectiveManager.Instance.ActivatedBeacons, 3);
            }


            HideEndPanels();
        }


        public void SetInteractionPrompt(string prompt)
        {
            if (_promptText != null)
            {
                _promptText.text = prompt;
            }
        }


        public void FlashObjectiveProgress()
        {
            if (_objectiveText != null)
            {
                _objectiveText.color = Color.white;
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


        private void OnBeaconCountChanged(int activated, int required)
        {
            if (_objectiveText != null)
            {
                _objectiveText.text = $"Beacons: {activated}/{required}";
            }
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
