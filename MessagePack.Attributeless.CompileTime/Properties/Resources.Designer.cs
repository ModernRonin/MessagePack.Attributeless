﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MessagePack.Attributeless.CompileTime.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MessagePack.Attributeless.CompileTime.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public class {{IdentifierTypeName}}Formatter: IMessagePackFormatter&lt;{{FullTypeName}}&gt;
        ///{
        ///		
        ///	public {{FullTypeName}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        ///	{
        ///		if (reader.TryReadNil()) return default;
        ///
        ///		var key = reader.ReadInt32();
        ///		switch (key)
        ///		{			
        ///			{% for subType in SubTypes -%}
        ///			case {{subType.Key}}: return options.Resolver.GetFormatterWithVerify&lt;{{subType.Type}}&gt;().Deserialize(ref reader, options);
        ///			{% endfor -%}				
        ///		}
        ///		throw new Mess [rest of string was truncated]&quot;;.
        /// </summary>
        public static string BaseTypeFormatter_template {
            get {
                return ResourceManager.GetString("BaseTypeFormatter.template", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to namespace {{Namespace}}
        ///{
        ///	using MessagePack;
        ///	using MessagePack.Formatters;
        ///	using MessagePack.Resolvers;
        ///
        ///	{{Code}}
        ///}.
        /// </summary>
        public static string Common_template {
            get {
                return ResourceManager.GetString("Common.template", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public class {{IdentifierTypeName}}Formatter: IMessagePackFormatter{{FullTypeName}}
        ///{
        ///	public {{FullTypeName}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        ///	{
        ///		if (reader.TryReadNil()) return default;
        ///
        ///		return new {{FullTypeName}}
        ///		{
        ///			{% for property in Properties -%} 
        ///			{{property.Name}} = options.Resolver.GetFormatterWithVerify&lt;{{property.Type}}&gt;().Deserialize(ref reader, options),
        ///			{% endfor -%}
        ///		};
        ///	}
        ///	public void Serialize(ref MessagePackWriter w [rest of string was truncated]&quot;;.
        /// </summary>
        public static string ConcreteTypeFormatter_template {
            get {
                return ResourceManager.GetString("ConcreteTypeFormatter.template", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public class {{IdentifierTypeName}}Formatter: IMessagePackFormatter&lt;{{FullTypeName}}&gt;
        ///{
        ///	public {{FullTypeName}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        ///	{
        ///		return ({{FullTypeName}}) reader.{{ReaderMethod}};
        ///	}
        ///	public void Serialize(ref MessagePackWriter writer, {{FullTypeName}} value, MessagePackSerializerOptions options) 
        ///	{ 
        ///		writer.{{WriterMethod}};
        ///	}
        ///}
        ///.
        /// </summary>
        public static string EnumFormatter_template {
            get {
                return ResourceManager.GetString("EnumFormatter.template", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to public static class MessagePackSerializerOptionsExtensions
        ///{
        ///	public static MessagePackSerializerOptions Add(this MessagePackSerializerOptions self)
        ///		=&gt; self.WithResolver(CompositeResolver.Create(new IMessagePackFormatter[]
        ///		{
        ///			{% for formatter in Formatters -%}
        ///			new {{formatter}}(),
        ///			{% endfor -%}
        ///		}, new[] { self.Resolver }));
        ///}
        ///.
        /// </summary>
        public static string Extensions_template {
            get {
                return ResourceManager.GetString("Extensions.template", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to namespace {{Namespace}}
        ///{
        ///	using MessagePack;
        ///
        ///	public partial class {{Name}}
        ///	{
        ///		public MessagePackSerializerOptions Options { get; }= MessagePackSerializerOptions.Standard.WithResolver(
        ///			CompositeResolver.Create(new IMessagePackFormatter[]
        ///			{
        ///				{% for formatter in Formatters -%}
        ///				new {{formatter}}(),
        ///				{% endfor -%}
        ///			}, new[] { self.Resolver }));
        ///	}
        ///}.
        /// </summary>
        public static string PartialClass_template {
            get {
                return ResourceManager.GetString("PartialClass.template", resourceCulture);
            }
        }
    }
}
