namespace MessagePack.Attributeless
{
    public static class MessagePackSerializerOptionsExtensions
    {
        public static MessagePackSerializerOptionsBuilder Configure(this MessagePackSerializerOptions self,
            bool doImplicitlyAutokeySubtypes = true) =>
            new MessagePackSerializerOptionsBuilder(self, doImplicitlyAutokeySubtypes);
    }
}