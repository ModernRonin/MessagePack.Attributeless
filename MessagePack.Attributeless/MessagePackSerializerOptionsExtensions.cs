namespace MessagePack.Attributeless
{
    public static class MessagePackSerializerOptionsExtensions
    {
        /// <summary>
        ///     Fluent entry point to use attributeless. By default, if you register sub-types, keys for their properties are
        ///     generated implicitly. If this is not what you want, pass <code>false</code> for
        ///     <paramref name="doImplicitlyAutokeySubtypes" />.
        ///     <para>
        ///         It is <b>strongly</b> recommended you perform any configuration not coming from Attributeless <b>before</b>
        ///         adding
        ///         Attributeless. For example, if you want compression, then add compression first and afterwards call
        ///         <see cref="Configure" /> on your options instance.
        ///     </para>
        /// </summary>
        public static MessagePackSerializerOptionsBuilder Configure(this MessagePackSerializerOptions self,
            bool doImplicitlyAutokeySubtypes = true) =>
            new MessagePackSerializerOptionsBuilder(self, doImplicitlyAutokeySubtypes);
    }
}