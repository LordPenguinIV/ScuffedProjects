using System.Numerics;
using Thingimajig;

public class Sphere : IObject
{
    public float Radius;

    public override RayCollision GetCollision(Ray ray)
    {
        Vector3 oc = ray.Origin - Position;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = 2 * Vector3.Dot(oc, ray.Direction);
        float c = Vector3.Dot(oc, oc) - Radius * Radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant <= 0)
        {
            return new RayCollision
            {
                Collides = false
            };
        }

        float distance = (-b - MathF.Sqrt(discriminant)) / (2 * a);

        if (distance <= 0)
        {
            return new RayCollision
            {
                Collides = false
            };
        }

        Vector3 collisonPoint = ray.Origin + ray.Direction * distance;

        return new RayCollision
        {
            Collides = true,
            Distance = distance,
            CollisionPoint = collisonPoint,
            Material = Material,
            Normal = Vector3.Normalize(collisonPoint - Position)
        };
    }
}
