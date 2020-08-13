using System;
using System.Collections.Generic;

namespace Mgi.Robot.Cantroller.Can
{
    public class SimulatedCanContorller : ICanController
    {
        public CanParameter Parameter { get; private set; }

        public SimulatedCanContorller(CanParameter parameter)
        {
            Parameter = parameter;            
        }

        public void Binding(uint deviceType, uint deviceIndex, uint canIndex, TimeSpan ioTimeout, uint frameId)
        {
            ////NotImplementedException();
        }

        public void Close()
        {
            ////throw new NotImplementedException();
        }

        public void Initialize()
        {
            ////throw new NotImplementedException();
        }

        public void Open()
        {
            ////throw new NotImplementedException();
        }

        public VciCanFrame Read() => new VciCanFrame();
        //{
        //    ////throw new NotImplementedException();
            
        //}

        public byte[] ReadContents()
        {
            ////throw new NotImplementedException();
            return new[] { (byte)0 };
        }

        public void Write(IEnumerable<byte[]> datas)
        {
            ////throw new NotImplementedException();
        }

        public ICanController Write(byte[] data, byte length)
        {
            ////throw new NotImplementedException();
            return this;
        }
    }
}
