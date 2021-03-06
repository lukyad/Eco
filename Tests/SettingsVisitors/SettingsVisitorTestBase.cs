﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Eco;

namespace Tests.SettingsVisitors
{
    public abstract class SettingsVisitorTestBase
    {
        public static void Visit<TObject, TField>(ISettingsVisitor visitor, Expression<Func<TObject, TField>> fieldGetter, TObject settings)
        {
            visitor.Visit(null, null, settings, Reflect<TObject>.Field(fieldGetter));
        }

        public static void Visit<TObject>(ISettingsVisitor visitor, TObject settings)
        {
            visitor.Visit(null, null, settings);
        }
    }
}
