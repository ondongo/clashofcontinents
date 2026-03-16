namespace DevelopersHub.ClashOfWhatecer
{
    // Minimal no-op networking layer replacing external realtime dependency.
    public class Packet
    {
        public void Write<T>(T value)
        {
            // Intentionally blank: offline/minimal mode.
        }
    }

    public static class Sender
    {
        public static void TCP_Send(Packet packet)
        {
            // Intentionally blank: offline/minimal mode.
        }
    }
}
