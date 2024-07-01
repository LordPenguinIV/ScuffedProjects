using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

namespace MapGenerator
{
    public class GPUMethods
    {
        public static void RandomNumbersKernel(Index2D i, ArrayView<uint> r, ArrayView2D<float, Stride2D.DenseY> output)
        {
            r[0] ^= r[0] << 13;
            r[0] ^= r[0] >> 17;
            r[0] ^= r[0] << 5;

            output[i] = r[0] / 4294967295f;
        }

        public static void DisplayConvertKernel(Index2D i, ArrayView2D<float, Stride2D.DenseY> input, ArrayView2D<int, Stride2D.DenseY> output)
        {
            output[i] = (int)(input[i] * 255);
        }

        public static void ConvolutionKernel(Index2D i, KernelInput k, ArrayView2D<float, Stride2D.DenseY> output)
        {
            int kernelOffset = k.KernelOffset[0];

            float t = 0f;

            for(int x = -kernelOffset; x <= kernelOffset; x++)
            {
                for (int y = -kernelOffset; y <= kernelOffset; y++)
                {
                    Index2D index = new Index2D(i.X + x, i.Y + y);

                    if (index.InBounds(k.Input.IntExtent))
                    {
                        t += k.Kernel[x + kernelOffset, y + kernelOffset] * k.Input[index];
                    }
                }
            }

            output[i] = t;
        }

        public static void ErosionKernel(Index2D i, KernelInput k, ArrayView2D<float, Stride2D.DenseY> output)
        {
            int kernelOffset = k.KernelOffset[0];

            float min = 1f;

            for (int x = -kernelOffset; x <= kernelOffset; x++)
            {
                for (int y = -kernelOffset; y <= kernelOffset; y++)
                {
                    if(k.Kernel[x + kernelOffset, y + kernelOffset] != 0)
                    {
                        Index2D index = new Index2D(i.X + x, i.Y + y);

                        if (index.InBounds(k.Input.IntExtent))
                        {
                            if (k.Input[index] < min)
                            {
                                min = k.Input[index];
                            }
                        }
                    }
                }
            }

            output[i] = min;
        }

        public static void DilationKernel(Index2D i, KernelInput k, ArrayView2D<float, Stride2D.DenseY> output)
        {
            int kernelOffset = k.KernelOffset[0];

            float max = 0f;

            for (int x = -kernelOffset; x <= kernelOffset; x++)
            {
                for (int y = -kernelOffset; y <= kernelOffset; y++)
                {
                    if (k.Kernel[x + kernelOffset, y + kernelOffset] != 0)
                    {
                        Index2D index = new Index2D(i.X + x, i.Y + y);

                        if (index.InBounds(k.Input.IntExtent))
                        {
                            if (k.Input[index] > max)
                            {
                                max = k.Input[index];
                            }
                        }
                    }
                }
            }

            output[i] = max;
        }
    }
}
