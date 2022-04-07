namespace Orchid.Util
{
    public class IDIssuer
    {
        private static long lastNetworkID = -1;

        public static long GetUniqueNetworkID()
        {
            return ++lastNetworkID;
        }
        
    }
}
