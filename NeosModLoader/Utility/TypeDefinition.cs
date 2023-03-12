using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeosModLoader.Utility
{
	public readonly struct TypeDefinition : IEquatable<TypeDefinition>, IEnumerable<Type>
	{
		internal readonly Type[] types;

		public int Length => types.Length;

		public TypeDefinition(params Type[] types)
			=> this.types = types ?? Array.Empty<Type>();

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
			return types.SequenceEqual(other.types);
		}

		public IEnumerator<Type> GetEnumerator()
			=> ((IEnumerable<Type>)types).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => types.GetEnumerator();

		public override int GetHashCode()
		{
			return unchecked(types.Aggregate(0, (acc, type) => (31 * acc) + type.GetHashCode()));
		}
	}
}
