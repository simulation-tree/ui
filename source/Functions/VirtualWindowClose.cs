namespace InteractionKit.Functions
{
    public unsafe readonly struct VirtualWindowClose
    {
#if NET
        private readonly delegate* unmanaged<VirtualWindow, void> function;

        public VirtualWindowClose(delegate* unmanaged<VirtualWindow, void> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<VirtualWindow, void> function;

        public VirtualWindowClose(delegate*<VirtualWindow, void> function)
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