using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlueGraphEditor
{
    public static class TypeExtension
    {
        /// Caching of castability between types to avoid repeat reflection
        static Dictionary<(Type, Type), bool> k_CastSupportCache = new Dictionary<(Type, Type), bool>();

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
            var bindinFlags = BindingFlags.Public | BindingFlags.Static
                            | (lookInBase ? BindingFlags.FlattenHierarchy : BindingFlags.DeclaredOnly);

            return type.GetMethods(bindinFlags).Any(
                m => (m.Name == "op_Implicit" || (!implicitly && m.Name == "op_Explicit"))
                    && baseType(m).IsAssignableFrom(derivedType(m))
            );
        }
    }
}
