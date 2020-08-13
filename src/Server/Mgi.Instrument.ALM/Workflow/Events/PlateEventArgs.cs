using System;

namespace Mgi.Instrument.ALM.Workflow.Events
{
    public class PlateEventArgs : EventArgs
    {
        public string PlateName { get; }
        public PlateEventArgs(string name)
        {
            PlateName = name;
        }
    }
}
