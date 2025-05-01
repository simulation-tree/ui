using System;
using Unmanaged;

namespace UI.Components
{
    public struct IsUIObjectRequest
    {
        public ASCIIText256 address;
        public TimeSpan timeout;
        public TimeSpan duration;
        public Status status;

        [Obsolete("Default constructor not supported", true)]
        public IsUIObjectRequest()
        {
            throw new NotSupportedException();
        }

        public IsUIObjectRequest(ASCIIText256 address, TimeSpan timeout)
        {
            this.address = address;
            this.timeout = timeout;
            duration = TimeSpan.Zero;
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