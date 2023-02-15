namespace MiMFa.RP.CSharp
{
    public interface IARPSupported
    {
        ARPBase GetPointerJS();
        ARPBase GetPointerJS(string pointer, ARPMode pointerType);
        ARPBase GetPointerJS(long x, long y);
        ARPBase GetPointerJS(string query);
    }
}
