using System;
using System.Collections.Generic;

namespace DeflexPro.Model.Events
{
    public class LoadCompletedEventArgs : OperationCompletedEventArgs
    {
        public Fwd? FwdMachine { get; set; }
    }
}
