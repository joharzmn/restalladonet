using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Common.EntitySql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTAll.Data.Contracts;
using RESTAll.Data.Extensions;
using RESTAll.Data.Providers;
using RESTAll.Data.Utilities;

namespace RESTAll.Data.Common
{
    public class RestAllConnection : DbConnection
    {
        private ConnectionState _State = ConnectionState.Closed;
        private MetaDataProvider _MetaDataProvider;
        public override string ConnectionString { get; set; }
        public override string Database { get; }
        public override ConnectionState State { get; }
        public override string DataSource { get; }
        public override string ServerVersion { get; }
        public int CommandTimeout { set; get; }
        private IAuthenticationClient _AuthenticationClient;
        private RestAllConnectionStringBuilder _Builder;
        private ILogger<RestAllConnection> _logger;
        public RestAllConnection()
        {
            _Builder = new RestAllConnectionStringBuilder(this.ConnectionString);
            ServiceContainer.Services.AddSingleton(_Builder);

            ServiceContainer.Services.AddSingleton<ITemplateEngine, StringTemplateEngine>();
            ServiceContainer.Services.AddSingleton<DataUtility>();
            ServiceContainer.Services.AddSingleton<IConfigProvider, ConfigProvider>();
            ServiceContainer.Services.AddSingleton<IQueryParser, QueryParser>();
            ServiceContainer.Services.AddSingleton<MetaDataProvider>();
            ServiceContainer.Services.AddSingleton<SQLiteProvider>();
            ServiceContainer.Services.AddSingleton<IAuthenticationClient, AuthenticationClient>();
            ServiceContainer.Services.AddSingleton<DataProvider>();
            ServiceContainer.Services.AddHttpClient("RestClient").AddTraceLogHandler();
            ServiceContainer.Services.AddSingleton(this);
            ServiceContainer.Services.AddLogging(config =>
            {
                config.SetMinimumLevel(_Builder.LogLevel.ToLogLevel());

                if (!string.IsNullOrEmpty(_Builder.LogFilePath))
                {
                    config.AddFileLogger(file =>
                    {
                        file.LogFilePath = _Builder.LogFilePath;
                        file.LogLevel=_Builder.LogLevel.ToLogLevel();

                    });
                }
                config.AddConsole();
            });

            ServiceContainer.Build();
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllConnection>>();
        }

        public RestAllConnection(string connectionString)
        {
            _Builder = new RestAllConnectionStringBuilder(connectionString);
            ServiceContainer.Services.AddSingleton(_Builder);
            ServiceContainer.Services.AddSingleton<ITemplateEngine, StringTemplateEngine>();
            ServiceContainer.Services.AddSingleton<DataUtility>();
            ServiceContainer.Services.AddSingleton<IQueryParser, QueryParser>();
            ServiceContainer.Services.AddSingleton<MetaDataProvider>();
            ServiceContainer.Services.AddSingleton<SQLiteProvider>();
            ServiceContainer.Services.AddSingleton<IAuthenticationClient, AuthenticationClient>();
            ServiceContainer.Services.AddSingleton<DataProvider>();
            ServiceContainer.Services.AddHttpClient("RestClient").AddTraceLogHandler();
            ServiceContainer.Services.AddSingleton(this);
            ServiceContainer.Services.AddLogging(config =>
            {
                config.SetMinimumLevel(_Builder.LogLevel.ToLogLevel());
                config.AddConsole();
                if (!string.IsNullOrEmpty(_Builder.LogFilePath))
                {
                    config.AddFileLogger(file =>
                    {
                        file.LogFilePath = _Builder.LogFilePath;
                        file.LogLevel=_Builder.LogLevel.ToLogLevel();

                    });
                }
            });
            ServiceContainer.Build();
            ConnectionString = connectionString;
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllConnection>>();
        }
        public RestAllConnection(DbConnectionStringBuilder builder)
        {
            ConnectionString = builder.ConnectionString;
            _Builder = new RestAllConnectionStringBuilder(builder.ConnectionString);
            ServiceContainer.Services.AddSingleton(_Builder);
            ServiceContainer.Services.AddSingleton<ITemplateEngine, StringTemplateEngine>();
            ServiceContainer.Services.AddSingleton<IConfigProvider, ConfigProvider>();
            ServiceContainer.Services.AddSingleton<DataUtility>();
            ServiceContainer.Services.AddSingleton<IQueryParser, QueryParser>();
            ServiceContainer.Services.AddSingleton<MetaDataProvider>();
            ServiceContainer.Services.AddSingleton<SQLiteProvider>();
            ServiceContainer.Services.AddSingleton<IAuthenticationClient, AuthenticationClient>();
            ServiceContainer.Services.AddSingleton<DataProvider>();
            ServiceContainer.Services.AddHttpClient("RestClient").AddTraceLogHandler();
            ServiceContainer.Services.AddLogging(config =>
            {
                config.SetMinimumLevel(_Builder.LogLevel.ToLogLevel());
                config.AddConsole();
                if (!string.IsNullOrEmpty(_Builder.LogFilePath))
                {
                    config.AddFileLogger(file =>
                    {
                        file.LogFilePath = _Builder.LogFilePath;
                        file.LogLevel = _Builder.LogLevel.ToLogLevel();

                    });
                }
               
            });
            
            ServiceContainer.Services.AddSingleton(this);

            ServiceContainer.Build();
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllConnection>>();
        }

        public override DataTable GetSchema(string collectionName)
        {

            if (_MetaDataProvider == null)
            {
                _logger.LogDebug("MetaDataProvider is Null");
                _MetaDataProvider = ServiceContainer.ServiceProvider.GetRequiredService<MetaDataProvider>();
            }

            if (collectionName.ToLower() == "tables")
            {
                _logger.LogDebug("Getting Tables Information");
                return _MetaDataProvider.GetSchemaInfo();
            }
            _logger.LogDebug($"Getting Schema information for:{collectionName}");
            return _MetaDataProvider.GetTableInfo(collectionName);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();

        }

        protected override DbProviderFactory? DbProviderFactory => new RestAllDbProviderFactory();

        public override void ChangeDatabase(string databaseName)
        {

        }

        public override void Close()
        {

        }

        public override void Open()
        {
            if (_AuthenticationClient == null)
            {
                _AuthenticationClient = ServiceContainer.ServiceProvider.GetRequiredService<IAuthenticationClient>();
            }

            if (_State == ConnectionState.Closed)
            {
                _logger.LogInformation("Opening Connection");
                ExecuteWithinStateTransition(
                    ConnectionState.Connecting,
                    f =>
                    {
                        _AuthenticationClient.GetAuthenticatedAsync().Wait();
                    },
                    ConnectionState.Open);
            }
            //else
            //{
            //    throw new InvalidOperationException("Connection can only be opened if it is currently in the closed state.");
            //}
        }


        protected override DbCommand CreateDbCommand()
        {
            var cmd = new RestAllCommand();
            cmd.Connection = this;
            cmd.CommandTimeout = CommandTimeout;
           
            return cmd;
        }

        private void ExecuteWithinStateTransition(ConnectionState stateWhileExecuting, Action<RestAllConnection> execute, ConnectionState transitionToStateOnSuccess)
        {
            // bool hadToChangeState = false;
            ConnectionState previousState = this._State;
            try
            {
                if (_State != stateWhileExecuting)
                {
                    //  hadToChangeState = true;
                    this._State = stateWhileExecuting;
                }
                execute(this);
                _State = transitionToStateOnSuccess;
                return;
            }
            catch (Exception)
            {
                _State = ConnectionState.Broken;
                throw;
            }

        }
    }
}
