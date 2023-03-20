using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// Makes the .NET runtime ignore access of private members of the <see cref="Assembly"/> with the given name.<br/>
	/// Use when building against publicized assemblies to prevent problems if Neos ever switches from running on Mono,
	/// where checking the "Allow Unsafe Code" option in the Project Settings is enough.<br/>
	/// <para/>
	/// Usage: <c>[assembly: IgnoresAccessChecksTo("FrooxEngine")]</c>
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class IgnoresAccessChecksToAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of the Assembly to ignore access checks to.
		/// </summary>
		public string AssemblyName { get; }

		/// <summary>
		/// Makes the .NET runtime ignore access of private members of the <see cref="Assembly"/> with the given name.<br/>
		/// Use when building against publicized assemblies to prevent problems if Neos ever switches from running on Mono,
		/// where checking the "Allow Unsafe Code" option in the Project Settings is enough.<br/>
		/// <para/>
		/// Usage: <c>[assembly: IgnoresAccessChecksTo("FrooxEngine")]</c>
		/// </summary>
		/// <param name="assemblyName">The name of the Assembly to ignore access checks to.</param>
		public IgnoresAccessChecksToAttribute(string assemblyName)
		{
			AssemblyName = assemblyName;
		}
	}
}
