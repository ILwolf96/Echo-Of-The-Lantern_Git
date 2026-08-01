using EchoOfTheLantern.Runtime.Data;
using EchoOfTheLantern.Runtime.Services;
using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    public sealed class WorldBuilder : MonoBehaviour
    {
        [SerializeField] private WorldLayoutData _layout;

        [Header("Optional Prefab References")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _beaconPrefab;
        [SerializeField] private GameObject _shrinePrefab;
        [SerializeField] private GameObject _refillPrefab;
        [SerializeField] private GameObject _gatePrefab;
        [SerializeField] private GameObject _hazardPrefab;
        [SerializeField] private GameObject _fragmentPrefab;
        [SerializeField] private GameObject _groundPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _mistPrefab;

        private GameObject _root;

        private void Start()
        {
            EnsureCoreSystems();
            BuildWorld();
        }

        private void EnsureCoreSystems()
        {
            if (GameStateManager.Instance == null)
            {
                new GameObject("GameStateManager").AddComponent<GameStateManager>();
            }

            if (ObjectiveManager.Instance == null)
            {
                new GameObject("ObjectiveManager").AddComponent<ObjectiveManager>();
            }

            if (AssetRegistry.Instance == null)
            {
                new GameObject("AssetRegistry").AddComponent<AssetRegistry>();
            }
        }

        public void BuildWorld()
        {
            if (_root == null)
            {
                _root = GameObject.Find("WorldRoot");
                if (_root == null)
                {
                    _root = new GameObject("WorldRoot");
                    DontDestroyOnLoad(_root);
                }
            }

            BuildPlayer();
            BuildObjectives();
            BuildHazards();
        }

        private void BuildPlayer()
        {
            Vector2 spawn = _layout != null ? _layout.playerSpawn : new Vector2(-4.5f, -3.75f);
            SpawnOrFallback(_playerPrefab, "Player", spawn, "SPR_Player_Idle", new Vector2(0.45f, 0.65f), false, true, 0);
        }

        private void BuildObjectives()
        {
            if (_layout == null) return;

            foreach (Vector2 pos in _layout.beaconPositions)
                SpawnOrFallback(_beaconPrefab, "Beacon", pos, "SPR_Beacon_Off", new Vector2(0.85f, 0.85f), true, false, 3);

            SpawnOrFallback(_shrinePrefab, "Shrine", _layout.shrinePosition, "SPR_Shrine", new Vector2(1.2f, 1.2f), true, false, 3);
            SpawnOrFallback(_refillPrefab, "Refill", _layout.refillPosition, "SPR_Refill", new Vector2(0.8f, 0.8f), true, false, 3);
            SpawnOrFallback(_gatePrefab, "Gate", _layout.gatePosition, "SPR_Gate_Closed", new Vector2(1.2f, 1.2f), true, false, 3);

            foreach (Vector2 pos in _layout.fragmentPositions)
                SpawnOrFallback(_fragmentPrefab, "Fragment", pos, "SPR_Fragment", new Vector2(0.4f, 0.4f), true, false, 3);
        }

        private void BuildHazards()
        {
            if (_layout == null) return;

            foreach (Vector2 pos in _layout.hazardPositions)
                SpawnOrFallback(_hazardPrefab, "Hazard", pos, "SPR_Shadow_Hazard", new Vector2(1f, 1f), true, false, 3);
        }

        private GameObject SpawnOrFallback(GameObject prefab, string name, Vector2 position, string spriteKey, Vector2 colliderSize, bool isTrigger, bool addRigidbody, int sortingOrder)
        {
            GameObject instance;

            if (prefab != null)
            {
                instance = Instantiate(prefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, _root.transform);
            }
            else
            {
                instance = CreateFallbackObject(name, position, spriteKey, colliderSize, isTrigger, addRigidbody, sortingOrder);
                instance.transform.SetParent(_root.transform, true);
            }

            instance.name = name;
            return instance;
        }

        private GameObject CreateFallbackObject(string name, Vector2 position, string spriteKey, Vector2 colliderSize, bool isTrigger, bool addRigidbody, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(position.x, position.y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.Instance != null ? AssetRegistry.Instance.GetSprite(spriteKey) : null;
            sr.sortingOrder = sortingOrder;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = colliderSize;
            col.isTrigger = isTrigger;

            if (addRigidbody)
            {
                Rigidbody2D body = go.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            if (name == "Player")
            {
                go.AddComponent<PlayerController>();
            }

            return go;
        }
    }
}