namespace Connection
{
    public static class HandshakeMessage
    {
        public static byte[] clientMessage = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        public static byte[] serverMessage = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };
    }
}
