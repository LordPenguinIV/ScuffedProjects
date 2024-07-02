using System.Numerics;
using Thingimajig;

public struct Ray
{
    public Vector3 Origin { get; set; }

    public Vector3 Direction { get; set; }

    public RayCollision GetNextCollision(List<IObject> scene)
    {
        RayCollision bestCollision = new RayCollision
        {
            Distance = float.MaxValue,
        };

        foreach (IObject obj in scene)
        {
            RayCollision collision = obj.GetCollision(this);
            if (collision.Collides && collision.Distance < (bestCollision.Distance))
            {
                bestCollision = collision;
            }
        }

        return bestCollision;
    }
    private static float RandomFloat(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;

        return state / 4294967295f;
    }
    private static float RandomNormalFloat(ref uint state)
    {
        float theta = 2 * MathF.PI * RandomFloat(ref state);
        float rho = MathF.Sqrt(-2 * MathF.Log(RandomFloat(ref state)));
        return rho * MathF.Cos(theta);
    }

    public static Vector3 RandomDirection(Vector3 normal, ref uint state)
    {
        float randX = RandomNormalFloat(ref state);
        float randY = RandomNormalFloat(ref state);
        float randZ = RandomNormalFloat(ref state);

        Vector3 randomRay = Vector3.Normalize(new Vector3(randX, randY, randZ));

        return randomRay * MathF.Sign(Vector3.Dot(normal, randomRay));
    }
}