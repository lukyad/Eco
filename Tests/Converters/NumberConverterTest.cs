using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NHamcrest.Core;
using Eco;
using Eco.Converters;

namespace Tests.Converters
{
    public class NumberConverterTest
    {
        [Fact]
        public static void ParsingSucceeds()
        {
            Assert.That(NumberConverter.ParseDecimal("1k"), Is.EqualTo(1000m));
            Assert.That(NumberConverter.ParseDecimal("1m"), Is.EqualTo(1000000m));
            Assert.That(NumberConverter.ParseDecimal("1g"), Is.EqualTo(1000000000m));
            Assert.That(NumberConverter.ParseDecimal("1.5k"), Is.EqualTo(1500m));
            Assert.That(NumberConverter.ParseDecimal("1.2345k"), Is.EqualTo(1234.5m));
            Assert.That(NumberConverter.ParseDecimal("1K"), Is.EqualTo(1000m));
            Assert.That(NumberConverter.ParseDecimal("1Kb"), Is.EqualTo(1024m));
            Assert.That(NumberConverter.ParseDecimal("1Mb"), Is.EqualTo(1048576m));
            Assert.That(NumberConverter.ParseDecimal("1Gb"), Is.EqualTo(1073741824m));
        }

        [Fact]
        public static void ParsingFails()
        {
            Assert.That(NumberConverter.Parse("1s", null), Is.Null());
        }

        [Fact]
        public static void ParsingThrows()
        {
            Assert.That(() => NumberConverter.ParseDecimal("1s"), Throws.An<ConfigurationException>());
        }
    }
}
