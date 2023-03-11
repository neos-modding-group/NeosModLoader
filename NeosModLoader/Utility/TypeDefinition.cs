using System;
using System.Linq;

namespace NeosModLoader.Utility
{
	internal readonly struct TypeDefinition : IEquatable<TypeDefinition>
	{
		public readonly Type[] Types;

		public int Length => Types.Length;

		public TypeDefinition(params Type[] types)
		{
			Types = types ?? Array.Empty<Type>();
		}

		public static implicit operator TypeDefinition(Type[] types) => new(types);

		public static implicit operator TypeDefinition(Type type)
		{
			if (type == null)
				return new(Array.Empty<Type>());

			return new(type);
		}

		public static bool operator !=(TypeDefinition left, TypeDefinition right) => !(left == right);

		public static bool operator ==(TypeDefinition left, TypeDefinition right) => left.Equals(right);

		public override bool Equals(object obj)
		{
			return obj is TypeDefinition definition && Equals(definition);
		}

		public bool Equals(TypeDefinition other)
		{
			return Types.SequenceEqual(other.Types);
		}

		public override int GetHashCode()
		{
			return unchecked(Types.Aggregate(0, (acc, type) => (-136316459 * acc) + type.GetHashCode()));
		}
	}
}
