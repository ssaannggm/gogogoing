// Assets/Game/Scripts/Services/RNGService.cs
using UnityEngine;

namespace Game.Services
{
    public class RNGService : MonoBehaviour
    {
        System.Random _master = new System.Random(12345);
        public void Reseed(int seed) { _master = new System.Random(seed); }
        public System.Random NewStream() => new System.Random(_master.Next());
        public int NextInt(int min, int max) => _master.Next(min, max);
        public float NextFloat() => (float)_master.NextDouble();
    }
}
