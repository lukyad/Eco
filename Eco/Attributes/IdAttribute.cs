using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eco
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IdAttribute : Attribute
    {
        public static void ValidateContext(MemberInfo context)
        {
        }
    }
}
