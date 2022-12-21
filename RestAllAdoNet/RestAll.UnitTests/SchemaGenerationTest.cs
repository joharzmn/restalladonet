using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTAll.Data.Common;
using RESTAll.Data.Models;
using RESTAll.Data.Providers;
using StatementType = RESTAll.Data.Models.StatementType;

namespace RestAll.UnitTests
{
    [TestClass]
    public class SchemaGenerationTest
    {
        private IConfiguration _configuration;
        public SchemaGenerationTest()
        {

            _configuration = new ConfigurationBuilder()
                .AddUserSecrets("44646777-d312-49c1-ab49-082585d042c3").Build();

            //_apiKey = configuration.GetValue<string>("ApiKey");

        }
        [TestMethod]
        public void GenerateSchema()
        {
            var connectionStringBuilder = new RestAllConnectionStringBuilder
            {
                Profile = @"C:\RESTALL",
                Schema = "QBO",
                Url = "https://sandbox-quickbooks.api.intuit.com",//base URL of endpoint
            };
            Utility.GenerateEntity($@"{AppDomain.CurrentDomain.BaseDirectory}\Configs\EntityObj.json", "select * from RefundReceipt", "$.QueryResponse.RefundReceipt", "RefundReceipts", "^[Connection.URL]^/v3/company/^[Token.realmid]^/query?minorversion=65", "QuickBooks RefundReceipts", connectionStringBuilder);
        }

        [TestMethod]
        public void GenerateBatchRequest()
        {
            var builder = new RestAllConnectionStringBuilder
            {
                Profile = @"C:\RESTAll",
                Schema = "QBO"
            };
            var metaProvider = new MetaDataProvider(builder, null, null);
            var batchList = new List<BatchRequest>
            {
                new BatchRequest()
                {
                    Endpoint = "^[Connection.URL]^/v3/company/^[Token.realmid]^/batch",
                    RequestFormat = " {\r\n \"bId\":\"^[Input.BatchId]^\",\r\n \"^[Input.Entity]^\":^[Input.Data]^,\r\n \"operation\":\"create\"\r\n }",
                    RootObject = "BatchItemRequest",
                    SuccessMapping = new BatchSuccessMap()
                    {
                        RootElement = "$.BatchItemResponse",
                        SuccessElement = "^[Input.Entity]^"
                    },
                    ErrorMapping = new BatchErrorMap()
                    {
                        RootElement = "$.BatchItemResponse",
                        ErrorElement = "Fault"
                    },
                    Operation = StatementType.Insert
                },
                new BatchRequest()
                {
                Endpoint = "^[Connection.URL]^/v3/company/^[Token.realmid]^/batch",
                RequestFormat = " {\r\n \"bId\":\"^[Input.BatchId]^\",\r\n \"^[Input.Entity]^\":^[Input.Data]^,\r\n \"operation\":\"update\"\r\n }",
                RootObject = "BatchItemRequest",
                SuccessMapping = new BatchSuccessMap()
                {
                    RootElement = "$.BatchItemResponse",
                    SuccessElement = "^[Input.Entity]^"
                },
                ErrorMapping = new BatchErrorMap()
                {
                    RootElement = "$.BatchItemResponse",
                    ErrorElement = "Fault"
                },
                Operation = StatementType.Update
            }
            };
            metaProvider.GenerateBatchRequest(batchList);
            if (File.Exists($"{builder.Profile}/{builder.Schema}/Config/BatchRequest.xml"))
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Batch Request file not generated");
            }

        }

        [TestMethod]
        public void TestConnection()
        {
            var builder = new RestAllConnectionStringBuilder
            {
                AuthType = "OAuth2",
                Url = "https://sandbox-quickbooks.api.intuit.com",
                Profile = @"C:\RESTALL",
                OAuthAccessTokenUrl = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer",
                OAuthSettingsLocation = @"C:\RESTALL\QBO\Config\auth.json",
                CallBackUrl = "http://localhost:5678",
                GrantType = "code",
                OAuthClientId = _configuration.GetValue<string>("QBO:ClientId"),
                OAuthClientSecret = _configuration.GetValue<string>("QBO:ClientSecret"),
                OAuthAuthorizationUrl = "https://appcenter.intuit.com/connect/oauth2",
                OAuthRefreshTokenUrl = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer",
                Scope = "com.intuit.quickbooks.accounting com.intuit.quickbooks.payment openid profile",
                CacheLocation = @"C:\RESTALL",
                RequireCSRF = true,
                ReadUrlParameters = "realmId",
                RefreshTokenMethod = "clientcredentials",
                Schema = "QBO",
                LogLevel = "trace"
            };
            var restConnection = new RestAllConnection(builder);
            restConnection.Open();
            var cmd = restConnection.CreateCommand();
            cmd.CommandText = "Select * From [QBO].Payments";
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            restConnection.Close();
        }
    }
}
