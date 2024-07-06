using System.Numerics;

namespace Thingimajig
{
    public struct RayCollision
    {
        // bool is not blittable, so kernel gets angry
        public byte Collides { get; set; }

        public Vector3 CollisionPoint { get; set; }

        public Vector3 Normal { get; set; }

        public float Distance { get; set; }

        public Material Material { get; set; }
    }
}
