using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Eco;

namespace Tests.SettingsVisitors
{
    public abstract class SettingsVisitorTestBase
    {
        public static void Visit<TObject, TField>(IRawSettingsVisitor visitor, Expression<Func<TObject, TField>> fieldGetter, TObject settings)
        {
            visitor.Visit(null, Reflect<TObject>.Field(fieldGetter), settings);
        }

        //public static void Visit<TObject, TField>(IRefinedSettingsVisitor visitor, Expression<Func<TObject, TField>> fieldGetter, object settings)
        //{
        //    visitor.Visit(null, Reflect<TObject>.Field(fieldGetter), settings);
        //}
    }
}
