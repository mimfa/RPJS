namespace MiMFa.RP.CSharp.JS
{
    public interface IARPSupported
    {
        ARP GetPointerJS();
        ARP GetPointerJS(string pointer, ARPMode pointerType);
        ARP GetPointerJS(long x, long y);
        ARP GetPointerJS(string query);
    }
}
