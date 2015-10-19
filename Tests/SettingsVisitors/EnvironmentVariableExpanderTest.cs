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
    public class EnvironmentVariableExpanderTest
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
        }

        [Fact]
        public static void ExpandStringField()
        {
            var settings = new settings { value1 = S1 + S2 + S3 };
            Environment.SetEnvironmentVariable(EnvVar1, "def");
            Environment.SetEnvironmentVariable(EnvVar2, "123");

            var fieldVisitor = new EnvironmentVariableExpander();
            var field = Reflect<settings>.Field(s => s.value1);
            fieldVisitor.Visit(null, field, settings);

            Assert.That((string)settings.value1, Is.EqualTo("abcdef123456" + S3));
        }

        [Fact]
        public static void ExpandStringArrayField()
        {
            var settings = new settings { array = new[] { S1, S2, S3 } };
            Environment.SetEnvironmentVariable(EnvVar1, "def");
            Environment.SetEnvironmentVariable(EnvVar2, "123");

            var fieldVisitor = new EnvironmentVariableExpander();
            var field = Reflect<settings>.Field(s => s.array);
            fieldVisitor.Visit(null, field, settings);

            Assert.That(settings.array[0], Is.EqualTo("abcdef"));
            Assert.That(settings.array[1], Is.EqualTo("123456"));
            Assert.That(settings.array[2], Is.EqualTo(S3));
        }

        class settings
        {
            public string value1;
            [Sealed]
            public string value2;
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
            var field = Reflect<settings>.Field(s => s.value1);
            fieldVisitor.Visit(null, field, settings);
            field = Reflect<settings>.Field(s => s.value2);
            fieldVisitor.Visit(null, field, settings);

            Assert.That((string)settings.value1, Is.EqualTo("abcdef"));
            Assert.That(settings.value2, Is.EqualTo($"abc%{EnvVar1}%"));
        }

    }
}
