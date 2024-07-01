// Initialize ILGPU.
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Numerics;


Context context = Context.CreateDefault();
Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
                          .CreateAccelerator(context);

// Load the data.
MemoryBuffer1D<Vector3, Stride1D.Dense> deviceData = accelerator.Allocate1D(new Vector3[] { new Vector3(1), new Vector3(2) });
MemoryBuffer1D<Output, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<Output>(1);

// load / precompile the kernel
Action<Index1D, ArrayView<Vector3>, ArrayView<Output>> loadedKernel =
    accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector3>, ArrayView<Output>>(Kernel);

// finish compiling and tell the accelerator to start computing the kernel
loadedKernel((int)deviceOutput.Length, deviceData.View, deviceOutput.View);

// wait for the accelerator to be finished with whatever it's doing
// in this case it just waits for the kernel to finish.
accelerator.Synchronize();

// moved output data from the GPU to the CPU for output to console
Output[] hostOutput = deviceOutput.GetAsArray1D();

for (int i = 0; i < hostOutput.Length; i++)
{
    Console.Write(hostOutput[i]);
}

accelerator.Dispose();
context.Dispose();

static void Kernel(Index1D i, ArrayView<Vector3> data, ArrayView<Output> output)
{
    float emittedLightSample = 0;

    output[0].EmittedLight += emittedLightSample;
    output[0].Color += data[i];

}

public struct Ray
{
    public Vector3 Origin { get; set; }

    public Vector3 Direction { get; set; }
}

public struct Output
{
    public float EmittedLight;
    public Vector3 Color;

    public Output()
    {
        Color = new Vector3(0, 0, 0);
        EmittedLight = 0;
    }

    public override string ToString()
    {
        return $"{Color} * {EmittedLight}";
    }
}

public struct RayCollision
{
    public bool Collides { get; set; }
    public Vector3 CollisionPoint { get; set; }
    public Vector3 Normal { get; set; }
    public float Distance { get; set; }
    public Material Material { get; set; }
}

public struct Material
{
    public float Reflectivity;
    public float EmittedLight;
    public Vector3 Color;
}