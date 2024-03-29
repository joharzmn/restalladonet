﻿using System.Data.Common;
using System.Data;
using Microsoft.Extensions.Configuration;
//using RESTAll.Data.Common;
//using RESTAll.Data.Extensions;
//using RESTAll.Data.Utilities;
using SampleConsole;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Common;
using RESTAll.Data.Extensions;
using RESTAll.Data.Parser;

class Program
{
    static void Main(string[] args)
    {
        //Place your Secret Id from Project after configuring secrets for your project
        var configuration = new ConfigurationBuilder()
             .AddUserSecrets("c56d9a64-5dc7-4a47-bcaa-5c0600baea79").Build();
        var cb = new RestAllConnectionStringBuilder
        {
            AuthType = "OAuth2",
            Url = "https://sandbox-quickbooks.api.intuit.com",
            Profile = @"C:\Backups\Data",
            OAuthAccessTokenUrl = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer",
            OAuthSettingsLocation = @"C:\Backups\Data\QBO\authfile.txt",
            CallBackUrl = "http://localhost:5678",
            GrantType = "code",
            OAuthClientId = configuration.GetValue<string>("QBO:ClientId"),
            OAuthClientSecret = configuration.GetValue<string>("QBO:ClientSecret"),
            OAuthAuthorizationUrl = "https://appcenter.intuit.com/connect/oauth2",
            OAuthRefreshTokenUrl = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer",
            Scope = "com.intuit.quickbooks.accounting com.intuit.quickbooks.payment openid profile",
            CacheLocation = @"C:\Backups\Data\QBO\Cache",
            RequireCSRF = true,
            ReadUrlParameters = "realmId",
            RefreshTokenMethod = "clientcredentials",
            Schema = "QBO",
            LogLevel = "trace"
        };

        DbConnection restConnection = new RestAllConnection(cb);
        //var processed = ParseSql("Select a.* From Accounts as a Where Id=1");
        //var visitor = new StatementVisitor();
        //processed.sqlTree.Accept(visitor);

        //var queryParser = new QueryParser();
        //queryParser.Parse("Select * From Items as a Inner Join Accounts as c on c.Id=a.AccountId");

        restConnection.Open();
        //var connectionSchema = restConnection.GetSchema("Accounts");
        var cmd = restConnection.CreateCommand();
        //cmd.CommandText = "Insert Into Items (Name,ExpenseAccountRef_value) values(@Name,@ExpenseAccountRef_value)";
        //cmd.AddParameter("@Name","Test Item15");
        //cmd.AddParameter("@ExpenseAccountRef_value",30);
        cmd.CommandText = @"Select * From BillLines";
        //cmd.CommandText = @"Update Items Set Name='Hello World New Update',SyncToken=0, ExpenseAccountRef_value=30 where Id=20";
        var dt = new DataTable();
        //cmd.AddParameter("@id",20);
        //cmd.ExecuteNonQuery();
        dt.Load(cmd.ExecuteReader());
        Console.ReadLine();
        //dt.Columns.Add("Name");
        //dt.Columns.Add("ExpenseAccountRef_value");
        //for (int i = 0; i <= 5; i++)
        //{
        //    var row = dt.NewRow();
        //    row["Name"] = $"Item {i} Test";
        //    row["ExpenseAccountRef_value"] = 30;
        //    dt.Rows.Add(row);
        //    dt.AcceptChanges();
        //}

        //var nrow = dt.NewRow();
        //nrow["ExpenseAccountRef_value"] = 30;


        //nrow["Name"] = "Success Item 4";
        //dt.Rows.Add(nrow);
        //dt.AcceptChanges();
        //foreach (DataRow dataRow in dt.Rows)
        //{
        //    dataRow.SetAdded();
        //}


        ////Console.ReadLine();
        //var adapter = new RestAllDataAdapter();

        //cmd.CommandText = "Insert into Items(Name,ExpenseAccountRef_value) values (@Name,@ExpenseAccountRef_value)";
        //cmd.AddParameter("@Name", "Name");
        //cmd.AddParameter("@ExpenseAccountRef_value", "ExpenseAccountRef_value");
        //adapter.InsertCommand = (RestAllCommand)cmd;
        //adapter.UpdateBatchSize = 10;
        //adapter.RowUpdated += Adapter_RowUpdated;
        //cmd.UpdatedRowSource = UpdateRowSource.None;
        //adapter.Update(dt);

        Console.ReadLine();
        //foreach (DataRow dr in dt.Rows)
        //{

        //    var entityDescriptor = new EntityDescriptor();
        //    entityDescriptor.Actions.Add(new DataAction() { Url = $@"^[Connection.URL]^/services/data/v56.0/query/?q=Select FIELDS(ALL) From {dr["Name"]} LIMIT 200", Operation = "Select" });

        //    entityDescriptor.Table.Input.Add(new DataInput() { Column = "Id" });
        //    entityDescriptor.RepeatElement = "$.records";
        //    entityDescriptor.Table.Schema = "SalesForce";
        //    //entityDescriptor.AutoBuild = true;
        //    entityDescriptor.Table.TableName = $"{dr["Name"]}";
        //    cmd.CommandText = $"Select * From [Salesforce].TableColumns where TableName='{dr["Name"]}'";
        //    var dtColumns = new DataTable();
        //    adapter.Fill(dtColumns);
        //    foreach (DataRow dtColumnsRow in dtColumns.Rows)
        //    {
        //        entityDescriptor.Table.Fields.Add(new DataField()
        //        {
        //            DataType = metaProvider.GetType(dtColumnsRow["Type"].ToString()),
        //            Field = dtColumnsRow["Name"].ToString(),
        //            Path = dtColumnsRow["Name"].ToString(),
        //            Key = dtColumnsRow["Name"].ToString().ToLower() == "id"
        //        });
        //    }
        //    entityDescriptor.Table.Description = $"{dr["Label"]}";
        //    metaProvider.GenerateEntityDescriptor(entityDescriptor);
        //}



    }
    private static (TSqlFragment sqlTree, IList<ParseError> errors) ParseSql(string procText)
    {
        var parser = new TSql150Parser(true, SqlEngineType.All);
        using (var textReader = new StringReader(procText))
        {
            var sqlTree = parser.Parse(textReader, out var errors);

            return (sqlTree, errors);
        }
    }

    //private static void Adapter_RowUpdated(object sender, RestAllDataAdapterRowUpdatedEventArgs e)
    //{

    //}
}