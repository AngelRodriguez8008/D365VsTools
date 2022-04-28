using System;

namespace D365VsTools.CodeGenerator
{
    public class MapperEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string MessageExtended { get; set; }
    }
}
