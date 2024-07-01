using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using System.Diagnostics;
using System.Numerics;
using static MapGenerator.GPUMethods;

namespace MapGenerator
{
    public class Map
    {
        public int X { get; set; }
        public int Y { get; set; }

        private float[,] HeightMap { get; set; }

        public Map(int x, int y)
        {
            X = x;
            Y = y;

            HeightMap = new float[x, y];
        }

        public int[,] GetDisplayMap()
        {
            Stopwatch sw = Stopwatch.StartNew();

            Context context = Context.CreateDefault();
            Accelerator accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

            var randomKernel = accelerator.LoadImplicitlyGroupedStreamKernel<Index2D, ArrayView<uint>, ArrayView2D<float, Stride2D.DenseY>>(RandomNumbersKernel, 1);
            var convolutionKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, KernelInput, ArrayView2D<float, Stride2D.DenseY>>(ConvolutionKernel);
            var erosionKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, KernelInput, ArrayView2D<float, Stride2D.DenseY>>(ErosionKernel);
            var dilationKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, KernelInput, ArrayView2D<float, Stride2D.DenseY>>(DilationKernel);
            var displayKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<int, Stride2D.DenseY>>(DisplayConvertKernel);

            var guassianBlurKernel = new float[,]
            {
                { 1/16f, 1/8f, 1/16f },
                { 1/8f, 1/4f, 1/8f },
                { 1/16f, 1/8f, 1/16f },
            };

            sw.Stop();
            Debug.WriteLine($"setup: {sw.ElapsedMilliseconds}");

            sw.Restart();
            InitHeightMap(accelerator, randomKernel);
            sw.Stop();
            Debug.WriteLine($"random: {sw.ElapsedMilliseconds}");

            /*sw.Restart();
            ConvolveHeightMap(accelerator, convolutionKernel, guassianBlurKernel);
            sw.Stop();
            Debug.WriteLine($"blur: {sw.ElapsedMilliseconds}");*/

            sw.Restart();
            var d = ConvertHeightMapForDisplay(accelerator, displayKernel);
            sw.Stop();
            Debug.WriteLine($"convert: {sw.ElapsedMilliseconds}");

            accelerator.Dispose();
            context.Dispose();

            return d;
        }

        private void InitHeightMap(Accelerator accelerator, Action<Index2D, ArrayView<uint>, ArrayView2D<float, Stride2D.DenseY>> kernel)
        {
            MemoryBuffer1D<uint, Stride1D.Dense> randomSeed = accelerator.Allocate1D(new uint[] { (uint)DateTime.Now.Ticks });
            MemoryBuffer2D<float, Stride2D.DenseY> deviceOutput = accelerator.Allocate2DDenseY<float>(new LongIndex2D(X, Y));

            kernel(deviceOutput.IntExtent, randomSeed.View, deviceOutput.View);
            accelerator.Synchronize();
            HeightMap = deviceOutput.GetAsArray2D();

            deviceOutput.Dispose();
            randomSeed.Dispose();
        }

        private void ConvolveHeightMap(Accelerator accelerator, Action<Index2D, KernelInput, ArrayView2D<float, Stride2D.DenseY>> gpuKernel, float[,] convolutionKernel)
        {
            MemoryBuffer1D<int, Stride1D.Dense> kernelOffset = accelerator.Allocate1D(new int[] { (int)Math.Sqrt(convolutionKernel.Length) / 2 });
            MemoryBuffer2D<float, Stride2D.DenseY> kernel = accelerator.Allocate2DDenseY(convolutionKernel);
            MemoryBuffer2D<float, Stride2D.DenseY> input = accelerator.Allocate2DDenseY(HeightMap);
            MemoryBuffer2D<float, Stride2D.DenseY> output = accelerator.Allocate2DDenseY<float>(new LongIndex2D(X, Y));

            KernelInput kio = new KernelInput
            {
                KernelOffset = kernelOffset.View,
                Kernel = kernel.View,
                Input = input.View,
            };

            gpuKernel(input.IntExtent, kio, output.View);
            accelerator.Synchronize();
            HeightMap = output.GetAsArray2D();

            output.Dispose();
            input.Dispose();
        }

        private int[,] ConvertHeightMapForDisplay(Accelerator accelerator, Action<Index2D, ArrayView2D<float, Stride2D.DenseY>, ArrayView2D<int, Stride2D.DenseY>> gpuKernel)
        {
            MemoryBuffer2D<float, Stride2D.DenseY> deviceInput = accelerator.Allocate2DDenseY(HeightMap);
            MemoryBuffer2D<int, Stride2D.DenseY> deviceOutput = accelerator.Allocate2DDenseY<int>(new Index2D(X, Y));

            gpuKernel(deviceOutput.IntExtent, deviceInput.View, deviceOutput.View);
            accelerator.Synchronize();
            int[,] displayMap =  deviceOutput.GetAsArray2D();

            deviceOutput.Dispose();
            deviceInput.Dispose();

            return displayMap;
        }
    }
}
