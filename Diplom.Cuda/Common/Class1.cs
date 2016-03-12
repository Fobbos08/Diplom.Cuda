using System;
using Alea.CUDA;
using Alea.CUDA.Utilities;
using Alea.CUDA.IL;

using System.Linq;

namespace Common
{
    public class Class1
    {
        [AOTCompile]
        static void SquareKernel(deviceptr<byte> outputs, deviceptr<byte> inputs, int n)
        {
            var start = blockIdx.x * blockDim.x + threadIdx.x;
            var stride = gridDim.x * blockDim.x;
            for (var i = start; i < n; i += stride)
            {
                outputs[i] = (byte)(inputs[i] + 100 > 255 ? 255 : inputs[i] + 100);
            }
        }

        public static byte[] GPUTransfer(byte[] inputs)
        {
            var worker = Worker.Default;
            using (var dInputs = worker.Malloc(inputs))
            using (var dOutputs = worker.Malloc<byte>(inputs.Length))
            {
                const int blockSize = 256;
                var numSm = worker.Device.Attributes.MULTIPROCESSOR_COUNT;
                var gridSize = Math.Min(16 * numSm, Alea.CUDA.Utilities.Common.divup(inputs.Length, blockSize));
                var lp = new LaunchParam(gridSize, blockSize);
                worker.Launch(SquareKernel, lp, dOutputs.Ptr, dInputs.Ptr, inputs.Length);
                return dOutputs.Gather();
            }
        }

    }
}
