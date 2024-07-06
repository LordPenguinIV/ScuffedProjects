using ILGPU;
using ILGPU.Runtime;
using System.Numerics;
using Thingimajig;

namespace MapGenerator
{
    public class GPUMethods
    {
        public static void GetInitialCollision(Index2D i, ArrayView1D<float, Stride1D.Dense> screenInfo, ArrayView2D<float, Stride2D.DenseY> rotationMatrix, ArrayView1D<Triangle, Stride1D.Dense> scene, ArrayView1D<byte, Stride1D.Dense> output)
        {
            Vector3 skyColor = new Vector3(135, 206, 235);

            float localX = i.X * (screenInfo[3] / screenInfo[1]) - (screenInfo[3] / 2);
            float localY = -i.Y * (screenInfo[2] / screenInfo[0]) + (screenInfo[2] / 2);

            Vector3 rayDirection = new Vector3(localX * rotationMatrix[0, 0] + localY * rotationMatrix[0, 1] + screenInfo[4] * rotationMatrix[0, 2],
                localX * rotationMatrix[1, 0] + localY * rotationMatrix[1, 1] + screenInfo[4] * rotationMatrix[1, 2],
                localX * rotationMatrix[2, 0] + localY * rotationMatrix[2, 1] + screenInfo[4] * rotationMatrix[2, 2]);

            Ray ray = new Ray
            {
                Origin = new Vector3(screenInfo[5], screenInfo[6], screenInfo[7]),
                Direction = rayDirection
            };

            RayCollision initialCollision = GetNextCollision(ray, scene);

            int outputIndex = (i.Y * (int)screenInfo[1] + i.X ) * 4;

            if (initialCollision.Collides == 0)
            {
                SetPixelColor(outputIndex, skyColor, output);
            }
            else if (screenInfo[8] == 0)
            {
                SetPixelColor(outputIndex, initialCollision.Material.Color, output);
            }
            else if (initialCollision.Material.EmittedLight > 0)
            {
                SetPixelColor(outputIndex, initialCollision.Material.Color * initialCollision.Material.EmittedLight, output);
            }
            else
            {
                Vector3 incomingLight = new Vector3(0);

                int colorSamples = 512;
                int bounces = 3;

                uint state = (uint)(i.X * i.Y + i.X);
                for (int sample = 0; sample < colorSamples; sample++)
                {
                    Vector3 rayColor = new Vector3(1);
                    Vector3 incomingLightSample = new Vector3(0);

                    incomingLightSample += initialCollision.Material.Color * initialCollision.Material.EmittedLight * rayColor;
                    rayColor *= initialCollision.Material.Color / 255;

                    Ray currentRay = new Ray
                    {
                        Origin = initialCollision.CollisionPoint,
                        Direction = Vector3.Normalize(initialCollision.Normal + Ray.RandomDirection(ref state))
                    };

                    for (int bounce = 0; bounce < bounces; bounce++)
                    {
                        RayCollision rayCollision = GetNextCollision(currentRay, scene);

                        if (rayCollision.Collides == 0)
                        {
                            break;
                        }
                        else
                        {
                            incomingLightSample += rayCollision.Material.Color * rayCollision.Material.EmittedLight * rayColor;
                            rayColor *= rayCollision.Material.Color / 255;

                            currentRay = new Ray
                            {
                                Origin = rayCollision.CollisionPoint,
                                Direction = Vector3.Normalize(rayCollision.Normal + Ray.RandomDirection(ref state))
                            };
                        }
                    }

                    incomingLight += incomingLightSample;
                }

                SetPixelColor(outputIndex, incomingLight / colorSamples, output);
            }
        }

        public static RayCollision GetNextCollision(Ray ray, ArrayView1D<Triangle, Stride1D.Dense> scene)
        {
            RayCollision rayCollision = new RayCollision
            {
                Distance = float.MaxValue,
                Collides = 0,
            };

            for (int j = 0; j < scene.IntLength; j++)
            {
                RayCollision collision = scene[j].GetCollision(ray);

                if (collision.Collides == 1 && collision.Distance < rayCollision.Distance)
                {
                    rayCollision = collision;
                }
            }

            return rayCollision;
        }

        public static void SetPixelColor(int index, Vector3 color, ArrayView1D<byte, Stride1D.Dense> output)
        {
            output[index    ] = (byte)color.Z; // blue
            output[index + 1] = (byte)color.Y; // green
            output[index + 2] = (byte)color.X; // red
            output[index + 3] = 0;             // ignore
        }

        /*public static void GetViewingPlane(Index3D i, ArrayView1D<float, Stride1D.Dense> screenInfo, ArrayView2D<RayCollision, Stride2D.DenseY> rayCollisions, ArrayView1D<Triangle, Stride1D.Dense> scene, ArrayView1D<byte, Stride1D.Dense> output)
        {
            RayCollision initialCollision = rayCollisions[i.XY];

            if (initialCollision.Collides == 0)
            {
                return;
            }

            int outputIndex = (i.Y * (int)screenInfo[1] + i.X) * 4;

            int colorSamples = 256;
            int bounces = 2;

            uint state = (uint)(i.X * i.Y * i.Z + i.Z);

            Ray currentRay = new Ray
            {
                Origin = initialCollision.CollisionPoint,
                Direction = Ray.RandomDirection(initialCollision.Normal, ref state)
            };

            Vector3 colorSample = initialCollision.Material.Color * Vector3.Dot(initialCollision.Normal, currentRay.Direction) * 2;
            float emittedLightSample = 0;

            for (int bounce = 0; bounce < bounces; bounce++)
            {
                RayCollision rayCollision = new RayCollision
                {
                    Distance = float.MaxValue,
                };

                for (int j = 0; j < scene.IntLength; j++)
                {
                    RayCollision collision = scene[j].GetCollision(currentRay);

                    if (collision.Collides == 1 && collision.Distance < rayCollision.Distance)
                    {
                        rayCollision = collision;
                    }
                }

                if (rayCollision.Collides == 0)
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

            output[outputIndex + 0] += (byte)(colorSample.Z * emittedLightSample / colorSamples); // blue
            output[outputIndex + 1] += (byte)(colorSample.Y * emittedLightSample / colorSamples); // green
            output[outputIndex + 2] += (byte)(colorSample.X * emittedLightSample / colorSamples); // red
        }

        public static void GetInitialCollisions(Index2D i, ArrayView1D<float, Stride1D.Dense> screenInfo, ArrayView2D<float, Stride2D.DenseY> rotationMatrix, ArrayView1D<Triangle, Stride1D.Dense> scene, ArrayView2D<RayCollision, Stride2D.DenseY> initialCollisions, ArrayView1D<byte, Stride1D.Dense> output)
        {
            float localX = i.X * (screenInfo[3] / screenInfo[1]) - (screenInfo[3] / 2);
            float localY = -i.Y * (screenInfo[2] / screenInfo[0]) + (screenInfo[2] / 2);

            Vector3 rayDirection = new Vector3(localX * rotationMatrix[0, 0] + localY * rotationMatrix[0, 1] + screenInfo[4] * rotationMatrix[0, 2],
                localX * rotationMatrix[1, 0] + localY * rotationMatrix[1, 1] + screenInfo[4] * rotationMatrix[1, 2],
                localX * rotationMatrix[2, 0] + localY * rotationMatrix[2, 1] + screenInfo[4] * rotationMatrix[2, 2]);

            Ray ray = new Ray
            {
                Origin = new Vector3(screenInfo[5], screenInfo[6], screenInfo[7]),
                Direction = rayDirection
            };

            RayCollision initialCollision = new RayCollision
            {
                Distance = float.MaxValue,
            };

            for (int j = 0; j < scene.IntLength; j++)
            {
                RayCollision collision = scene[j].GetCollision(ray);

                if (collision.Collides == 1 && collision.Distance < initialCollision.Distance)
                {
                    initialCollision = collision;
                }
            }

            int outputIndex = (i.Y * (int)screenInfo[1] + i.X) * 4;

            if (initialCollision.Collides == 0)
            {
                output[outputIndex + 0] = 235; // blue
                output[outputIndex + 1] = 206; // green
                output[outputIndex + 2] = 135; // red
                output[outputIndex + 3] = 0; // ignore
            }
            else if (screenInfo[8] == 0)
            {
                output[outputIndex + 0] = (byte)initialCollision.Material.Color.Z; // blue
                output[outputIndex + 1] = (byte)initialCollision.Material.Color.Y; // green
                output[outputIndex + 2] = (byte)initialCollision.Material.Color.X; // red
                output[outputIndex + 3] = 0; // ignore
            }
            else if (initialCollision.Material.EmittedLight > 0)
            {
                output[outputIndex + 0] = (byte)(initialCollision.Material.Color.Z * initialCollision.Material.EmittedLight); // blue
                output[outputIndex + 1] = (byte)(initialCollision.Material.Color.Y * initialCollision.Material.EmittedLight); // green
                output[outputIndex + 2] = (byte)(initialCollision.Material.Color.X * initialCollision.Material.EmittedLight); // red
                output[outputIndex + 3] = 0; // ignore
            }
            else
            {
                output[outputIndex + 0] = 0; // blue
                output[outputIndex + 1] = 0; // green
                output[outputIndex + 2] = 0; // red
                output[outputIndex + 3] = 0; // ignore
                initialCollisions[i] = initialCollision;

                return;
            }

            // If it should be further calculated, it will have returned
            // Otherwise make comparison easy in future
            initialCollisions[i] = new RayCollision
            {
                Collides = 0
            };
        }*/
    }
}
