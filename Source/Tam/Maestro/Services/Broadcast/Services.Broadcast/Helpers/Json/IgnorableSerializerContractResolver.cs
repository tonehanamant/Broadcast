using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Services.Broadcast.Helpers.Json
{
    /// <summary>
    /// A contract resolver that can ignore properties as directed.
    /// </summary>
    public class IgnorableSerializerContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Items to ignore.
        /// </summary>
        protected readonly Dictionary<Type, HashSet<string>> Ignores;

        /// <summary>
        /// Constructor.
        /// </summary>
        public IgnorableSerializerContractResolver()
        {
            this.Ignores = new Dictionary<Type, HashSet<string>>();
        }

        /// <summary>
        /// Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void Ignore(Type type, params string[] propertyName)
        {
            // start bucket if DNE
            if (!this.Ignores.ContainsKey(type)) this.Ignores[type] = new HashSet<string>();

            foreach (var prop in propertyName)
            {
                this.Ignores[type].Add(prop);
            }
        }

        /// <summary>
        /// Is the given property for the given type ignored?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsIgnored(Type type, string propertyName)
        {
            if (!this.Ignores.ContainsKey(type)) return false;

            // if no properties provided, ignore the type entirely
            if (this.Ignores[type].Count == 0) return true;

            return this.Ignores[type].Contains(propertyName);
        }

        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (this.IsIgnored(property.DeclaringType, property.PropertyName)
            // need to check basetype as well for EF -- @per comment by user576838
            // when checking an interface, property.DeclaringType.BaseType is null, which causes an ArgumentNullException in IsIgnored
            || (property.DeclaringType.BaseType != null && this.IsIgnored(property.DeclaringType.BaseType, property.PropertyName)))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }

        /// <inheritdoc />
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return properties?.OrderBy(p => p.PropertyName).ToList();
        }
    }
}