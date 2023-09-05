using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeosModLoader.Utility
{
	/// <summary>
	/// Represents a generic method invoker that invokes potentially generic methods on generic types.
	/// </summary>
	public sealed class GenericTypeMethodsInvoker
	{
		private readonly Dictionary<TypeDefinition, ConcreteType> concreteTypes = new();

		/// <summary>
		/// Gets the generic <see cref="Type"/> who's concrete versions will have concrete methods invoked by this invoker.
		/// </summary>
		public Type GenericType { get; }

		/// <summary>
		/// Gets the function that handles resolving passed in <see cref="MethodInfo"/>s of the <see cref="GenericType">GenericType</see> to ones of the concrete one.
		/// </summary>
		public Func<MethodInfo, Type, MethodInfo> GetGenericMethodOfConcreteType { get; }

		/// <summary>
		/// Creates a new generic type methods invoker with the given generic type and the default concrete method resolving function.
		/// </summary>
		/// <param name="genericType">The generic type who's concrete versions will have concrete methods invoked by this invoker.</param>
		public GenericTypeMethodsInvoker(Type genericType)
			: this(genericType, GetGenericMethodOfConcreteTypeDefault)
		{ }

		/// <summary>
		/// Creates a new generic type methods invoker with the given generic type and method resolving function.
		/// </summary>
		/// <param name="genericType">The generic type who's concrete versions will have concrete methods invoked by this invoker.</param>
		/// <param name="getGenericMethodOfConcreteType">The function that handles resolving passed in <see cref="MethodInfo"/>s of the <see cref="GenericType">GenericType</see> to ones of the concrete one.</param>
		public GenericTypeMethodsInvoker(Type genericType, Func<MethodInfo, Type, MethodInfo> getGenericMethodOfConcreteType)
		{
			GenericType = genericType;
			GetGenericMethodOfConcreteType = getGenericMethodOfConcreteType;
		}

		/// <summary>
		/// Invokes a concrete version of the given generic method using the given <paramref name="instance"/>,
		/// <paramref name="methodDefinition"/> type parameters and method <paramref name="parameters"/> on the
		/// concrete version of this instance's <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="methodDefinition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public TReturn Invoke<TReturn>(MethodInfo method, TypeDefinition instanceDefinition, object instance, TypeDefinition methodDefinition, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceDefinition, instance, methodDefinition, parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given generic method using the given <paramref name="instance"/>,
		/// <paramref name="methodDefinition"/> type parameters and method <paramref name="parameters"/> on the
		/// concrete version of this instance's <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="methodDefinition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public object Invoke(MethodInfo method, TypeDefinition instanceDefinition, object instance, TypeDefinition methodDefinition, params object[] parameters)
		{
			return InvokeInternal(method, instanceDefinition, instance, methodDefinition, parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given method on the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public TReturn Invoke<TReturn>(MethodInfo method, TypeDefinition instanceDefinition, object instance, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceDefinition, instance, new TypeDefinition(), parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given static method of the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public TReturn Invoke<TReturn>(MethodInfo method, TypeDefinition instanceDefinition, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceDefinition, null, new TypeDefinition(), parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given method on the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public object Invoke(MethodInfo method, TypeDefinition instanceDefinition, object instance, params object[] parameters)
		{
			return InvokeInternal(method, instanceDefinition, instance, new TypeDefinition(), parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given static method of the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the <paramref name="instanceDefinition"/> type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instanceDefinition">The generic type parameter definition to create the concrete instance type with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		public object Invoke(MethodInfo method, TypeDefinition instanceDefinition, params object[] parameters)
		{
			return InvokeInternal(method, instanceDefinition, null, new TypeDefinition(), parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given method on the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the given <paramref name="instance"/>'s type parameters.
		/// </summary>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		/// <exception cref="ArgumentException">The <paramref name="instance"/>is not descended from the <see cref="GenericType">GenericType</see>.</exception>
		public object Invoke(MethodInfo method, object instance, params object[] parameters)
		{
			return InvokeInternal(method, GetMatchingGenericTypeArguments(instance), instance, new TypeDefinition(), parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given generic method using the given <paramref name="instance"/>,
		/// <paramref name="methodDefinition"/> type parameters and method <paramref name="parameters"/> on the
		/// concrete version of this instance's <see cref="GenericType">GenericType</see> created with the given <paramref name="instance"/>'s type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="methodDefinition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		/// <exception cref="ArgumentException">The <paramref name="instance"/>is not descended from the <see cref="GenericType">GenericType</see>.</exception>
		public object Invoke(MethodInfo method, object instance, TypeDefinition methodDefinition, params object[] parameters)
		{
			return InvokeInternal(method, GetMatchingGenericTypeArguments(instance), instance, methodDefinition, parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given generic method using the given <paramref name="instance"/>,
		/// <paramref name="methodDefinition"/> type parameters and method <paramref name="parameters"/> on the
		/// concrete version of this instance's <see cref="GenericType">GenericType</see> created with the given <paramref name="instance"/>'s type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="methodDefinition">The generic type parameter definition to create the concrete method with.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		/// <exception cref="ArgumentException">The <paramref name="instance"/>is not descended from the <see cref="GenericType">GenericType</see>.</exception>
		public TReturn Invoke<TReturn>(MethodInfo method, object instance, TypeDefinition methodDefinition, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, GetMatchingGenericTypeArguments(instance), instance, methodDefinition, parameters);
		}

		/// <summary>
		/// Invokes a concrete version of the given method on the concrete version of this instance's
		/// <see cref="GenericType">GenericType</see> created with the given <paramref name="instance"/>'s type parameters.
		/// </summary>
		/// <typeparam name="TReturn">The invoked method's return type.</typeparam>
		/// <param name="method">The generic method of the generic type to invoke.</param>
		/// <param name="instance">The instance to invoke the constructed concrete method on.</param>
		/// <param name="parameters">The parameters to invoke the method with. May be empty if there is none.</param>
		/// <returns>The invoked method's return value.</returns>
		/// <exception cref="ArgumentException">The <paramref name="instance"/>is not descended from the <see cref="GenericType">GenericType</see>.</exception>
		public TReturn Invoke<TReturn>(MethodInfo method, object instance, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, GetMatchingGenericTypeArguments(instance), instance, new TypeDefinition(), parameters);
		}

		private static MethodInfo GetGenericMethodOfConcreteTypeDefault(MethodInfo needleMethod, Type concreteType)
		{
			Logger.DebugFuncInternal(() => $"Looking for: {needleMethod.FullDescription()}");

			return concreteType.GetMethods(AccessTools.all)
				.Single(hayMethod =>
				{
					if (hayMethod.Name != needleMethod.Name)
						return false;

					Logger.DebugFuncInternal(() => $"Testing potential candidate: {hayMethod.FullDescription()}");

					var needleParameters = needleMethod.GetParameters();
					var hayParameters = hayMethod.GetParameters();

					if (hayParameters.Length != needleParameters.Length)
						return false;

					for (var i = 0; i < needleParameters.Length; ++i)
					{
						//var needleParameter = needleParameters[i];
						//var hayParameter = hayParameters[i];
						//var checkType = (hayParameter.ParameterType.IsGenericParameter && needleParameter.ParameterType.IsGenericParameter)
						//             || (!hayParameter.ParameterType.IsGenericParameter && !needleParameter.ParameterType.IsGenericParameter);

						//NeosMod.Msg($"Comparing: {hayParameter.ParameterType} to {needleParameter.ParameterType} => {hayParameter.ParameterType.FullName == needleParameter.ParameterType.FullName}");

						//if (checkType && hayParameter.ParameterType.FullName != needleParameter.ParameterType.FullName)
						//    return false;

						// TODO: Do a proper type check? lol
						if (hayParameters[i].Name != needleParameters[i].Name)
							return false;
					}

					return true;
				});
		}

		private Type[] GetMatchingGenericTypeArguments(object instance)
		{
			var type = instance.GetType();

			do
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == GenericType)
					return type.GenericTypeArguments;

				type = type.BaseType;
			}
			while (type != null);

			throw new ArgumentException(
				$"Provided instance [{instance.GetType().FullDescription()}] was not descended from {nameof(GenericType)} [{GenericType.FullDescription()}].",
				nameof(instance));
		}

		private object InvokeInternal(MethodInfo method, TypeDefinition instanceTypes, object? instance, TypeDefinition methodTypes, object[] parameters)
		{
			if (!concreteTypes.TryGetValue(instanceTypes, out var concreteType))
			{
				concreteType = GenericType.MakeGenericType(instanceTypes.types);
				concreteTypes.Add(instanceTypes, concreteType);
			}

			var methodInvoker = concreteType.GetMethodInvoker(method, GetGenericMethodOfConcreteType);

			return methodInvoker.InvokeInternal(instance, methodTypes, parameters);
		}

		private readonly struct ConcreteType
		{
			public readonly Dictionary<MethodInfo, BaseGenericMethodInvoker<object, object>> MethodInvokers = new();
			public readonly Type Type;

			public ConcreteType(Type type)
			{
				Type = type;
			}

			public static implicit operator ConcreteType(Type type) => new(type);

			public BaseGenericMethodInvoker<object, object> GetMethodInvoker(MethodInfo genericMethod, Func<MethodInfo, Type, MethodInfo> getMethod)
			{
				if (!MethodInvokers.TryGetValue(genericMethod, out var methodInvoker))
				{
					methodInvoker = new GenericInstanceMethodInvoker<object, object>(getMethod(genericMethod, Type), true);
					MethodInvokers.Add(genericMethod, methodInvoker);
				}

				return methodInvoker;
			}
		}
	}
}
