using System.Numerics;
using Thingimajig;

public struct Sphere
{
    public float Radius;
    public Vector3 Position { get; set; }

    public Material Material;

    public RayCollision GetCollision(Ray ray)
    {
        Vector3 oc = ray.Origin - Position;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = 2 * Vector3.Dot(oc, ray.Direction);
        float c = Vector3.Dot(oc, oc) - Radius * Radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 1E-6f)
        {
            return new RayCollision
            {
                Collides = 0
            };
        }

        float distance = (-b - MathF.Sqrt(discriminant)) / (2 * a);

        if (distance < 1E-6f)
        {
            return new RayCollision
            {
                Collides = 0
            };
        }

        Vector3 collisonPoint = ray.Origin + ray.Direction * distance;

        return new RayCollision
        {
            Collides = 1,
            Distance = distance,
            CollisionPoint = collisonPoint,
            Material = Material,
            Normal = Vector3.Normalize(collisonPoint - Position),
        };
    }
}
