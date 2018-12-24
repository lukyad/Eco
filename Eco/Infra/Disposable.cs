using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Eco
{
    public class Disposable : IDisposable
    {
        Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}
