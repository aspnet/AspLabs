using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public class CollectionProfile
    {
        public static readonly string DefaultProfileName = "Default";

        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<EventSpec> EventSpecs { get; }

        public CollectionProfile(string name, string description, IEnumerable<EventSpec> eventSpecs)
        {
            Name = name;
            Description = description;
            EventSpecs = eventSpecs.ToList();
        }
    }
}
