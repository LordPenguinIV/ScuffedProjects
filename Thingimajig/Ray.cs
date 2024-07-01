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

    private static Random _rand = new Random();

    public static Vector3 RandomDirection()
    {
        float[] r = new float[6];
        for(int i = 0; i < 6; i++)
        {
            r[i] = _rand.NextSingle();
        }
        
        float randX = MathF.Sqrt(-2 * MathF.Log(r[0])) * MathF.Cos(2 * MathF.PI * r[1]);
        float randY = MathF.Sqrt(-2 * MathF.Log(r[2])) * MathF.Cos(2 * MathF.PI * r[3]);
        float randZ = MathF.Sqrt(-2 * MathF.Log(r[4])) * MathF.Cos(2 * MathF.PI * r[5]);

        Vector3 randomRay = Vector3.Normalize(new Vector3(randX, randY, randZ));

        return randomRay;
    }
}