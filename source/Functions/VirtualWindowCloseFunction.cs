namespace InteractionKit.Functions
{
    public unsafe readonly struct VirtualWindowCloseFunction
    {
#if NET
        private readonly delegate* unmanaged<VirtualWindow, void> function;

        public VirtualWindowCloseFunction(delegate* unmanaged<VirtualWindow, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<VirtualWindow, void> function;

        public VirtualWindowCloseFunction(delegate*<VirtualWindow, void> function)
        {
            this.function = function;
        }
#endif

        public readonly void Invoke(VirtualWindow window)
        {
            function(window);
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }
    }
}