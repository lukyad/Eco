using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NHamcrest.Core;
using Eco;
using Eco.FieldVisitors;

namespace Tests.SettingsVisitors
{
    public class ConfigurationVariableExpanderTest
    {
        const string Var1 = "Var1";
        const string Var2 = "Var2";
        static string S1 = $"abc${{{Var1}}}";
        static string S2 = $"${{{Var2}}}456";
        static string S3 = "${Var3}"; // doesn't exist
        static readonly Dictionary<string, string> _vars = new Dictionary<string, string>
        {
            { Var1, "def" },
            { Var2, "123" },
        };

        class settings
        {
            public string value1;
        }

        class settings { public string[] array; }

        class settings
        {
            public string value1;
            [Sealed]
            public string value2;
        }

        [Fact]
        public static void ExpandStringField()
        {
            var settings = new settings { value1 = S1 + S2 + S3 };
            var fieldVisitor = new ConfigurationVariableExpander(_vars);
            var field = Reflect<settings>.Field(s => s.value1);
            fieldVisitor.Visit(null, field, settings);
            Assert.That(settings.value1, Is.EqualTo("abcdef123456" + S3));
        }

        

        [Fact]
        public static void ExpandStringArrayField()
        {
            var settings = new settings { array = new[] { S1, S2, S3 } };
            var fieldVisitor = new ConfigurationVariableExpander(_vars);
            var field = Reflect<settings>.Field(s => s.array);
            fieldVisitor.Visit(null, field, settings);
            Assert.That(settings.array[0], Is.EqualTo("abcdef"));
            Assert.That(settings.array[1], Is.EqualTo("123456"));
            Assert.That(settings.array[2], Is.EqualTo(S3));
        }


        [Fact]
        public static void SkipSealedFields()
        {
            var settings = new settings
            {
                value1 = S1,
                value2 = S1
            };
            Environment.SetEnvironmentVariable(Var1, "def");

            var fieldVisitor = new ConfigurationVariableExpander(_vars);
            var field = Reflect<settings>.Field(s => s.value1);
            fieldVisitor.Visit(null, field, settings);
            field = Reflect<settings>.Field(s => s.value2);
            fieldVisitor.Visit(null, field, settings);

            Assert.That(settings.value1, Is.EqualTo("abcdef"));
            Assert.That(settings.value2, Is.EqualTo($"abc${{Var1}}"));
        }
    }
}
