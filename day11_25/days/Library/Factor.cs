using System;
using System.Linq.Expressions;

namespace Library
{
    public interface IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        T Value { get; }
    }

    public class ValueFactor<T> : IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        public T Value { get; set; }
    }

    public class AddObject<T> : IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        private Func<T, T, T> Lambda { get; }
        private IFactor<T> Arg1 { get; }
        private IFactor<T> Arg2 { get; }

        public T Value => Lambda.Invoke(Arg1.Value, Arg2.Value);

        public AddObject(IFactor<T> arg1, IFactor<T> arg2)
        {
            Lambda = ExpressionTree.Add<T, T, T>();
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    public class SubtractObject<T> : IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        private Func<T, T, T> Lambda { get; }
        private IFactor<T> Arg1 { get; }
        private IFactor<T> Arg2 { get; }

        public T Value => Lambda.Invoke(Arg1.Value, Arg2.Value);

        public SubtractObject(IFactor<T> arg1, IFactor<T> arg2)
        {
            Lambda = ExpressionTree.Sub<T, T, T>();
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    public class MultiplyObject<T> : IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        private Func<T, T, T> Lambda { get; }
        private IFactor<T> Arg1 { get; }
        private IFactor<T> Arg2 { get; }

        public T Value => Lambda.Invoke(Arg1.Value, Arg2.Value);

        public MultiplyObject(IFactor<T> arg1, IFactor<T> arg2)
        {
            Lambda = ExpressionTree.Mul<T, T, T>();
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    public class DivideObject<T> : IFactor<T> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        private Func<T, T, T> Lambda { get; }
        private IFactor<T> Arg1 { get; }
        private IFactor<T> Arg2 { get; }

        public T Value => Lambda.Invoke(Arg1.Value, Arg2.Value);

        public DivideObject(IFactor<T> arg1, IFactor<T> arg2)
        {
            Lambda = ExpressionTree.Div<T, T, T>();
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    public class EqualsObject<T> : IFactor<bool> where T : IComparable, IComparable<T>, IEquatable<T>
    {
        private Func<T, T, bool> Lambda { get; }
        private IFactor<T> Arg1 { get; }
        private IFactor<T> Arg2 { get; }

        public bool Value => Lambda.Invoke(Arg1.Value, Arg2.Value);

        public EqualsObject(IFactor<T> arg1, IFactor<T> arg2)
        {
            Lambda = ExpressionTree.Equal<T, T, bool>();
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }
}
