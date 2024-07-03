﻿using ILGPU;
using ILGPU.Runtime;
using System.Numerics;
using Thingimajig;

namespace MapGenerator
{
    public class GPUMethods
    {
        public static void GetViewingPlane(Index2D i, ArrayView1D<float, Stride1D.Dense> screenInfo, ArrayView2D<float, Stride2D.DenseY> rotationMatrix, ArrayView1D<Sphere, Stride1D.Dense> scene, ArrayView1D<byte, Stride1D.Dense> output)
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

                if (collision.Collides && collision.Distance < initialCollision.Distance)
                {
                    initialCollision = collision;
                }
            }

            int outputIndex = (i.Y * (int)screenInfo[1] + i.X ) * 4;

            if (!initialCollision.Collides)
            {
                output[outputIndex + 0] = 235; // blue
                output[outputIndex + 1] = 206; // green
                output[outputIndex + 2] = 135; // red
                output[outputIndex + 3] = 0; // ignore
            }
            else if (screenInfo[8] == 0)
            {
                output[outputIndex + 0] = (byte)(initialCollision.Material.Color.Z); // blue
                output[outputIndex + 1] = (byte)(initialCollision.Material.Color.Y); // green
                output[outputIndex + 2] = (byte)(initialCollision.Material.Color.X); // red
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
                Vector3 color = new Vector3(0);
                float emittedLight = 0;

                int colorSamples = 256;
                int bounces = 3;

                uint state = (uint)(i.X * i.Y + i.X);
                for (int sample = 0; sample < colorSamples; sample++)
                {
                    Vector3 colorSample = new Vector3(0);
                    float emittedLightSample = 0;

                    Ray currentRay = ray;
                    for (int bounce = 0; bounce < bounces; bounce++)
                    {
                        RayCollision rayCollision = new RayCollision
                        {
                            Distance = float.MaxValue,
                        };

                        for (int j = 0; j < scene.IntLength; j++)
                        {
                            RayCollision collision = scene[j].GetCollision(currentRay);

                            if (collision.Collides && collision.Distance < rayCollision.Distance)
                            {
                                rayCollision = collision;
                            }
                        }

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
                }

                output[outputIndex + 0] = (byte)(color.Z * (emittedLight / colorSamples)); // blue
                output[outputIndex + 1] = (byte)(color.Y * (emittedLight / colorSamples)); // green
                output[outputIndex + 2] = (byte)(color.X * (emittedLight / colorSamples)); // red
                output[outputIndex + 3] = 0; // ignore
            }
        }
    }
}
