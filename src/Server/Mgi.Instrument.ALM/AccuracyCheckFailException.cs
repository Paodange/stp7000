using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime;
using Mgi.Instrument.ALM.Device;
using Mgi.Robot.Cantroller;

namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 电机丢步异常
    /// </summary>
    [Serializable]
    public class AccuracyCheckFailException : Exception
    {
        public ISp200Axis Axis { get; }
        public int Actual { get; }
        public int Encoder { get; }

        public AccuracyCheckFailException(string message, ISp200Axis axis, int actual, int encoder)
            : base(message)
        {
            Axis = axis;
            Actual = actual;
            Encoder = encoder;
        }
    }

    public class AspirateAccuracyCheckFailException : Exception
    {
        public PipetteChannels Channel { get; }
        public List<AccuracyCheckFailException> AccuracyExceptions { get; }

        public AspirateAccuracyCheckFailException(string message, PipetteChannels channel, params AccuracyCheckFailException[] accuracyExceptions)
            : base(message)
        {
            Channel = channel;
            AccuracyExceptions = new List<AccuracyCheckFailException>();
            if (accuracyExceptions != null && accuracyExceptions.Length > 0)
            {
                AccuracyExceptions.AddRange(accuracyExceptions);
            }
        }
    }
}
