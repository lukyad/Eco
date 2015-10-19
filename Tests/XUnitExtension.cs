using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHamcrest;
using Xunit.Sdk;


namespace Tests
{
    public class Assert : Xunit.Assert
    {
        public static void That<T>(T actual, IMatcher<T> matcher)
        {
            if (matcher.Matches(actual))
                return;

            var description = new StringDescription();
            matcher.DescribeTo(description);

            var mismatchDescription = new StringDescription();
            matcher.DescribeMismatch(actual, mismatchDescription);

            throw new MatchException(description.ToString(), mismatchDescription.ToString(), null);
        }
    }

    public class MatchException : AssertActualExpectedException
    {
        public MatchException(object expected, object actual, string userMessage) : base(expected, actual, userMessage)
        {
        }
    }
}
