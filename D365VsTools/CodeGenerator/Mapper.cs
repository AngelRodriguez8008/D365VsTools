using System;
using System.Collections.Generic;
using System.Linq;
using D365VsTools.CodeGenerator.Model;
using D365VsTools.Extensions;
using D365VsTools.VisualStudio;
using D365VsTools.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Context = D365VsTools.CodeGenerator.Model.Context;

namespace D365VsTools.CodeGenerator
{
    public class Mapper : IDisposable
    {
        const string ActivityParty = "activityparty";
        private readonly MappingSettings settings;

        private EntityMetadata[] metadataCache;

        public Mapper(MappingSettings settings)
        {
            this.settings = settings;
        }
        
        public void Dispose()
        {
            metadataCache = null;
            GC.Collect();
        }

        public Context CreateContext(IOrganizationService service)
        {
            Logger.WriteLine("Gathering metadata, this may take a few minutes...");
            metadataCache = GetMetadataFromServer(service);
            Logger.WriteLine($"Entities Metadata retrieved from server: {metadataCache.Length}");

            var result = CreateContextFromCache();
            return result;
        }

        public Context CreateContextFromCache()
        {
            if(metadataCache == null)
                throw new ArgumentNullException("The Metadata Cache was not initialized yet", nameof(metadataCache));

            var selectedEntities = GetSelectedEntities();
            Logger.WriteLine($"Selected Entities Metadata: {selectedEntities.Count}");

            var entities = GetEntities(selectedEntities);
            var enums = GetGlobalEnums(entities);
            var result = new Context
            {
                Entities = entities,
                Enums = enums
            };
            SortEntities(result);
            SortEnums(result);
            return result;
        }

        private EntityMetadata[] GetMetadataFromServer(IOrganizationService service)
        {
            var selection = settings.Entities.Keys;
            if (selection.Count > 20)
                return service.GetAllEntitiesMetadata();

            var selectedEntities = selection.ToList();
            var isActivityPartySelected = selectedEntities.Any(x => x.Equals(ActivityParty));
            if (isActivityPartySelected == false)
                selectedEntities.Add(ActivityParty);

            var result = service.GetEntitiesMetadata(selectedEntities);
            return result;
        }

        public void SortEntities(Context context)
        {
            context.Entities = context.Entities.OrderBy(e => e.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.Enums = e.Enums.OrderBy(en => en.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.Fields = e.Fields.OrderBy(f => f.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.RelationshipsOneToMany = e.RelationshipsOneToMany.OrderBy(r => r.LogicalName).ToArray();

            foreach (var e in context.Entities)
                e.RelationshipsManyToOne = e.RelationshipsManyToOne.OrderBy(r => r.LogicalName).ToArray();
        }

        public void SortEnums(Context context)
        {
            context.Enums = context.Enums.OrderBy(e => e.DisplayName).ToArray();
        }

        public MappingEnum[] GetGlobalEnums(MappingEntity[] entities)
        {
            var uniqueNames = new List<string>();
            List<MappingEnum> globalEnums = new List<MappingEnum>();

            foreach (MappingEntity entity in entities)
            {
                foreach (MappingEnum e in entity.Enums.Where(w => w.IsGlobal))
                {
                    if (!uniqueNames.Contains(e.GlobalName))
                    {
                        globalEnums.Add(e);
                        uniqueNames.Add(e.GlobalName);
                    }
                }
            }

            return globalEnums.ToArray();
        }

        public MappingEntity[] GetEntities(IEnumerable<EntityMetadata> entitiesMetadata)
        {
            Dictionary<string, MappingEntity> mappedEntities = entitiesMetadata.Select(e =>
                                                     {
                                                         EntityMappingSetting mapping = null;
                                                         settings.Entities?.TryGetValue(e.LogicalName, out mapping);
                                                         var r = MappingEntity.Parse(e, mapping);
                                                         return r;
                                                     })
                                                   .OrderBy(e => e.DisplayName)
                                                   .ToDictionary(e => e.LogicalName);

            var result = mappedEntities.Values.ToArray();
            FilterRelationshipsNotIncludedInMapping(result);

            foreach (MappingEntity mapping in mappedEntities.Values)
            {
                foreach (var field in mapping.Fields)
                {
                    string lookupSingleType = field.LookupSingleType;
                    if (string.IsNullOrWhiteSpace(lookupSingleType))
                        continue;

                    MappingEntity mappedEntity = mappedEntities.GetValueOrDefault(lookupSingleType);
                    string mappedName = mappedEntity?.MappedName;
                    if (string.IsNullOrWhiteSpace(mappedName) == false)
                        field.LookupSingleType = mappedName;
                }

                foreach (var rel in mapping.RelationshipsOneToMany)
                    rel.ToEntity = mappedEntities.GetValueOrDefault(rel.Attribute.ToEntity);

                foreach (var rel in mapping.RelationshipsManyToOne)
                    rel.ToEntity = mappedEntities.GetValueOrDefault(rel.Attribute.ToEntity);

                foreach (var rel in mapping.RelationshipsManyToMany)
                    rel.ToEntity = mappedEntities.GetValueOrDefault(rel.Attribute.ToEntity);
            }

            return result;
        }

        public List<EntityMetadata> GetSelectedEntities()
        {
            string[] selection = settings.Entities.Keys.ToArray();

            var result = metadataCache.Where(e => selection.Contains(e.LogicalName)).ToList();


            if (!selection.Contains(ActivityParty))
                return result;

            if (!result.Any(e => e.IsActivity == true || e.IsActivityParty == true))
                return result;

            if (result.Any(e => e.LogicalName.Equals(ActivityParty)))
                return result;

            var activityparty = metadataCache.SingleOrDefault(r => r.LogicalName.Equals(ActivityParty));
            if (activityparty == null)
                return result;

            result.Add(activityparty);
            return result;
        }

        private static void FilterRelationshipsNotIncludedInMapping(MappingEntity[] mappedEntities)
        {
            List<string> entityNames = mappedEntities.Select(m => m.LogicalName).ToList();

            TRelation[] Filter<TRelation>(TRelation[] relations) where TRelation : MappingRelationship
                => relations.Where(r => entityNames.IndexOf(r.Type) != -1).ToArray();

            foreach (MappingEntity mapping in mappedEntities)
            {
                mapping.RelationshipsOneToMany = Filter(mapping.RelationshipsOneToMany);
                mapping.RelationshipsManyToOne = Filter(mapping.RelationshipsManyToOne);
                mapping.RelationshipsManyToMany = Filter(mapping.RelationshipsManyToMany);
            }
        }
    }
}
