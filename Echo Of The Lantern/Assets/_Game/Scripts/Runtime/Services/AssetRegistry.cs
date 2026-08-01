// =========================================================
// FILE: AssetRegistry.cs
// PATH: Assets/_Game/Scripts/Runtime/Services/AssetRegistry.cs
// =========================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoOfTheLantern.Runtime.Services
{
    /// <summary>
    /// Central asset lookup service.
    ///
    /// Responsibilities:
    /// - Resolve gameplay assets by logical key rather than hardcoded scene references.
    /// - Fall back safely to placeholder assets when the final AI-generated asset is missing.
    /// - Keep asset lookup logic out of gameplay, UI, and scene-building systems.
    /// </summary>
    public sealed class AssetRegistry : MonoBehaviour
    {
        public static AssetRegistry Instance { get; private set; }

        [Serializable]
        public sealed class SpriteEntry
        {
            public string key;
            public Sprite sprite;
        }

        [Serializable]
        public sealed class AudioEntry
        {
            public string key;
            public AudioClip clip;
        }

        [Serializable]
        public sealed class MaterialEntry
        {
            public string key;
            public Material material;
        }

        [Header("Sprite Registry")]
        [SerializeField] private SpriteEntry[] _sprites;

        [Header("Audio Registry")]
        [SerializeField] private AudioEntry[] _audioClips;

        [Header("Material Registry")]
        [SerializeField] private MaterialEntry[] _materials;

        [Header("Fallbacks")]
        [SerializeField] private Sprite _fallbackSprite;
        [SerializeField] private AudioClip _fallbackAudioClip;
        [SerializeField] private Material _fallbackMaterial;

        private readonly Dictionary<string, Sprite> _spriteLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AudioClip> _audioLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Material> _materialLookup = new(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookups();
        }

        private void BuildLookups()
        {
            _spriteLookup.Clear();
            _audioLookup.Clear();
            _materialLookup.Clear();

            if (_sprites != null)
            {
                foreach (SpriteEntry entry in _sprites)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.sprite == null)
                        continue;

                    _spriteLookup[entry.key] = entry.sprite;
                }
            }

            if (_audioClips != null)
            {
                foreach (AudioEntry entry in _audioClips)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.clip == null)
                        continue;

                    _audioLookup[entry.key] = entry.clip;
                }
            }

            if (_materials != null)
            {
                foreach (MaterialEntry entry in _materials)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.material == null)
                        continue;

                    _materialLookup[entry.key] = entry.material;
                }
            }
        }

        public Sprite GetSprite(string key)
        {
            if (!string.IsNullOrWhiteSpace(key) && _spriteLookup.TryGetValue(key, out Sprite sprite) && sprite != null)
                return sprite;

            return _fallbackSprite;
        }

        public AudioClip GetAudioClip(string key)
        {
            if (!string.IsNullOrWhiteSpace(key) && _audioLookup.TryGetValue(key, out AudioClip clip) && clip != null)
                return clip;

            return _fallbackAudioClip;
        }

        public Material GetMaterial(string key)
        {
            if (!string.IsNullOrWhiteSpace(key) && _materialLookup.TryGetValue(key, out Material material) && material != null)
                return material;

            return _fallbackMaterial;
        }

        public bool HasSprite(string key) => !string.IsNullOrWhiteSpace(key) && _spriteLookup.ContainsKey(key);
        public bool HasAudioClip(string key) => !string.IsNullOrWhiteSpace(key) && _audioLookup.ContainsKey(key);
        public bool HasMaterial(string key) => !string.IsNullOrWhiteSpace(key) && _materialLookup.ContainsKey(key);
    }
}

