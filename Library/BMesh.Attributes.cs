// Copyright (C) 2021-2022 Steffen Itterheim
// Usage is bound to the Unity Asset Store Terms of Service and EULA: https://unity3d.com/legal/as_terms

using UnityEngine;

public partial class BMesh
{
	/**
     * Attribute type is used when declaring new attributes to be automatically
     * attached to topological entities, using Add*Attributes() methods.
     */
	public class AttributeType
	{
		public AttributeBaseType baseType;
		public int dimensions;

		/**
         * Checks whether a given value matches this type.
         */
		public bool CheckValue(AttributeValue value)
		{
			Debug.Assert(dimensions > 0);
			switch (baseType)
			{
				case AttributeBaseType.Int:
				{
					var valueAsInt = value as IntAttributeValue;
					return valueAsInt != null && valueAsInt.data.Length == dimensions;
				}
				case AttributeBaseType.Float:
				{
					var valueAsFloat = value as FloatAttributeValue;
					return valueAsFloat != null && valueAsFloat.data.Length == dimensions;
				}
				default:
					Debug.Assert(false);
					return false;
			}
		}
	}

	/**
     * The generic class of values stored in the attribute dictionnary in each
     * topologic entity. It contains an array of float or int, depending on its
     * type.
     */
	public class AttributeValue
	{
		/**
         * Deep copy of an attribute value.
         */
		public static AttributeValue Copy(AttributeValue value)
		{
			if (value is IntAttributeValue valueAsInt)
			{
				var data = new int[valueAsInt.data.Length];
				valueAsInt.data.CopyTo(data, 0);
				return new IntAttributeValue { data = data };
			}
			if (value is FloatAttributeValue valueAsFloat)
			{
				var data = new float[valueAsFloat.data.Length];
				valueAsFloat.data.CopyTo(data, 0);
				return new FloatAttributeValue { data = data };
			}
			Debug.Assert(false);
			return null;
		}

		/**
         * Measure the euclidean distance between two attributes, which is set
         * to infinity if they have different types (int or float / dimension)
         */
		public static float Distance(AttributeValue value1, AttributeValue value2)
		{
			if (value1 is IntAttributeValue value1AsInt)
			{
				if (value2 is IntAttributeValue value2AsInt)
					return IntAttributeValue.Distance(value1AsInt, value2AsInt);
			}
			if (value1 is FloatAttributeValue value1AsFloat)
			{
				if (value2 is FloatAttributeValue value2AsFloat)
					return FloatAttributeValue.Distance(value1AsFloat, value2AsFloat);
			}
			return float.PositiveInfinity;
		}

		/**
         * Cast to FloatAttributeValue (return null if it was not actually a
         * float attribute).
         */
		public FloatAttributeValue asFloat() => this as FloatAttributeValue;

		/**
         * Cast to IntAttributeValue (return null if it was not actually an
         * integer attribute).
         */
		public IntAttributeValue asInt() => this as IntAttributeValue;
	}

	/**
     * Attributes definitions are stored in the mesh to automatically add an
     * attribute with a default value to all existing and added topological
     * entities of the target type.
     */
	public class AttributeDefinition
	{
		public string name;
		public AttributeType type;
		public AttributeValue defaultValue;

		public AttributeDefinition(string name, AttributeBaseType baseType, int dimensions)
		{
			this.name = name;
			type = new AttributeType { baseType = baseType, dimensions = dimensions };
			defaultValue = NullValue();
		}

		/**
         * Return a null value of the target type
         * (should arguably be in AttributeType)
         */
		public AttributeValue NullValue()
		{
			//Debug.Assert(type.dimensions > 0);
			switch (type.baseType)
			{
				case AttributeBaseType.Int:
					return new IntAttributeValue { data = new int[type.dimensions] };
				case AttributeBaseType.Float:
					return new FloatAttributeValue { data = new float[type.dimensions] };
				default:
					Debug.Assert(false);
					return new AttributeValue();
			}
		}
	}

	public class FloatAttributeValue : AttributeValue
	{
		public float[] data;

		public static float Distance(FloatAttributeValue value1, FloatAttributeValue value2)
		{
			var n = value1.data.Length;
			if (n != value2.data.Length) return float.PositiveInfinity;

			float s = 0;
			for (var i = 0; i < n; ++i)
			{
				var diff = value1.data[i] - value2.data[i];
				s += diff * diff;
			}
			return Mathf.Sqrt(s);
		}

		public FloatAttributeValue() {}

		public FloatAttributeValue(float f) => data = new[] { f };

		public FloatAttributeValue(float f0, float f1) => data = new[] { f0, f1 };

		public FloatAttributeValue(Vector3 v) => data = new[] { v.x, v.y, v.z };

		public void FromVector2(Vector2 v)
		{
			data[0] = v.x;
			data[1] = v.y;
		}

		public void FromVector3(Vector3 v)
		{
			data[0] = v.x;
			data[1] = v.y;
			data[2] = v.z;
		}

		public void FromColor(Color c)
		{
			data[0] = c.r;
			data[1] = c.g;
			data[2] = c.b;
			data[3] = c.a;
		}

		public Vector3 AsVector3() => new(
			data.Length > 0 ? data[0] : 0,
			data.Length > 1 ? data[1] : 0,
			data.Length > 2 ? data[2] : 0
		);

		public Color AsColor() => new(
			data.Length > 0 ? data[0] : 0,
			data.Length > 1 ? data[1] : 0,
			data.Length > 2 ? data[2] : 0,
			data.Length > 3 ? data[3] : 1
		);
	}

	public class IntAttributeValue : AttributeValue
	{
		public int[] data;

		public static float Distance(IntAttributeValue value1, IntAttributeValue value2)
		{
			var n = value1.data.Length;
			if (n != value2.data.Length) return float.PositiveInfinity;

			float s = 0;
			for (var i = 0; i < n; ++i)
			{
				float diff = value1.data[i] - value2.data[i];
				s += diff * diff;
			}
			return Mathf.Sqrt(s);
		}

		public IntAttributeValue() {}

		public IntAttributeValue(int i) => data = new[] { i };

		public IntAttributeValue(int i0, int i1) => data = new[] { i0, i1 };
	}
}