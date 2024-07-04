using System.Numerics;

namespace Thingimajig
{
    public struct RayCollision
    {

        public byte Collides { get; set; }

        public Vector3 CollisionPoint { get; set; }

        public Vector3 Normal { get; set; }

        public float Distance { get; set; }

        public Material Material { get; set; }

        public int SphereIndex { get; set; }
    }
}
