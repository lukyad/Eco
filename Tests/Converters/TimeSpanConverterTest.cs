using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NHamcrest;
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
            Assert.That(TimeSpanConverter.ParseTimeSpan("1Ms"), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1s"), Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1S"), Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1m"), Is.EqualTo(TimeSpan.FromMinutes(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1M"), Is.EqualTo(TimeSpan.FromMinutes(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1h"), Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1H"), Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1d"), Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1D"), Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1w"), Is.EqualTo(TimeSpan.FromDays(1 * 7)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1W"), Is.EqualTo(TimeSpan.FromDays(1 * 7)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1y"), Is.EqualTo(TimeSpan.FromDays(1 * 365)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1.2ms"), Is.EqualTo(TimeSpan.FromMilliseconds(1.2)));
            Assert.That(TimeSpanConverter.ParseTimeSpan("1.2y"), Is.EqualTo(TimeSpan.FromDays(1.2 * 365)));
        }

        [Fact]
        public static void ParsingFails()
        {
            Assert.That("1sec".TryParseTimeSpan(out TimeSpan t1), Is.False());
            Assert.That("Ams".TryParseTimeSpan(out TimeSpan t3), Is.False());
        }

        [Fact]
        public static void ParsingThrows()
        {
            Assert.That(() => TimeSpanConverter.ParseTimeSpan("1us"), Throws.An<ConfigurationException>());
        }
    }
}
