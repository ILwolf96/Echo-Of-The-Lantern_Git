// =========================================================
// FILE: WorldLayoutData.cs
// PATH: Assets/_Game/Scripts/Runtime/Data/WorldLayoutData.cs
// =========================================================
using System;
using UnityEngine;

namespace EchoOfTheLantern.Runtime.Data
{
    /// <summary>
    /// Describes the world layout in data form so the scene can be built without manual placement.
    /// </summary>
    [CreateAssetMenu(menuName = "Echo of the Lantern/World Layout Data", fileName = "SO_WorldLayoutData")]
    public sealed class WorldLayoutData : ScriptableObject
    {
        [Header("Grid Size")]
        [Min(1)] public int width = 11;
        [Min(1)] public int height = 9;

        [Header("Player Spawn")]
        public Vector2 playerSpawn = new(-4.5f, -3.75f);

        [Header("Objective Placement")]
        public Vector2[] beaconPositions =
        {
            new Vector2(-3.5f, 0f),
            new Vector2(0f, 1.75f),
            new Vector2(3.5f, -0.5f)
        };

        public Vector2 shrinePosition = new(0f, -3.25f);
        public Vector2 refillPosition = new(-0.25f, 3.0f);
        public Vector2 gatePosition = new(5.25f, -3.25f);

        [Header("Environmental Props")]
        public Vector2[] pillarPositions =
        {
            new Vector2(-3.5f, 2.5f),
            new Vector2(3.25f, 1.75f)
        };

        public Vector2[] rubblePositions =
        {
            new Vector2(-2.75f, -1.5f),
            new Vector2(2.5f, -2.2f)
        };

        public Vector2 statuePosition = new(0f, 3.4f);

        [Header("Hazards")]
        public Vector2[] hazardPositions =
        {
            new Vector2(-1.75f, -0.5f),
            new Vector2(1.75f, -1.0f),
            new Vector2(0.5f, 2.75f)
        };

        [Header("Collectibles")]
        public Vector2[] fragmentPositions =
        {
            new Vector2(-2.2f, 1.9f),
            new Vector2(2.1f, 2.1f)
        };
    }
}

