using System;

namespace D365VsTools
{
    /// <summary>
    /// Guids used for Extension package
    /// </summary>
    public class ProjectGuids
    {
        public const string Package = "944f3eda-3d74-49f0-a2d4-a25775f1ab35";
        public const string CodeGenerator = "BB69ADDB-6AB5-4E29-B263-F918D86D1CC0";

        public static readonly Guid OutputWindow = new Guid("10B2DB3C-1CB4-43B4-80D4-A03204A616D5");
        public static readonly Guid ProjectCommandSet = new Guid("e51702bf-0cd0-413b-87ba-7d267eecc6c1");
        public static readonly Guid ItemCommandSet = new Guid("AE7DC0B9-634A-46DB-A008-D6D15DD325E1");
        public static readonly Guid FolderCommandSet = new Guid("18CFE3ED-8E6B-4BD0-BFE7-9AFF7BF02009");
    }
}
