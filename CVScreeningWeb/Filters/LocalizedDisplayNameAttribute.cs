using System;
using System.ComponentModel;
using System.Reflection;

namespace CVScreeningWeb.Filters
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private PropertyInfo _nameProperty;
        private Type _resourceType;
        private readonly string _displayName;

        public LocalizedDisplayNameAttribute(string displayNameKey) : base(displayNameKey)
        {
            _displayName = displayNameKey;
        }

        public Type NameResourceType
        {
            get { return _resourceType; }
            set
            {
                _resourceType = value;
                _nameProperty = _resourceType.GetProperty(_displayName,
                    BindingFlags.Static | BindingFlags.Public);
            }
        }

        public override string DisplayName
        {
            get
            {
                //check if nameProperty is null and return original display name value
                if (_nameProperty == null)
                {
                    return _displayName;
                }

                return (string) _nameProperty.GetValue(_nameProperty.DeclaringType, null);
            }
        }
    }
}