using System.Diagnostics;
using OptimeGBAServer.Collections.Generics;

namespace OptimeGBAServer.Collections
{
    public class WindowAverage
    {
        private readonly RingBuffer<double> _pool;

        private readonly double sampleModifier;

        public double Average { get; private set; }

        public WindowAverage(int windowSize)
        {
            Debug.Assert(windowSize > 0);

            _pool = new RingBuffer<double>(windowSize);
            sampleModifier = 1d / (double)windowSize;
            Average = 0d;
        }

        public void AddSample(double sample)
        {
            double sampleContribution = sample * sampleModifier;
            if (_pool.PushAndPopWhenFull(sampleContribution, out double popped))
            {
                Average -= popped;
            }
            Average += sampleContribution;
        }
    }
}
