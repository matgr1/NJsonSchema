﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.Converters;
using Xunit;

namespace NJsonSchema.CodeGeneration.Tests.CSharp
{
    public class InheritanceTests
    {
        public class MyContainer
        {
            public EmptyClassInheritingDictionary CustomDictionary { get; set; }
        }

        /// <summary>
        /// Foobar.
        /// </summary>
        public sealed class EmptyClassInheritingDictionary : Dictionary<string, object>
        {
        }

        [Fact]
        public async Task When_empty_class_inherits_from_dictionary_then_allOf_inheritance_still_works()
        {
            //// Arrange
            var schema = await JsonSchema4.FromTypeAsync<MyContainer>();
            var data = schema.ToJson();

            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings());

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            var dschema = schema.Definitions["EmptyClassInheritingDictionary"];

            Assert.Equal(0, dschema.AllOf.Count);
            Assert.True(dschema.IsDictionary);
            Assert.Contains("Foobar.", data);
            Assert.Contains("Foobar.", code);

            Assert.DoesNotContain("class CustomDictionary :", code);
            Assert.Contains("public EmptyClassInheritingDictionary CustomDictionary", code);
            Assert.Contains("public partial class EmptyClassInheritingDictionary : System.Collections.Generic.Dictionary<string, object>", code);
        }

        [KnownType(typeof(MyException))]
        [JsonConverter(typeof(JsonInheritanceConverter), "kind")]
        public class ExceptionBase : Exception
        {
            public string Foo { get; set; }
        }

        /// <summary>
        /// Foobar.
        /// </summary>
        public class MyException : ExceptionBase
        {
            public string Bar { get; set; }
        }

        public class ExceptionContainer
        {
            public ExceptionBase Exception { get; set; }
        }

        [Fact]
        public async Task When_class_with_discriminator_has_base_class_then_csharp_is_generated_correctly()
        {
            //// Arrange
            var schema = await JsonSchema4.FromTypeAsync<ExceptionContainer>();
            var data = schema.ToJson();

            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("Foobar.", data);
            Assert.Contains("Foobar.", code);

            Assert.Contains("class ExceptionBase : Exception", code);
            Assert.Contains("class MyException : ExceptionBase", code);
        }
    }
}
