using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlueGraph.Editor
{
    /// <summary>
    /// Extensions to the Type class for checking cast operations
    /// </summary>
    public static class TypeExtension
    {
        /// Caching of castability between types to avoid repeat reflection
        static readonly Dictionary<(Type, Type), bool> k_CastSupportCache = new Dictionary<(Type, Type), bool>();

        /// Caching of prettified type names
        static readonly Dictionary<Type, string> k_Names = new Dictionary<Type, string>();

        /// <summary>
        /// Test if a type can cast to another, taking in account cast operators. 
        /// Based on https://stackoverflow.com/a/22031364
        /// </summary>
        /// <returns></returns>
        public static bool IsCastableTo(this Type from, Type to, bool implicitly = false)
        {
            var key = (from, to);
            if (k_CastSupportCache.TryGetValue(key, out bool support))
            {
                return support;
            }

            support = to.IsAssignableFrom(from) || from.HasCastDefined(to, implicitly);
            k_CastSupportCache.Add(key, support);
            return support;
        }

        static bool HasCastDefined(this Type from, Type to, bool implicitly)
        {
            if ((from.IsPrimitive || from.IsEnum) && (to.IsPrimitive || to.IsEnum))
            {
                if (!implicitly)
                {
                    return from == to || (from != typeof(bool) && to != typeof(bool));
                }
                
                Type[][] typeHierarchy = {
                    new Type[] { typeof(byte),  typeof(sbyte), typeof(char) },
                    new Type[] { typeof(short), typeof(ushort) },
                    new Type[] { typeof(int), typeof(uint) },
                    new Type[] { typeof(long), typeof(ulong) },
                    new Type[] { typeof(float) },
                    new Type[] { typeof(double) }
                };

                IEnumerable<Type> lowerTypes = Enumerable.Empty<Type>();
                foreach (Type[] types in typeHierarchy)
                {
                    if (types.Any(t => t == to))
                    {
                        return lowerTypes.Any(t => t == from);
                    }
                        
                    lowerTypes = lowerTypes.Concat(types);
                }

                return false; // IntPtr, UIntPtr, Enum, Boolean
            }

            return HasCastOperator(to, m => m.GetParameters()[0].ParameterType, _ => from, implicitly, false)
                || HasCastOperator(from, _ => to, m => m.ReturnType, implicitly, true);
        }

        static bool HasCastOperator(
            Type type, Func<MethodInfo, Type> baseType, 
            Func<MethodInfo, Type> derivedType, bool implicitly, bool lookInBase
        ) {
            var bindingFlags = BindingFlags.Public | BindingFlags.Static
                            | (lookInBase ? BindingFlags.FlattenHierarchy : BindingFlags.DeclaredOnly);

            return type.GetMethods(bindingFlags).Any(
                m => (m.Name == "op_Implicit" || (!implicitly && m.Name == "op_Explicit"))
                    && baseType(m).IsAssignableFrom(derivedType(m))
            );
        }

        /// <summary>
        /// Generate a list of USS classes that can represent this type.
        /// 
        /// Special properties of the type are also represented as additional classes
        /// (e.g. <c>type-is-enumerable</c> and <c>type-is-generic</c>)
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToUSSClasses(this Type type)
        {
            // TODO: Better variant that handles lists and such.
            // E.g. lists end up something like:
            // type-System-Collections-Generic-List`1[[System-Single, mscorlib, Ver... etc
            var classes = new List<string>();
            var name = type.ToPrettyName();

            if (type.IsCastableTo(typeof(IEnumerable)))
            {
                classes.Add("type-is-enumerable");
            }

            if (type.IsCastableTo(typeof(ICollection)))
            {
                classes.Add("type-is-collection");
            }
            
            if (type.IsGenericType)
            {
                classes.Add("type-is-generic");

                // Use the type inside the generic as the name
                name = type.GenericTypeArguments[0].ToPrettyName();
            }

            if (type.IsEnum)
            {
                classes.Add("type-is-enum");
            }

            // Add a class for the resolved name itself
            classes.Add("type-" + Regex.Replace(name, @"[^a-zA-Z0-9]+", "-").Trim('-'));
            return classes;
        }

        /// <summary>
        /// Convert the type name to something more human readable
        /// </summary>
        /// <remarks>
        /// Code adapted from https://stackoverflow.com/a/56281483
        /// </remarks>
        public static string ToPrettyName(this Type type)
        {
            if (k_Names.TryGetValue(type, out string name))
            {
                return name;
            }

            var args = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
            var format = Regex.Replace(type.FullName, @"`\d+.*", "") + (type.IsGenericType ? "<?>" : "");
            var names = args.Select((arg) => arg.IsGenericParameter ? "" : arg.ToPrettyName());

            name = string.Join(string.Join(",", names), format.Split('?'));
            k_Names.Add(type, name);

            return name;
        }
    }
}
