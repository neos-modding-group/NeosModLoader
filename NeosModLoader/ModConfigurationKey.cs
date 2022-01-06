using System;

namespace NeosModLoader
{
    public abstract class ModConfigurationKey
    {
        internal ModConfigurationKey(string name, string description, bool internalAccessOnly)
        {
            Name = name;
            Description = description;
            InternalAccessOnly = internalAccessOnly;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool InternalAccessOnly { get; private set; }
        public abstract Type ValueType();
        public abstract bool Validate(object value);
    }

    public class ModConfigurationKey<T> : ModConfigurationKey
    {
        public ModConfigurationKey(string name, string description, bool internalAccessOnly = false, Predicate<T> valueValidator = null) : base(name, description, internalAccessOnly)
        {
            IsValueValid = valueValidator;
        }

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
