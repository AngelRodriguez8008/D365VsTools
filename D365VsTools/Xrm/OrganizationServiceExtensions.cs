using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace D365VsTools.Xrm
{
    public static class OrganizationServiceExtensions
    {
        public static Entity GetSolution(this IOrganizationService service, Guid solutionId)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("friendlyname", "uniquename", "publisherid")
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities.FirstOrDefault();
        }
        
        public static string GetSolutionVersion(this IOrganizationService service, Guid solutionId)
        {
            var solution = service.Retrieve("solution", solutionId, new ColumnSet("friendlyname", "version"));
            return solution.GetAttributeValue<string>("version");
        }
        
        public static void UpdateSolutionVersion(this IOrganizationService service, Guid solutionId, string version)
        {
            if (version == null)
                return;

            var solution = new Entity("solution", solutionId) {["version"] = version};
            service.Update(solution);
        }

        public static OrganizationDetail GetOrganization(this IDiscoveryService service, Guid organizationId)
        {
            var details = GetAllOrganizations(service);
            var result = details.FirstOrDefault(d => d.OrganizationId == organizationId);
            return result;
        }

        public static OrganizationDetail[] GetAllOrganizations(this IDiscoveryService service)
        {
            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse) service.Execute(request);

            var details = response.Details.ToArray();
            return details;
        }
        public static List<string> GetAllEntityNames(this IOrganizationService service, bool includeUnpublish = true)
        {
            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = includeUnpublish,
            };
            var response = (RetrieveAllEntitiesResponse)service.Execute(request);
            List<string> result = response.EntityMetadata.Select(e => e.LogicalName).ToList();
            return result;
        }

        public static EntityMetadata[] GetEntitiesMetadata(this IOrganizationService service, IEnumerable<string> entities, bool includeUnpublish = true)
        {
            var results = new List<EntityMetadata>();
            foreach (var entity in entities)
            {
                EntityMetadata entityMetadata = service.GetEntityMetadata(entity, includeUnpublish);
                if(entityMetadata!= null)
                    results.Add(entityMetadata);
            }
            return results.ToArray();
        }

        public static EntityMetadata GetEntityMetadata(this IOrganizationService service, string entity, bool includeUnpublish = true)
        {
            try
            {
                var req = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.All,
                    LogicalName = entity,
                    RetrieveAsIfPublished = includeUnpublish
                };
                var res = (RetrieveEntityResponse)service.Execute(req);
                var entityMetadata = res.EntityMetadata;
                return entityMetadata;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static EntityMetadata[] GetAllEntitiesMetadata(this IOrganizationService service, bool includeUnpublish = true)
        {
            OrganizationRequest request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.All,
                RetrieveAsIfPublished = true
            };
            var results = (RetrieveAllEntitiesResponse)service.Execute(request);
            var entities = results.EntityMetadata;
            return entities;
        }

        public static bool CheckConnection(this IOrganizationService service)
        {
            var success = service.WhoAmI().IsNullOrEmpty() == false;
            return success;
        }

        public static Guid? WhoAmI(this IOrganizationService service)
        {
            if (service == null)
                return null;

            var request = new WhoAmIRequest();
            var response = (WhoAmIResponse)service.Execute(request);

            var result = response?.UserId;
            return result;
        }
    }
}
