using System;
using Unmanaged;

namespace UI.Components
{
    public struct IsUIObjectRequest
    {
        public ASCIIText256 address;
        public double timeout;
        public double duration;
        public Status status;

        [Obsolete("Default constructor not supported", true)]
        public IsUIObjectRequest()
        {
            throw new NotSupportedException();
        }

        public IsUIObjectRequest(ASCIIText256 address, double timeout)
        {
            this.address = address;
            this.timeout = timeout;
            duration = 0;
            status = Status.Submitted;
        }

        public enum Status : byte
        {
            Submitted,
            Loading,
            Loaded,
            NotFound
        }
    }
}