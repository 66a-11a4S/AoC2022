using System;
using System.Linq.Expressions;

namespace Library
{
    public static class ExpressionTree
    {
        public static Func<T1, T2, TOutput> Add<T1, T2, TOutput>()
        {
            var arg1 = Expression.Parameter(typeof(T1));
            var arg2 = Expression.Parameter(typeof(T2));
            var exp = Expression.Lambda<Func<T1, T2, TOutput>>(
                Expression.Add(arg1, arg2),
                arg1,
                arg2
            );

            return exp.Compile();
        }

        public static Func<T1, T2, TOutput> Sub<T1, T2, TOutput>()
        {
            var arg1 = Expression.Parameter(typeof(T1));
            var arg2 = Expression.Parameter(typeof(T2));
            var exp = Expression.Lambda<Func<T1, T2, TOutput>>(
                Expression.Subtract(arg1, arg2),
                arg1,
                arg2
            );

            return exp.Compile();
        }

        public static Func<T1, T2, TOutput> Mul<T1, T2, TOutput>()
        {
            var arg1 = Expression.Parameter(typeof(T1));
            var arg2 = Expression.Parameter(typeof(T2));
            var exp = Expression.Lambda<Func<T1, T2, TOutput>>(
                Expression.Multiply(arg1, arg2),
                arg1,
                arg2
            );

            return exp.Compile();
        }

        public static Func<T1, T2, TOutput> Div<T1, T2, TOutput>()
        {
            var arg1 = Expression.Parameter(typeof(T1));
            var arg2 = Expression.Parameter(typeof(T2));
            var exp = Expression.Lambda<Func<T1, T2, TOutput>>(
                Expression.Divide(arg1, arg2),
                arg1,
                arg2
            );

            return exp.Compile();
        }

        public static Func<T1, T2, TOutput> Equal<T1, T2, TOutput>()
        {
            var arg1 = Expression.Parameter(typeof(T1));
            var arg2 = Expression.Parameter(typeof(T2));
            var exp = Expression.Lambda<Func<T1, T2, TOutput>>(
                Expression.Equal(arg1, arg2),
                arg1,
                arg2
            );

            return exp.Compile();
        }
    }
}
