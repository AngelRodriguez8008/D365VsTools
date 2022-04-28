using System;

namespace D365VsTools.CodeGenerator.Model
{
    [Serializable]
    public class Context
    {
        public string Namespace { get; set; }

        public MappingEntity[] Entities { get; set; }

        public MappingEnum[] Enums { get; set; }
    }
}
