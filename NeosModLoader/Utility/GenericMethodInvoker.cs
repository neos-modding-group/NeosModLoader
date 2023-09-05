using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeosModLoader.Utility
{
	/// <summary>
	/// Represents the base class for more specific generic method invokers.
	/// </summary>
	/// <typeparam name="TInstance">The type of the instances that the generic method gets invoked on.</typeparam>
	/// <typeparam name="TReturn">The type of the generic method's return value. Use <c>object</c> if it depends on the generic type parameters of the method.</typeparam>
	public abstract class BaseGenericMethodInvoker<TInstance, TReturn>
	{
		private readonly Dictionary<TypeDefinition, MethodInfo> concreteMethods = new();

		/// <summary>
		/// Gets the generic <see cref="MethodInfo"/> who's concrete versions will be in invoked by this generic method invoker.
		/// </summary>
		public MethodInfo GenericMethod { get; }

		/// <summary>
		/// Creates a new generic method invoker for the given method which may not have generic type parameters.
		/// <para/>
		/// Use <c>object</c> for <typeparamref name="TReturn"/> if it depends on the generic type parameters of the method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		/// <param name="ignoreLackOfGenericParameters">Ignores the method lacking generic type parameters if <c>true</c>.</param>
		internal BaseGenericMethodInvoker(MethodInfo method, bool ignoreLackOfGenericParameters)
		{
			if (!ignoreLackOfGenericParameters && !method.ContainsGenericParameters)
				throw new ArgumentException("Target method must have remaining open type parameters.", nameof(method));

			GenericMethod = method;
		}

		/// <summary>
		/// Creates a new generic method invoker for the given open generic method.
		/// <para/>
		/// Use <c>object</c> for <typeparamref name="TReturn"/> if it depends on the generic type parameters of the method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		internal BaseGenericMethodInvoker(MethodInfo method) : this(method, false)
		{ }

		/// <summary>
		/// Invokes a concrete version of this invoker's <see cref="GenericMethod">GenericMethod</see> using the given <paramref name="instance"/>,
		/// type parameter <paramref name="definition"/> and method <paramref name="parameters"/>.
		/// </summary>
		/// <param name="instance">The object instance to invoke the method on. Use <c>null</c> for static methods.</param>
		/// <param name="definition">The generic type parameter definition to create the concrete method with.<br/>
		/// May be ignored if <see cref="GenericMethod"/> doesn't contain generic parameters.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty or null if there is none.</param>
		/// <returns>The result of the method invocation.</returns>
		internal TReturn InvokeInternal(TInstance? instance, TypeDefinition definition, params object[]? parameters)
		{
			if (!concreteMethods.TryGetValue(definition, out var method))
			{
				if (GenericMethod.ContainsGenericParameters)
					method = GenericMethod.MakeGenericMethod(definition.types);
				else
					method = GenericMethod;

				concreteMethods.Add(definition, method);
			}

			return (TReturn)method.Invoke(instance, parameters);
		}
	}

	/// <summary>
	/// Represents a generic method invoker that invokes instance methods with a return value.
	/// </summary>
	/// <inheritdoc/>
	public sealed class GenericInstanceMethodInvoker<TInstance, TReturn> : BaseGenericMethodInvoker<TInstance, TReturn>
	{
		/// <summary>
		/// Creates a new generic method invoker for the given open generic instance method with a return value.
		/// <para/>
		/// Use <c>object</c> for <typeparamref name="TReturn"/> if it depends on the generic type parameters of the method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		public GenericInstanceMethodInvoker(MethodInfo method) : base(method)
		{ }

		internal GenericInstanceMethodInvoker(MethodInfo method, bool ignoreLackOfGenericParameters)
					: base(method, ignoreLackOfGenericParameters)
		{ }

		/// <summary>
		/// Invokes a concrete version of this invoker's
		/// <see cref="BaseGenericMethodInvoker{TInstance, TReturn}.GenericMethod">GenericMethod</see>
		/// using the given <paramref name="instance"/>, type parameter <paramref name="definition"/> and method <paramref name="parameters"/>.
		/// </summary>
		/// <param name="instance">The object instance to invoke the method on.</param>
		/// <param name="definition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with.</param>
		/// <returns>The result of the method invocation.</returns>
		public TReturn Invoke(TInstance instance, TypeDefinition definition, params object[] parameters)
		{
			return InvokeInternal(instance, definition, parameters);
		}
	}

	/// <summary>
	/// Represents a generic method invoker that invokes static methods with a return value.
	/// </summary>
	/// <inheritdoc/>
	public sealed class GenericStaticMethodInvoker<TReturn> : BaseGenericMethodInvoker<object, TReturn>
	{
		/// <summary>
		/// Creates a new generic method invoker for the given open generic static method with a return value.
		/// <para/>
		/// Use <c>object</c> for <typeparamref name="TReturn"/> if it depends on the generic type parameters of the method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		public GenericStaticMethodInvoker(MethodInfo method) : base(method)
		{ }

		/// <summary>
		/// Invokes a concrete version of this invoker's static
		/// <see cref="BaseGenericMethodInvoker{TInstance, TReturn}.GenericMethod">GenericMethod</see>
		/// using the given type parameter <paramref name="definition"/> and method <paramref name="parameters"/>.
		/// </summary>
		/// <param name="definition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with.</param>
		/// <returns>The result of the method invocation.</returns>
		public TReturn Invoke(TypeDefinition definition, params object[] parameters)
		{
			return InvokeInternal(null, definition, parameters);
		}
	}

	/// <summary>
	/// Represents a generic method invoker that invokes static void methods.
	/// </summary>
	public sealed class GenericStaticVoidMethodInvoker : BaseGenericMethodInvoker<object, object>
	{
		/// <summary>
		/// Creates a new generic method invoker for the given open generic static void method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		public GenericStaticVoidMethodInvoker(MethodInfo method) : base(method)
		{ }

		/// <summary>
		/// Invokes a concrete version of this invoker's static void
		/// <see cref="BaseGenericMethodInvoker{TInstance, TReturn}.GenericMethod">GenericMethod</see>
		/// using the given type parameter <paramref name="definition"/> and method <paramref name="parameters"/>.
		/// </summary>
		/// <param name="definition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with.</param>
		/// <returns>The result of the method invocation.</returns>
		public void Invoke(TypeDefinition definition, params object[] parameters)
		{
			InvokeInternal(null, definition, parameters);
		}
	}

	/// <summary>
	/// Represents a generic method invoker that invokes instance void methods.
	/// </summary>
	/// <inheritdoc/>
	public sealed class GenericVoidMethodInvoker<TInstance> : BaseGenericMethodInvoker<TInstance, object>
	{
		/// <summary>
		/// Creates a new generic method invoker for the given open generic instance void method.
		/// </summary>
		/// <param name="method">The generic method to invoke concrete version of.</param>
		public GenericVoidMethodInvoker(MethodInfo method) : base(method)
		{ }

		/// <summary>
		/// Invokes a concrete version of this invoker's void
		/// <see cref="BaseGenericMethodInvoker{TInstance, TReturn}.GenericMethod">GenericMethod</see>
		/// using the given <paramref name="instance"/>, type parameter <paramref name="definition"/> and method <paramref name="parameters"/>.
		/// </summary>
		/// <param name="instance">The object instance to invoke the method on.</param>
		/// <param name="definition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with.</param>
		public void Invoke(TInstance instance, TypeDefinition definition, params object[] parameters)
		{
			InvokeInternal(instance, definition, parameters);
		}
	}
}
