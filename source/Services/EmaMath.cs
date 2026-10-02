#nullable enable
using System;
using System.Collections.Generic;

namespace PVRSAIndicator
{
    /// <summary>
    /// ta.ema: SMA seed over the first n values, then α = 2/(n+1).
    /// State is the previous-bar committed EMA so the forming bar can be Peek'd on ticks.
    /// </summary>
    public sealed class EmaMath
    {
        private readonly int _length;
        private readonly double _alpha;
        private readonly List<double> _seed = new List<double>();
        private double _committed = double.NaN;
        private bool _seeded;

        public EmaMath(int length)
        {
            _length = length < 1 ? 1 : length;
            _alpha = 2.0 / (_length + 1.0);
        }

        public void Reset()
        {
            _seed.Clear();
            _committed = double.NaN;
            _seeded = false;
        }

        /// <summary>Commit a closed-bar close into the EMA state. Returns the committed value.</summary>
        public double Push(double src)
        {
            if (!_seeded)
            {
                _seed.Add(src);
                if (_seed.Count == _length)
                {
                    double sum = 0.0;
                    for (int i = 0; i < _seed.Count; i++) sum += _seed[i];
                    _committed = sum / _length;
                    _seeded = true;
                    _seed.Clear();
                }
                else
                {
                    _committed = double.NaN;
                }
                return _committed;
            }

            _committed = _alpha * src + (1.0 - _alpha) * _committed;
            return _committed;
        }

        /// <summary>What the EMA would be with <paramref name="src"/> as the current (uncommitted) close.</summary>
        public double Peek(double src)
        {
            if (!_seeded)
            {
                if (_seed.Count + 1 < _length) return double.NaN;
                double sum = src;
                for (int i = 0; i < _seed.Count; i++) sum += _seed[i];
                return sum / _length;
            }
            return _alpha * src + (1.0 - _alpha) * _committed;
        }

        public double Current => _committed;
    }

    /// <summary>
    /// ta.stdev: sample standard deviation (divide by n−1) of the last <c>length</c> values.
    /// Buffer holds closed bars only; PeekWithCurrent includes the forming close.
    /// </summary>
    public sealed class SampleStdev
    {
        private readonly int _length;
        private readonly double[] _buf;
        private int _count;
        private int _idx;

        public SampleStdev(int length)
        {
            _length = length < 2 ? 2 : length;
            _buf = new double[_length];
        }

        public void Reset()
        {
            _count = 0;
            _idx = 0;
        }

        public double Push(double src)
        {
            if (_count < _length)
            {
                _buf[_count] = src;
                _count++;
            }
            else
            {
                _buf[_idx] = src;
                _idx = (_idx + 1) % _length;
            }
            return Value();
        }

        /// <summary>Sample stdev of the last (length−1) committed values plus <paramref name="src"/>.</summary>
        public double PeekWithCurrent(double src)
        {
            if (_count + 1 < _length) return double.NaN;
            double mean = 0.0;
            if (_count >= _length)
            {
                int start = (_idx + 1) % _length;
                for (int i = 0; i < _length - 1; i++)
                    mean += _buf[(start + i) % _length];
                mean = (mean + src) / _length;
                double ss = 0.0;
                for (int i = 0; i < _length - 1; i++)
                {
                    double d = _buf[(start + i) % _length] - mean;
                    ss += d * d;
                }
                double ds = src - mean;
                ss += ds * ds;
                return Math.Sqrt(ss / (_length - 1));
            }
            else
            {
                mean = src;
                for (int i = 0; i < _count; i++) mean += _buf[i];
                mean /= _length;
                double ss = 0.0;
                for (int i = 0; i < _count; i++)
                {
                    double d = _buf[i] - mean;
                    ss += d * d;
                }
                double ds = src - mean;
                ss += ds * ds;
                return Math.Sqrt(ss / (_length - 1));
            }
        }

        public double Value()
        {
            if (_count < _length) return double.NaN;
            int n = _length;
            double mean = 0.0;
            for (int i = 0; i < n; i++) mean += _buf[i];
            mean /= n;
            double ss = 0.0;
            for (int i = 0; i < n; i++)
            {
                double d = _buf[i] - mean;
                ss += d * d;
            }
            return Math.Sqrt(ss / (n - 1));
        }
    }
}
