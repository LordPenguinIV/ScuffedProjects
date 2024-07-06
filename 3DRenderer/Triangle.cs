using System.Numerics;
using System.Windows.Forms.VisualStyles;
using Thingimajig;

public struct Triangle
{
    public Triangle(Vector3 posA, Vector3 posB, Vector3 posC, Material material)
    {
        PositionA = posA;
        PositionB = posB;
        PositionC = posC;
        Material = material;

        _edgeAB = PositionB - PositionA;
        _edgeAC = PositionC - PositionA;
        Normal = Vector3.Cross(_edgeAB, _edgeAC);
        NormalizeNormal = Vector3.Normalize(Normal);
    }

    public Triangle(Vector3 posA, Vector3 posB, Vector3 posC, Material material, Vector3 normal)
    {
        PositionA = posA;
        PositionB = posB;
        PositionC = posC;
        Material = material;

        _edgeAB = PositionB - PositionA;
        _edgeAC = PositionC - PositionA;
        Normal = Vector3.Cross(_edgeAB, _edgeAC);
        NormalizeNormal = normal;
    }

    public Vector3 PositionA { get; }
    public Vector3 PositionB { get; }
    public Vector3 PositionC { get; }
    public Material Material { get; }
    public Vector3 Normal { get; }
    public Vector3 NormalizeNormal { get; }

    private Vector3 _edgeAB { get; }
    private Vector3 _edgeAC { get; }

    public RayCollision GetCollision(Ray ray)
    {
        Vector3 ao = ray.Origin - PositionA;
        Vector3 dao = Vector3.Cross(ao, ray.Direction);

        float determinant = -Vector3.Dot(ray.Direction, Normal);
        float invDet = 1 / determinant;

        float distance = Vector3.Dot(ao, Normal) * invDet;
        float u = Vector3.Dot(_edgeAC, dao) * invDet;
        float v = -Vector3.Dot(_edgeAB, dao) * invDet;
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
            Normal = NormalizeNormal,
            Distance = distance,
            Material = Material,
        };
    }
}
