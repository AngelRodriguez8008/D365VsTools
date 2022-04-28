﻿using System;
using System.Linq;
using D365VsTools.CodeGenerator.Helpers;
using Microsoft.Xrm.Sdk.Metadata;

namespace D365VsTools.CodeGenerator.Model
{

    [Serializable]
    public class MappingRelationship1N : MappingRelationship
    {   
        public string LogicalName { get; set; }
        
        public static MappingRelationship1N Parse(OneToManyRelationshipMetadata rel, MappingField[] properties)
        {
            var property = properties?.FirstOrDefault(p => string.Equals(p.Attribute.LogicalName, rel.ReferencedAttribute, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                return null;

            var propertyName = property.DisplayName;
            
            var result = new MappingRelationship1N
            {
                Attribute = new CrmRelationshipAttribute
                {
                    FromEntity = rel.ReferencedEntity,
                    FromKey = rel.ReferencedAttribute,
                    ToEntity = rel.ReferencingEntity,
                    ToKey = rel.ReferencingAttribute,
                    IntersectingEntity = ""
                },
                ForeignKey = propertyName,
                DisplayName = Naming.GetProperVariableName(rel.SchemaName),
                SchemaName = Naming.GetProperVariableName(rel.SchemaName),
                LogicalName = rel.ReferencingAttribute,
                PrivateName = Naming.GetEntityPropertyPrivateName(rel.SchemaName),
                HybridName = Naming.GetPluralName(Naming.GetProperVariableName(rel.SchemaName)),
                EntityRole = "null",
                Type = Naming.GetProperVariableName(rel.ReferencingEntity),
            };

            if (rel.ReferencedEntity == rel.ReferencingEntity)
            {
                result.DisplayName = "Referenced" + result.DisplayName;
                result.EntityRole = "Microsoft.Xrm.Sdk.EntityRole.Referenced";
            }

            return result;
        }
    }
}
