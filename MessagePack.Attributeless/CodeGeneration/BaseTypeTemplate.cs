﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace MessagePack.Attributeless.CodeGeneration
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class BaseTypeTemplate : AFormatterTemplate
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("\r\nnamespace ");
            
            #line 7 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            
            #line default
            #line hidden
            this.Write(" \r\n{\r\n\tusing MessagePack.Formatters;\r\n\r\n\tpublic class ");
            
            #line 11 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(IdentifierTypeName));
            
            #line default
            #line hidden
            this.Write("Formatter: IMessagePackFormatter<");
            
            #line 11 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FullTypeName));
            
            #line default
            #line hidden
            this.Write(">\r\n\t{\r\n\t\t\r\n\t\tpublic ");
            
            #line 14 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FullTypeName));
            
            #line default
            #line hidden
            this.Write(" Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)\r" +
                    "\n\t\t{\r\n\t\t\tif (reader.TryReadNil()) return default;\r\n\r\n\t\t\tvar key = reader.ReadInt" +
                    "32();\r\n\r\n\t\t}\r\n\t\tpublic void Serialize(ref MessagePackWriter writer, ");
            
            #line 21 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FullTypeName));
            
            #line default
            #line hidden
            this.Write(" value, MessagePackSerializerOptions options) \r\n\t\t{ \r\n\t\t\twriter.");
            
            #line 23 "C:\Projects\Github\MessagePackExtras\MessagePack.Attributeless\CodeGeneration\BaseTypeTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(WriterMethod));
            
            #line default
            #line hidden
            this.Write(";\r\n\t\t}\r\n\t}\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
