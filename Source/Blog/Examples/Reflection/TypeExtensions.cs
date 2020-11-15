﻿#nullable enable
namespace Examples.Reflection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Examples.Common;

    public static partial class TypeExtensions
    {
#region Methods

        internal static FieldInfo? GetBaseField(this Type type, string name)
        {
            Type? @base = type.BaseType;
            return @base is null ? null : @base.GetTypeField(name) ?? @base.GetBaseField(name);
        }

        internal static PropertyInfo? GetBaseIndex(this Type type, params object[] args)
        {
            Type? @base = type.BaseType;
            return @base is null ? null : @base.GetTypeIndex(args) ?? @base.GetBaseIndex(args);
        }

        internal static MethodInfo? GetBaseMethod(this Type type, string name, params object?[] args)
        {
            Type? @base = type.BaseType;
            return @base is null ? null : @base.GetTypeMethod(name, args) ?? @base.GetBaseMethod(name, args);
        }

        internal static PropertyInfo? GetBaseProperty(this Type type, string name)
        {
            Type? @base = type.BaseType;
            return @base is null ? null : @base.GetTypeProperty(name) ?? @base.GetBaseProperty(name);
        }

        internal static MethodInfo? GetInterfaceMethod(this Type type, string name, params object?[] args)
        {
            type.NotNull(nameof(type));

            return type.GetInterfaces()
                .Select(type.GetInterfaceMap)
                .SelectMany(mapping => mapping.TargetMethods)
                .FirstOrDefault(
                    method =>
                    method.Name.Split('.').Last().Equals(name, StringComparison.Ordinal) &&
                    method.GetParameters().Count() == args.Length &&
                    method.GetParameters().Select((parameter, index) =>
                        parameter.ParameterType.IsInstanceOfType(args[index])).Aggregate(true, (a, b) => a && b));
        }

        internal static FieldInfo? GetTypeField(this Type type, string name)
        {
            type.NotNull(nameof(type));

            return type
                .GetRuntimeFields()
                .FirstOrDefault(field => field.Name.Equals(name, StringComparison.Ordinal));
        }

        internal static PropertyInfo? GetTypeIndex(this Type type, params object[] args)
        {
            type.NotNull(nameof(type));

            return type
                .GetRuntimeProperties()
                .FirstOrDefault(
                    property =>
                    property.GetIndexParameters().Any() &&
                    property.GetIndexParameters().Select(
                        (parameter, index) => parameter.ParameterType == args[index].GetType()).Aggregate(
                            true, (a, b) => a && b));
        }

        internal static MethodInfo? GetTypeMethod(this Type type, string name, params object?[] args)
        {
            type.NotNull(nameof(type));

            return type
                .GetRuntimeMethods()
                .FirstOrDefault(method =>
                    string.Equals(method.Name, name, StringComparison.Ordinal)
                    && method.GetParameters().Length == args.Length
                    && method
                        .GetParameters()
                        .Zip(args, (parameter, arg) => arg is null || parameter.ParameterType == args.GetType())
                        .All(match => true));
        }

        internal static PropertyInfo? GetTypeProperty(this Type type, string name)
        {
            type.NotNull(nameof(type));

            return type
                .GetRuntimeProperties()
                .FirstOrDefault(property => property.Name.Equals(name, StringComparison.Ordinal));
        }

#endregion
    }

    public static partial class TypeExtensions
    {
        public static bool IsAssignableTo(this Type from, Type to)
        {
            to.NotNull(nameof(to));
            from.NotNull(nameof(from));

            if (to.IsAssignableFrom(from))
            {
                return true;
            }

            if (!to.IsGenericTypeDefinition)
            {
                return false;
            }

            if (from.IsGenericType && from.GetGenericTypeDefinition() == to)
            {
                return true; // Collection<int> is assignable to Collection<>.
            }

            if (to.IsInterface && from.GetInterfaces().Any(
                @interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == to))
            {
                return true; // Collection<>/Collection<int> assignable to IEnumerable<>/ICollection<>.
            }

            Type? baseOfFrom = from.BaseType;
            return baseOfFrom is not null && IsAssignableTo(baseOfFrom, to);
        }
    }

    public static partial class TypeExtensions
    {
        [return: MaybeNull]
        public static TValue GetField<TValue>(this object @object, string name)
        {
            @object.NotNull(nameof(@object));

            object? value = (@object.GetType().GetTypeField(name) ?? throw new ArgumentOutOfRangeException(nameof(name))).GetValue(@object);
            return value is null ? default : (TValue)value;
        }

        public static void SetField(this object @object, string name, object value)
        {
            @object.NotNull(nameof(@object));

            (@object.GetType().GetTypeField(name) ?? throw new ArgumentOutOfRangeException(nameof(name))).SetValue(@object, value);
        }

        [return: MaybeNull]
        public static TValue GetProperty<TValue>(this object @object, string name)
        {
            @object.NotNull(nameof(@object));

            object? value = (@object.GetType().GetTypeProperty(name) ?? throw new ArgumentOutOfRangeException(nameof(name))).GetValue(@object);
            return value is null ? default : (TValue)value;
        }

        public static void SetProperty(this object @object, string name, object value)
        {
            @object.NotNull(nameof(@object));

            (@object.GetType().GetTypeProperty(name) ?? throw new ArgumentOutOfRangeException(nameof(name))).SetValue(@object, value);
        }
    }
}