using System;
using System.Collections.Generic;

namespace NeosModLoader
{
    /// <summary>
    /// Untyped mod configuration key.
    /// </summary>
    public abstract class ModConfigurationKey
    {
        internal ModConfigurationKey(string name, string description, bool internalAccessOnly)
        {
            Name = name;
            Description = description;
            InternalAccessOnly = internalAccessOnly;
        }

        /// <summary>
        /// Unique name of this config item
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Human-readable description of this config item
        /// </summary>
        public string Description { get; private set; }

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
        public abstract bool Validate(object value);

        public override bool Equals(object obj)
        {
            return obj is ModConfigurationKey key &&
                   Name == key.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
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
        /// <param name="internalAccessOnly">If true, only the owning mod should have access to this config item</param>
        /// <param name="valueValidator">Checks if a value is valid for this configuration item</param>
        public ModConfigurationKey(string name, string description, bool internalAccessOnly = false, Predicate<T> valueValidator = null) : base(name, description, internalAccessOnly)
        {
            IsValueValid = valueValidator;
        }

        /// <summary>
        /// Checks if a value is valid for this configuration item
        /// </summary>
        public Predicate<T> IsValueValid { get; private set; }
        public override Type ValueType() => typeof(T);
        public override bool Validate(object value)
        {
            if (value is T castValue)
            {
                return IsValueValid(castValue);
            }
            else
            {
                return false;
            }
        }
    }
}
