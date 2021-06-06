using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MessagePack.Attributeless.Implementation
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo WriteablePropertyInfo<T, TProperty>(
            this Expression<Func<T, TProperty>> self)
        {
            var result = (self.Body as MemberExpression)?.Member as PropertyInfo;
            precondition(result != default, "must be a property accessor");
            precondition(result.CanWrite, "must be a writeable property");
            return result;

            void precondition(bool condition, string message)
            {
                if (!condition) throw new ArgumentException(message, nameof(self));
            }
        }
    }
}