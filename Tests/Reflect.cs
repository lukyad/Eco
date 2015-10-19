using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

namespace Tests
{
    static class Reflect<TObject>
    {
        public static FieldInfo Field<TField>(Expression<Func<TObject, TField>> fieldGetter)
        {
            return (fieldGetter.Body as MemberExpression)?.Member as FieldInfo;
        }
    }
}
