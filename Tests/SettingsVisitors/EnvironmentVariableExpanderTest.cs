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
    public class EnvironmentVariableExpanderTest : SettingsVisitorTestBase
    {
        const string EnvVar1 = "ENV_VAR1";
        const string EnvVar2 = "ENV_VAR2";
        static string S1 = $"abc%{EnvVar1}%";
        static string S2 = $"%{EnvVar2}%456";
        static string S3 = "%ENV_VAR3%"; // doesn't exist

        class settings
        {
            public string value1;
            public string[] array;
            [Sealed]
            public string value2;
        }

        [Fact]
        public static void ExpandStringField()
        {
            var settings = new settings { value1 = S1 + S2 + S3 };
            Environment.SetEnvironmentVariable(EnvVar1, "def");
            Environment.SetEnvironmentVariable(EnvVar2, "123");
            Visit(new EnvironmentVariableExpander(), s => s.value1, settings);
            Assert.That((string)settings.value1, Is.EqualTo("abcdef123456" + S3));
        }

        [Fact]
        public static void ExpandStringArrayField()
        {
            var settings = new settings { array = new[] { S1, S2, S3 } };
            Environment.SetEnvironmentVariable(EnvVar1, "def");
            Environment.SetEnvironmentVariable(EnvVar2, "123");
            Visit(new EnvironmentVariableExpander(), s => s.array, settings);
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
            Environment.SetEnvironmentVariable(EnvVar1, "def");
            var fieldVisitor = new EnvironmentVariableExpander();
            Visit(fieldVisitor, s => s.value1, settings);
            Visit(fieldVisitor, s => s.value2, settings);
            Assert.That(settings.value1, Is.EqualTo("abcdef"));
            Assert.That(settings.value2, Is.EqualTo($"abc%{EnvVar1}%"));
        }

    }
}
