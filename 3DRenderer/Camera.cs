using ILGPU;
using ILGPU.Runtime;
using MapGenerator;
using System.Numerics;

public class Camera
{
    public Camera(int resolutionHeight, bool useGPU, List<Sphere> scene)
    {
        ResolutionHeight = resolutionHeight;
        ResolutionWidth = (int)(ResolutionHeight * 16f / 9f);
        PlaneHeight = (float)(_distanceToPlane * Math.Tan(FOV * 0.5f * Math.PI / 180) * 2);
        PlaneWidth = (int)(PlaneHeight * (16f / 9f));
        Scene = scene;
        UseGPU = useGPU;

        Context = Context.Create(builder => builder.Default().EnableAlgorithms().Math(MathMode.Fast));
        Accelerator = Context.GetPreferredDevice(preferCPU: !useGPU).CreateAccelerator(Context);
        ViewingPlaneKernel = Accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView1D<float, Stride1D.Dense>, ArrayView2D<float, Stride2D.DenseY>, ArrayView1D<Sphere, Stride1D.Dense>, ArrayView1D<byte, Stride1D.Dense>>(GPUMethods.GetViewingPlane);
        SceneBuffer = Accelerator.Allocate1D(scene.ToArray());
        OutputBuffer = Accelerator.Allocate1D<byte>(ResolutionWidth * ResolutionHeight * 4);

        RecalculationRotationMatrix();
    }

    public int CurrentRotationAngleZ = 180;
    public int CurrentRotationAngleY = 180;
    public int CurrentRotationAngleX = 0;
    public float[,] RotationMatrix;
    public Vector3 Position;

    public List<Sphere> Scene;

    public bool DoRayTracing = false;
    public bool UseGPU = false;

    public Context Context;
    public Accelerator Accelerator;
    public Action<Index2D, ArrayView1D<float, Stride1D.Dense>, ArrayView2D<float, Stride2D.DenseY>, ArrayView1D<Sphere, Stride1D.Dense>, ArrayView1D<byte, Stride1D.Dense>> ViewingPlaneKernel;
    public MemoryBuffer1D<Sphere, Stride1D.Dense> SceneBuffer;
    public MemoryBuffer1D<byte, Stride1D.Dense> OutputBuffer;

    private float _distanceToPlane = 10;

    public float PlaneHeight;
    public float PlaneWidth;

    public int ResolutionHeight;
    public int ResolutionWidth;

    public int FOV = 65;

    public void RecalculationRotationMatrix()
    {
        float degToRadScale = MathF.PI / 180;
        float alpha = CurrentRotationAngleZ * degToRadScale;
        float beta = CurrentRotationAngleX * degToRadScale; //supposed to by y
        float gamma = CurrentRotationAngleY * degToRadScale; // supposed to by x

        var r = new float[3, 3];

        r[0, 0] = MathF.Cos(alpha) * MathF.Cos(beta);
        r[0, 1] = MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) - MathF.Sin(alpha) * MathF.Cos(gamma);
        r[0, 2] = MathF.Cos(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) + MathF.Sin(alpha) * MathF.Sin(gamma);
        r[1, 0] = MathF.Sin(alpha) * MathF.Cos(beta);
        r[1, 1] = MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Sin(gamma) + MathF.Cos(alpha) * MathF.Cos(gamma);
        r[1, 2] = MathF.Sin(alpha) * MathF.Sin(beta) * MathF.Cos(gamma) - MathF.Cos(alpha) * MathF.Sin(gamma);
        r[2, 0] = -MathF.Sin(beta);
        r[2, 1] = MathF.Cos(alpha) * MathF.Sin(gamma);
        r[2, 2] = MathF.Cos(beta) * MathF.Cos(gamma);

        RotationMatrix = r;
    }

    public byte[] GetViewingPlane()
    {
        MemoryBuffer1D<float, Stride1D.Dense> screenInfoBuffer = Accelerator.Allocate1D(new float[]
        {
            ResolutionHeight,
            ResolutionWidth,
            PlaneHeight,
            PlaneWidth,
            _distanceToPlane,
            Position.X,
            Position.Y,
            Position.Z,
            DoRayTracing ? 1f : 0f,
        });
        MemoryBuffer2D<float, Stride2D.DenseY> rotationMatrixBuffer = Accelerator.Allocate2DDenseY(RotationMatrix);

        ViewingPlaneKernel(new Index2D(ResolutionWidth, ResolutionHeight),
            screenInfoBuffer, 
            rotationMatrixBuffer, 
            SceneBuffer, 
            OutputBuffer);

        byte[] output = OutputBuffer.GetAsArray1D();
        Accelerator.Synchronize();

        rotationMatrixBuffer.Dispose();
        screenInfoBuffer.Dispose();

        return output;
    }
}
