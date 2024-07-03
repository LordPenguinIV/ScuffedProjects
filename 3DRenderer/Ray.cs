using ILGPU.IR;
using System.Numerics;
using Thingimajig;

public struct Ray
{
    public Vector3 Origin { get; set; }

    public Vector3 Direction { get; set; }

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

        float sign;
        float value = Vector3.Dot(normal, randomRay);
        if (value < 0)
        {
            sign = - 1;
        }
        else if (value > 0)
        {
            sign = 1;
        }
        else
        {
            sign = 0;
        }

        return randomRay * sign;
    }
}