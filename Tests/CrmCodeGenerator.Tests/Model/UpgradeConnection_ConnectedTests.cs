using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CrmCodeGenerator.VSPackage;
using CrmCodeGenerator.VSPackage.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrmCodeGenerator.VSPackage.Xrm;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrmCodeGenerator.Tests
{

    [TestClass]
    public class UpgradeConnection_ConnectedTests : ConnectedTestsBase
    {
        //[TestInitialize] public override void TestInit() => base.TestInit();
        //[TestCleanup] public override void TestFinish() => base.TestFinish();

        [TestMethod]
        public void CheckConnection()
        {
            
        }

        /// <summary>
        /// Uses the global discovery service to return environment instances
        /// </summary>
        /// <param name="username">The user name</param>
        /// <param name="password">The password</param>
        /// <returns>A list of Instances</returns>
        static List<Instance> GetInstances(string username, string password)
        {

            string GlobalDiscoUrl = "https://globaldisco.crm.dynamics.com/";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GetAccessToken(username, password, 
                    new Uri("https://disco.crm.dynamics.com/api/discovery/")));
            client.Timeout = new TimeSpan(0, 2, 0);
            client.BaseAddress = new Uri(GlobalDiscoUrl);

            HttpResponseMessage response = 
                client.GetAsync("api/discovery/v2.0/Instances", HttpCompletionOption.ResponseHeadersRead).Result;

            if (response.IsSuccessStatusCode)
            {
                //Get the response content and parse it.
                string result = response.Content.ReadAsStringAsync().Result;
                JObject body = JObject.Parse(result);
                JArray values = (JArray)body.GetValue("value");

                if (!values.HasValues)
                {
                    return new List<Instance>();
                }

                return JsonConvert.DeserializeObject<List<Instance>>(values.ToString());
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}