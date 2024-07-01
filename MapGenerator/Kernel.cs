using ILGPU;
using ILGPU.Runtime;

namespace MapGenerator
{
    public struct KernelInput
    {
        public ArrayView2D<float, Stride2D.DenseY> Kernel;
        public ArrayView1D<int, Stride1D.Dense> KernelOffset;
        public ArrayView2D<float, Stride2D.DenseY> Input;
    }
}
