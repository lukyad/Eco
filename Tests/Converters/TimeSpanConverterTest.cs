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
    public class TimeSpanConverterTest
    {
        [Fact]
        public static void ParsingSucceeds()
        {
            Assert.That(TimeSpanConverter.ParseTimeSpan("1ms"), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1s"), Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1m"), Is.EqualTo(TimeSpan.FromMinutes(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1h"), Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1d"), Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1w"), Is.EqualTo(TimeSpan.FromDays(1 * 7)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1y"), Is.EqualTo(TimeSpan.FromDays(1 * 365)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1.2ms"), Is.EqualTo(TimeSpan.FromMilliseconds(1.2)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1.2y"), Is.EqualTo(TimeSpan.FromDays(1.2 * 365)));
        }

        [Fact]
        public static void ParsingFails()
        {
            Assert.That(NumberConverter.Parse("1sec", null), Is.Null());
            Assert.That(NumberConverter.Parse("1H", null), Is.Null());
            Assert.That(NumberConverter.Parse("Ams", null), Is.Null());
        }

        [Fact]
        public static void ParsingThrows()
        {
            Assert.That(() => NumberConverter.ParseDecimal("1us"), Throws.An<ConfigurationException>());
        }
    }
}
