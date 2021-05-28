namespace MessagePack.Contractless.Subtypes
{
    public static class MessagePackSerializerOptionsExtensions
    {
        public static MessagePackSerializerOptionsBuilder Configure(this MessagePackSerializerOptions self) =>
            new MessagePackSerializerOptionsBuilder(self);
    }
}