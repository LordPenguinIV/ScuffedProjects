using System.Numerics;
using Thingimajig;

public struct Triangle
{
    public Vector3 PositionA { get; set; }
    public Vector3 PositionB { get; set; }
    public Vector3 PositionC { get; set; }

    public Material Material;

    public RayCollision GetCollision(Ray ray)
    {
        Vector3 edgeAB = PositionB - PositionA;
        Vector3 edgeAC = PositionC - PositionA;
        Vector3 normal = Vector3.Cross(edgeAB, edgeAC);
        Vector3 ao = ray.Origin - PositionA;
        Vector3 dao = Vector3.Cross(ao, ray.Direction);

        float determinant = -Vector3.Dot(ray.Direction, normal);
        float invDet = 1 / determinant;

        float distance = Vector3.Dot(ao, normal) * invDet;
        float u = Vector3.Dot(edgeAC, dao) * invDet;
        float v = -Vector3.Dot(edgeAB, dao) * invDet;
        float w = 1 - u - v;

        if (!(determinant > 1E-6f && distance > 0 && u >= 0 && v >= 0 && w >= 0))
        {
            return new RayCollision
            {
                Collides = 0
            };
        }

        return new RayCollision
        {
            Collides = 1,
            CollisionPoint = ray.Origin + ray.Direction * distance,
            Normal = Vector3.Normalize(PositionA * w + PositionB * u + PositionC * v),
            Distance = distance,
            Material = Material,
        };
    }
}
