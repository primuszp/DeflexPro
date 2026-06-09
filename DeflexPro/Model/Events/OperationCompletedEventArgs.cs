using System;

namespace DeflexPro.Model.Events
{
    public class OperationCompletedEventArgs : EventArgs
    {
        public Exception Error { get; set; }
    }
}
