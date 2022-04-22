using System;

namespace Namiko.Handlers.Attributes
{
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; set; }
        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
