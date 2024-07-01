using System.Numerics;
using Thingimajig;

public class CameraV2 : IObject
{
    public Vector3 ViewingAngle;
    private float _distanceToPlane = 10;

    public float PlaneHeight => (float)(_distanceToPlane * Math.Tan(FOV * 0.5f * Math.PI / 180) * 2);
    public float PlaneWidth => PlaneHeight;

    public int ResolutionHeight;
    public int ResolutionWidth => ResolutionHeight;

    public int FOV = 65;

    public Ray GetPixelRay(int x, int y)
    {
        float localX = x * (PlaneWidth / ResolutionWidth) - (PlaneWidth / 2);
        float localY = -y * (PlaneHeight / ResolutionHeight) + (PlaneHeight / 2);

        return new Ray
        {
            Origin = Position,
            Direction = new Vector3(localX, localY, _distanceToPlane) + ViewingAngle
        };
    }

    public Color?[,] GetViewingPlane(List<IObject> scene)
    {
        Color?[,] plane = new Color?[ResolutionHeight, ResolutionWidth];

        Parallel.For(0, ResolutionHeight, y =>
        {
            Parallel.For(0, ResolutionWidth, x =>
            {
                Ray ray = GetPixelRay(x, y);
                Vector3 color = TraceRay(ray, scene);

                plane[y, x] = Color.FromArgb((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
            });
        });

        return plane;
    }

    public Vector3 TraceRay(Ray ray, List<IObject> scene, int bounces = 3)
    {
        Vector3 color = new Vector3(0);
        float emittedLight = 0;

        int colorSamples = 32;

        RayCollision initialCollision = ray.GetNextCollision(scene);

        if (initialCollision.Collides != true)
        {
            return color;
        }
        else if (initialCollision.Material.EmittedLight > 0)
        {
            return initialCollision.Material.Color * initialCollision.Material.EmittedLight;
        }

        Parallel.For(0, colorSamples, sample =>
        {
            Vector3 colorSample = new Vector3(0);
            float emittedLightSample = 0;

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
                        Direction = Vector3.Normalize(rayCollision.Normal + Ray.RandomDirection())
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
            color += colorSample;
        });

        return color * emittedLight / (colorSamples * colorSamples);
    }

    public override RayCollision GetCollision(Ray ray)
    {
        throw new NotImplementedException();
    }
}
