using System.Numerics;
using Thingimajig;

public abstract class IObject
{
    public Vector3 Position { get; set; }

    public Material Material;

    public abstract RayCollision GetCollision(Ray ray);
}
