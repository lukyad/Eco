using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NHamcrest.Core;
using Eco;
using Eco.SettingsVisitors;

namespace Tests.SettingsVisitors
{
    public class ConfigurationVariableExpanderTest : SettingsVisitorTestBase
    {
        const string Var1 = "Var1";
        const string Var2 = "Var2";
        static string S1 = $"abc${{{Var1}}}";
        static string S2 = $"${{{Var2}}}456";
        static string S3 = "${Var3}"; // doesn't exist
        static readonly Dictionary<string, Func<string>> _vars = new Dictionary<string, Func<string>>
        {
            { Var1, () => "def" },
            { Var2, () => "123" },
        };

        class settings
        {
            public string value1;
            [Sealed]
            public string value2;
            public string[] array;
        }

        [Fact]
        public static void ExpandStringField()
        {
            var settings = new settings { value1 = S1 + S2 + S3 };
            Visit(new ConfigurationVariableExpander(_vars), s => s.value1, settings);
            Assert.That(settings.value1, Is.EqualTo("abcdef123456" + S3));
        }

        

        [Fact]
        public static void ExpandStringArrayField()
        {
            var settings = new settings { array = new[] { S1, S2, S3 } };
            Visit(new ConfigurationVariableExpander(_vars), s => s.array, settings);
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
            var fieldVisitor = new ConfigurationVariableExpander(_vars);
            Visit(fieldVisitor, s => s.value1, settings);
            Visit(fieldVisitor, s => s.value2, settings);
            Assert.That(settings.value1, Is.EqualTo("abcdef"));
            Assert.That(settings.value2, Is.EqualTo($"abc${{Var1}}"));
        }
    }
}
