using System;
using System.Collections.Generic;

namespace NeosModLoader
{
    /// <summary>
    /// Untyped mod configuration key.
    /// </summary>
    public abstract class ModConfigurationKey
    {
        internal ModConfigurationKey(string name, string? description, bool internalAccessOnly)
        {
            Name = name ?? throw new ArgumentNullException("Configuration key name must not be null");
            Description = description;
            InternalAccessOnly = internalAccessOnly;
        }

        /// <summary>
        /// Unique name of this config item. Must be present.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Human-readable description of this config item. Should be specified by the defining mod.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// If true, only the owning mod should have access to this config item
        /// </summary>
        public bool InternalAccessOnly { get; private set; }

        /// <returns>Type of the config item</returns>
        public abstract Type ValueType();

        /// <summary>
        /// Checks if a value is valid for this configuration item
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>true if the value is valid</returns>
        public abstract bool Validate(object? value);

        /// <summary>
        /// Computes a default value for this key, if a default provider is set
        /// </summary>
        /// <param name="defaultValue">the computed default value, or null if no default provider was set</param>
        /// <returns>true if a default was computed</returns>
        public abstract bool TryComputeDefault(out object? defaultValue);

        // We only care about key name for non-defining keys.
        // For defining keys all of the other properties (default, validator, etc.) also matter.
        public override bool Equals(object obj)
        {
            return obj is ModConfigurationKey key &&
                   Name == key.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        private object? Value;
        internal bool HasValue;

        /// <summary>
        /// Each configuration item has exactly ONE defining key, and that is the key defined by the mod.
        /// Duplicate keys can be created (they only need to share the same Name) and they'll still work
        /// for reading configs.
        /// 
        /// This is a non-null self-reference for the defining key itself as soon as the definition is done initializing.
        /// </summary>
        internal ModConfigurationKey? DefiningKey;

        internal bool TryGetValue(out object? value)
        {
            if (HasValue)
            {
                value = Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        internal void Set(object? value)
        {
            Value = value;
            HasValue = true;
        }

        internal bool Unset()
        {
            bool hadValue = HasValue;
            HasValue = false;
            return hadValue;
        }
    }

    /// <summary>
    /// Typed mod configuration key.
    /// </summary>
    /// <typeparam name="T">The value's type</typeparam>
    public class ModConfigurationKey<T> : ModConfigurationKey
    {
        /// <summary>
        /// Creates a new ModConfigurationKey
        /// </summary>
        /// <param name="name">Unique name of this config item</param>
        /// <param name="description">Human-readable description of this config item</param>
        /// <param name="computeDefault">Function that, if present, computes a default value for this key</param>
        /// <param name="internalAccessOnly">If true, only the owning mod should have access to this config item</param>
        /// <param name="valueValidator">Checks if a value is valid for this configuration item</param>
        public ModConfigurationKey(string name, string? description = null, Func<T>? computeDefault = null, bool internalAccessOnly = false, Predicate<T?>? valueValidator = null) : base(name, description, internalAccessOnly)
        {
            ComputeDefault = computeDefault;
            IsValueValid = valueValidator;
        }

        private readonly Func<T>? ComputeDefault;
        private readonly Predicate<T?>? IsValueValid;

        /// <summary>
        /// Get the type of this key's value
        /// </summary>
        /// <returns>the type of this key's value</returns>
        public override Type ValueType() => typeof(T);

        /// <summary>
        /// Checks if a value is valid for this configuration item
        /// </summary>
        /// <param name="value">value to check</param>
        /// <returns>true if the value is valid</returns>
        public override bool Validate(object? value)
        {
            // specifically allow nulls for class types
            if (value is T || (value is null && !typeof(T).IsValueType))
            {
                return ValidateTyped((T?)value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a value is valid for this configuration item
        /// </summary>
        /// <param name="value">value to check</param>
        /// <returns>true if the value is valid</returns>
        public bool ValidateTyped(T? value)
        {
            if (IsValueValid == null)
            {
                return true;
            }
            else
            {
                return IsValueValid(value);
            }
        }

        public override bool TryComputeDefault(out object? defaultValue)
        {
            if (TryComputeDefaultTyped(out T? defaultTypedValue))
            {
                defaultValue = defaultTypedValue;
                return true;
            }
            else
            {
                defaultValue = null;
                return false;
            }
        }

        /// <summary>
        /// Computes a default value for this key, if a default provider is set
        /// </summary>
        /// <param name="defaultValue">the computed default value, or default(T) if no default provider was set</param>
        /// <returns>true if a default was computed</returns>
        public bool TryComputeDefaultTyped(out T? defaultValue)
        {
            if (ComputeDefault == null)
            {
                defaultValue = default;
                return false;
            }
            else
            {
                defaultValue = ComputeDefault();
                return true;
            }
        }
    }
}
