using System.Numerics;
using Thingimajig;

public class Camera : IObject
{
    public Camera()
    {
        RecalculationRotationMatrix();
    }

    public int CurrentRotationAngleZ = 180;
    public int CurrentRotationAngleY = 180;
    public int CurrentRotationAngleX = 0;
    public float[][] RotationMatrix;
    public bool DoRayTracing = false;

    private float _distanceToPlane = 10;

    public float PlaneHeight => (float)(_distanceToPlane * Math.Tan(FOV * 0.5f * Math.PI / 180) * 2);
    public float PlaneWidth => (int)(PlaneHeight * 16f / 9f);

    public int ResolutionHeight;
    public int ResolutionWidth => (int)(ResolutionHeight * 16f / 9f);

    public int FOV = 65;


    public Ray GetPixelRay(int x, int y)
    {
        float localX = x * (PlaneWidth / ResolutionWidth) - (PlaneWidth / 2);
        float localY = -y * (PlaneHeight / ResolutionHeight) + (PlaneHeight / 2);

        return new Ray
        {
            Origin = Position,
            Direction = GetDirectionRay(localX, localY)
        };
    }

    private Vector3 GetDirectionRay(float x, float y)
    {
        float z = _distanceToPlane;

        return new Vector3(x * RotationMatrix[0][0] + y * RotationMatrix[0][1] + z * RotationMatrix[0][2],
            x * RotationMatrix[1][0] + y * RotationMatrix[1][1] + z * RotationMatrix[1][2],
            x * RotationMatrix[2][0] + y * RotationMatrix[2][1] + z * RotationMatrix[2][2]);
    }

    public void RecalculationRotationMatrix()
    {
        float degToRadScale = MathF.PI / 180;
        float alpha = CurrentRotationAngleZ * degToRadScale;
        float beta = CurrentRotationAngleX * degToRadScale; //supposed to by y
        float gamma = CurrentRotationAngleY * degToRadScale; // supposed to by x

        var r = new float[][]
        {
            [MathF.Cos(alpha) * MathF.Cos(beta),
                MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) - MathF.Sin(alpha) * MathF.Cos(gamma),
                MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) + MathF.Sin(alpha) * MathF.Sin(gamma)],
            [MathF.Sin(alpha) * MathF.Cos(beta),
                MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) + MathF.Cos(alpha) * MathF.Cos(gamma),
                MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) - MathF.Cos(alpha) * MathF.Sin(gamma)],
            [-MathF.Sin(beta),
                MathF.Cos(alpha) * MathF.Sin(gamma),
                MathF.Cos(beta) * MathF.Cos(gamma)],
        };

        RotationMatrix = r;
    }

    public Color?[,] GetViewingPlane(List<IObject> scene)
    {
        Color?[,] plane = new Color?[ResolutionHeight, ResolutionWidth];

        Parallel.For(0, ResolutionHeight, y =>
        {
            Parallel.For(0, ResolutionWidth, x =>
            {
                Ray ray = GetPixelRay(x, y);
                Vector3? color = TraceRay(ray, scene);

                plane[y, x] = color != null ? Color.FromArgb((byte)(color.Value.X * 255), (byte)(color.Value.Y * 255), (byte)(color.Value.Z * 255)) : null;
            });
        });

        return plane;
    }

    public Vector3? TraceRay(Ray ray, List<IObject> scene, int bounces = 4)
    {
        Vector3 color = new Vector3(0);
        float emittedLight = 0;

        int colorSamples = 512;

        RayCollision initialCollision = ray.GetNextCollision(scene);

        if (initialCollision.Collides != true)
        {
            return null;
        }
        else if (initialCollision.Material.EmittedLight > 0)
        {
            return initialCollision.Material.Color * initialCollision.Material.EmittedLight;
        }

        if (!DoRayTracing)
        {
            return initialCollision.Material.Color;
        }

        Parallel.For(0, colorSamples, sample =>
        {
            Vector3 colorSample = new Vector3(0);
            float emittedLightSample = 0;
            uint state = (uint)DateTime.Now.Ticks;

            Ray currentRay = ray;
            for (int bounce = 0; bounce < bounces; bounce++)
            {
                RayCollision rayCollision = currentRay.GetNextCollision(scene);

                if (rayCollision.Collides != true)
                {
                    break;
                }
                else
                {
                    currentRay = new Ray
                    {
                        Origin = rayCollision.CollisionPoint,
                        Direction = Ray.RandomDirection(rayCollision.Normal, ref state)
                    };

                    if (rayCollision.Material.EmittedLight > 0)
                    {
                        emittedLightSample += rayCollision.Material.EmittedLight;
                        colorSample += rayCollision.Material.Color;
                    }
                    else
                    {
                        float lightStrength = Vector3.Dot(rayCollision.Normal, currentRay.Direction);
                        colorSample += rayCollision.Material.Color * lightStrength * 2;
                    }
                }
            }

            emittedLight += emittedLightSample;
            color += colorSample / colorSamples;
        });

        return color * (emittedLight / colorSamples);
    }

    public override RayCollision GetCollision(Ray ray)
    {
        throw new NotImplementedException();
    }
}
