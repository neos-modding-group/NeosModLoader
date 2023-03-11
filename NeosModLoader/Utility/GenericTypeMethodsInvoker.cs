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
	internal sealed class GenericTypeMethodsInvoker
	{
		private readonly Dictionary<TypeDefinition, ConcreteType> concreteTypes = new();

		public Type GenericType { get; }

		public Func<MethodInfo, Type, MethodInfo> GetGenericMethodOfConcreteType { get; }

		public GenericTypeMethodsInvoker(Type genericType)
			: this(genericType, GetGenericMethodOfConcreteTypeDefault)
		{ }

		public GenericTypeMethodsInvoker(Type genericType, Func<MethodInfo, Type, MethodInfo> getGenericMethodOfConcreteType)
		{
			GenericType = genericType;
			GetGenericMethodOfConcreteType = getGenericMethodOfConcreteType;
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type[] instanceTypes, object instance, Type[] methodTypes, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceTypes, instance, methodTypes, parameters);
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type[] instanceTypes, object instance, Type methodType, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceTypes, instance, methodType, parameters);
		}

		public object Invoke(MethodInfo method, Type[] instanceTypes, object instance, Type[] methodTypes, params object[] parameters)
		{
			return InvokeInternal(method, instanceTypes, instance, methodTypes, parameters);
		}

		public object Invoke(MethodInfo method, Type instanceType, object instance, Type[] methodTypes, params object[] parameters)
		{
			return InvokeInternal(method, instanceType, instance, methodTypes, parameters);
		}

		public object Invoke(MethodInfo method, Type[] instanceTypes, object instance, Type methodType, params object[] parameters)
		{
			return InvokeInternal(method, instanceTypes, instance, methodType, parameters);
		}

		public object Invoke(MethodInfo method, Type instanceType, object instance, Type methodType, params object[] parameters)
		{
			return InvokeInternal(method, instanceType, instance, methodType, parameters);
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type instanceType, object instance, Type[] methodTypes, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceType, instance, methodTypes, parameters);
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type instanceType, object instance, Type methodType, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceType, instance, methodType, parameters);
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type instanceType, object instance, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceType, instance, new TypeDefinition(), parameters);
		}

		public TReturn Invoke<TReturn>(MethodInfo method, Type instanceType, params object[] parameters)
		{
			return (TReturn)InvokeInternal(method, instanceType, null, new TypeDefinition(), parameters);
		}

		public object Invoke(MethodInfo method, Type[] instanceTypes, object instance, params object[] parameters)
		{
			return InvokeInternal(method, instanceTypes, instance, new TypeDefinition(), parameters);
		}

		public object Invoke(MethodInfo method, Type[] instanceTypes, params object[] parameters)
		{
			return InvokeInternal(method, instanceTypes, null, new TypeDefinition(), parameters);
		}

		private static MethodInfo GetGenericMethodOfConcreteTypeDefault(MethodInfo needleMethod, Type concreteType)
		{
			NeosMod.Debug($"Looking for: {needleMethod.ReturnType.Name} {needleMethod.Name}({string.Join(", ", needleMethod.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");

			return concreteType.GetMethods(AccessTools.all)
				.Single(hayMethod =>
				{
					NeosMod.Debug($"Testing: {hayMethod.ReturnType.Name} {hayMethod.Name}({string.Join(", ", hayMethod.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");

					if (hayMethod.Name != needleMethod.Name)
						return false;

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

		private object InvokeInternal(MethodInfo method, TypeDefinition instanceTypes, object? instance, TypeDefinition methodTypes, object[]? parameters)
		{
			if (!concreteTypes.TryGetValue(instanceTypes, out var concreteType))
			{
				concreteType = GenericType.MakeGenericType(instanceTypes.Types);
				concreteTypes.Add(instanceTypes, concreteType);
			}

			var methodInvoker = concreteType.GetMethodInvoker(method, GetGenericMethodOfConcreteType);

			return methodInvoker.InvokeInternal(instance, methodTypes, parameters);
		}

		private readonly struct ConcreteType
		{
			public readonly Dictionary<MethodInfo, GenericMethodInvoker<object, object>> MethodInvokers = new();
			public readonly Type Type;

			public ConcreteType(Type type)
			{
				Type = type;
			}

			public static implicit operator ConcreteType(Type type) => new(type);

			public GenericMethodInvoker<object, object> GetMethodInvoker(MethodInfo genericMethod, Func<MethodInfo, Type, MethodInfo> getMethod)
			{
				if (!MethodInvokers.TryGetValue(genericMethod, out var methodInvoker))
				{
					methodInvoker = new GenericMethodInvoker<object, object>(getMethod(genericMethod, Type), true);
					MethodInvokers.Add(genericMethod, methodInvoker);
				}

				return methodInvoker;
			}
		}
	}
}
