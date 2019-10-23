using System;
using UnityEngine;

namespace BlueGraphExamples
{
    /// <summary>
    /// Experimental type that allows you to pass any vector dimension around in a graph.
    /// 
    /// This demos the IConvertible support in BlueGraph for handling type conversions 
    /// for port connections that aren't primitive types or share a common inherited class
    /// </summary>
    public struct DynamicVector : IConvertible
    {
        readonly Vector4 value;

        public DynamicVector(float x, float y, float z, float w)
        {
            value = new Vector4(x, y, z, w);
        }
        
        public static implicit operator float(DynamicVector v) => v.value.x;
        public static implicit operator Vector2(DynamicVector v) => new Vector2(v.value.x, v.value.y);
        public static implicit operator Vector3(DynamicVector v) => new Vector3(v.value.x, v.value.y, v.value.z);
        public static implicit operator Vector4(DynamicVector v) => v.value;

        public static implicit operator DynamicVector(float f) => new DynamicVector(f, f, f, f);
        public static implicit operator DynamicVector(Vector2 v) => new DynamicVector(v.x, v.y, 0, 0);
        public static implicit operator DynamicVector(Vector3 v) => new DynamicVector(v.x, v.y, v.z, 0);
        public static implicit operator DynamicVector(Vector4 v) => new DynamicVector(v.x, v.y, v.z, v.w);
    
        public bool CanCastTo(Type type)
        {
            return  type == typeof(float) || 
                    type == typeof(Vector3) || 
                    type == typeof(Vector3) || 
                    type == typeof(Vector4); 
        }

        public TypeCode GetTypeCode() => TypeCode.Object;

        // Unsupported casts
        public DateTime ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
        public byte ToByte(IFormatProvider provider) => throw new InvalidCastException();
        public sbyte ToSByte(IFormatProvider provider) => throw new InvalidCastException();
        public char ToChar(IFormatProvider provider) => throw new InvalidCastException();
        public bool ToBoolean(IFormatProvider provider) => throw new InvalidCastException();
    
        // Supported casts
        public float ToSingle(IFormatProvider provider) { Debug.Log("DV:" + value.x); return value.x; }
        public decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(value.x);
        public double ToDouble(IFormatProvider provider) => Convert.ToDouble(value.x);
        public string ToString(IFormatProvider provider) => $"({value.x}, {value.y}, {value.z})";

        public short ToInt16(IFormatProvider provider) => Convert.ToInt16(value.x);
        public int ToInt32(IFormatProvider provider) => Convert.ToInt32(value.x);
        public long ToInt64(IFormatProvider provider) => Convert.ToInt64(value.x);
        public ushort ToUInt16(IFormatProvider provider) => Convert.ToUInt16(value.x);
        public uint ToUInt32(IFormatProvider provider) => Convert.ToUInt32(value.x);
        public ulong ToUInt64(IFormatProvider provider) => Convert.ToUInt64(value.x);

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == GetType()) return this;
            if (conversionType == typeof(Vector2)) return (Vector2)this;
            if (conversionType == typeof(Vector3)) return (Vector3)this;
            if (conversionType == typeof(Vector4)) return (Vector4)this;

            throw new InvalidCastException();
        }
    }
}
