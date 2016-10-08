namespace Dixin.Linq.Lambda
{
    using System;

#if DEMO
    using static Dixin.Linq.Lambda.Boolean;
#endif

    // Curried from object Boolean(object @true, object @false).
    // Boolean is the alias of Func<object, Func<object, object>>.
    public delegate Func<object, object> Boolean(object @true);

    public static partial class ChurchBoolean
    {
        //  True = true => false => true
        public static readonly Boolean
            True = @true => @false => @true;

        //  False = true => false => false
        public static readonly Boolean
            False = @true => @false => @false;
    }

#if DEMO
    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Func<Boolean, Func<Boolean, Boolean>>
            // Casting return value to Boolean is safe, because b and False are both of type Boolean.
            And = a => b => (Boolean)a(b)(False);
    }

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(true => false => false)
        public static Func<Boolean, Func<Boolean, Boolean>>
            And = a => b => (Boolean)a(b)(new Boolean(@true => @false => @false));

        // Or = a => b => a(True)(b)
        public static Func<Boolean, Func<Boolean, Boolean>> 
            Or = a => b => (Boolean)a(True)(b);

        // Not = boolean => boolean(False)(True)
        public static Func<Boolean, Boolean> 
            Not = boolean => (Boolean)boolean(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Func<Boolean, Func<Boolean, Boolean>>
            Xor = a => b => (Boolean)a(b(False)(True))(b(True)(False));
    }
#endif

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Boolean And(this Boolean a, Boolean b) => (Boolean)a(b)(False);

        internal static void CallAnd()
        {
            Boolean result1 = True.And(True);

            Boolean a = True;
            Boolean b = False;
            Boolean result2 = a.And(b);
        }

        // Or = a => b => a(True)(b)
        public static Boolean Or(this Boolean a, Boolean b) => (Boolean)a(True)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean Not(this Boolean a) => (Boolean)a(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean Xor(this Boolean a, Boolean b) =>
            (Boolean)a(b(False)(True))(b(True)(False));
    }

    public static partial class ChurchBoolean<T>
    {
        // EagerIf = condition => then => @else => condition(then)(@else)
        public static readonly Func<Boolean, Func<T, Func<T, T>>>
            EagerIf = condition => then => @else =>
                (T)condition // if (condition)
                    (then)   // then {  ... }
                    (@else); // else { ... }

        // If = condition then => @else => condition(then, @else)(Id)
        public static readonly Func<Boolean, Func<Func<Unit<T>, T>, Func<Func<Unit<T>, T>, T>>>
            If = condition => then => @else =>
                ((Func<Unit<T>, T>)condition
                    (then)
                    (@else))(Functions<T>.Id);
    }

    public static partial class ChurchBoolean
    {
        internal static void CallEagerIf(Boolean condition, Boolean a, Boolean b)
        {
            Boolean result = ChurchBoolean<Boolean>.EagerIf(condition)
                (a.And(b)) // then.
                (a.Or(b)); // else.
        }

        internal static void CallLazyIf(Boolean condition, Boolean a, Boolean b)
        {
            Boolean result = ChurchBoolean<Boolean>.If(condition)
                (_ => a.And(b)) // then.
                (_ => a.Or(b)); // else.
        }
    }

#if DEMO
    public abstract partial class Boolean
    {
        public abstract Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true);

        public Func<T, T> Invoke<T>(T @true) => @false => (T)this.Invoke<T, T>(@true)(@false);
    }

    public abstract partial class Boolean
    {
        private class TrueBoolean : Boolean
        {
            public override Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true) => @false => @true;
        }

        private class FalseBoolean : Boolean
        {
            public override Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true) => @false => @false;
        }

        public static Boolean True { get; } = new TrueBoolean();

        public static Boolean False { get; } = new FalseBoolean();
    }

    public static class BooleanExtensions
    {
        // And = a => b => a(b)(False)
        public static Boolean And(this Boolean a, Boolean b) => a.Invoke(b)(False);

        // Or = a => b => a(True)(b)
        public static Boolean Or(this Boolean a, Boolean b) => a.Invoke(True)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean Not(this Boolean boolean) => boolean.Invoke(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean Xor(this Boolean a, Boolean b) => a.Invoke(b.Invoke(False)(True))(b.Invoke(True)(False));
    }

    public static partial class ChurchBoolean<T>
    {
        public static readonly Func<Boolean, Func<Func<Unit<T>, T>, Func<Func<Unit<T>, T>, T>>>
            If = condition => then => @else => condition.Invoke(then)(@else)(_ => _);
    }

    public static partial class ChurchEncoding
    {
        // System.Boolean to Boolean
        public static Boolean Church(this bool boolean) => boolean ? True : False;

        // Boolean to System.Boolean
        public static bool Unchurch(this Boolean boolean) => boolean.Invoke(true)(false);
    }
#endif
    }