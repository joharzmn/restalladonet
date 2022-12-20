using RESTAll.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Contracts;
using RESTAll.Data.Exceptions;
using RESTAll.Data.Extensions;
using RESTAll.Data.Models;
using TSQL.Tokens;
using StatementType = RESTAll.Data.Models.StatementType;
using RESTAll.Data.Providers;
#nullable disable
namespace RESTAll.Data.Utilities
{
    public class DataUtility
    {
        private RestAllConnectionStringBuilder _builder;
        private readonly SQLiteProvider _sqLiteProvider;
        private readonly DataProvider _DataProvider;
        private readonly MetaDataProvider _MetaDataProvider;
        private readonly IQueryParser _QueryParser;
        private readonly IAuthenticationClient _AuthenticationClient;
        private readonly ILogger<DataUtility> _logger;
        private BatchRequest _currentBatchRequest;
        public DataUtility(RestAllConnectionStringBuilder builder,
            DataProvider dataProvider,
            SQLiteProvider sqLiteProvider,
            MetaDataProvider metaDataProvider,
            IQueryParser queryParser,
            IAuthenticationClient authenticationClient,
            ILogger<DataUtility> logger)
        {
            _builder = builder;
            _sqLiteProvider = sqLiteProvider;
            _DataProvider = dataProvider;
            _MetaDataProvider = metaDataProvider;
            _QueryParser = queryParser;
            _AuthenticationClient = authenticationClient;
            _logger = logger;

        }
        public DataTable FetchData(string sql)
        {


            var tableName = _QueryParser.Parse(sql);
            if (tableName.Count == 0)
            {
                throw new RESTException("Table Not Found or sql is not well formed", "SQL");
            }
            var queryDescriptor = tableName.FirstOrDefault();

            var entity = _MetaDataProvider.GetEntityDescriptor(queryDescriptor);
            var action = entity.Actions.FirstOrDefault(x => x.Operation == StatementType.Select);
            if (action != null)
            {

                foreach (var dataInput in entity.Table.Input)
                {
                    //if (action.Url != null && action.Url.Contains(dataInput.Column))
                    //{
                    //    var filter =
                    //        queryDescriptor.Filters.FirstOrDefault(x => x.ColumnName.ToLower() == dataInput.Column.ToLower());
                    //    var old = "{" + $"{dataInput.Column}" + "}";
                    //    action.Url = action.Url.Replace(old, filter.Value.ToString().Replace("'", ""));
                    //    _DataProvider.SetFilterValue(filter.ColumnName, filter.Value.ToString().Replace("'", ""));
                    //}
                }

                var data = _DataProvider.GetAsync(action.Operation, queryDescriptor.TableName, queryDescriptor.Schema).Result;
                _sqLiteProvider.ParkData(data.Data);
                var dt = new DataTable();
                if (!string.IsNullOrEmpty(entity.ViewSql))
                {
                    dt = _sqLiteProvider.GetData(entity.ViewSql);
                }
                else
                {
                    dt = _sqLiteProvider.GetData(sql);
                }

                return dt;
            }
            else
            {
                throw new RESTException("No Action Defined in the schema", "SchemaDefinition");
            }
        }

        public int ExecuteQuery(string sql, DbParameterCollection parameters)
        {
            var queries = _QueryParser.Parse(sql);
            var bodyDic = new Dictionary<string, object>();
            foreach (var item in queries)
            {
                var entity = _MetaDataProvider.GetEntityDescriptor(item);
                if (item.StatementType == StatementType.Insert || item.StatementType == StatementType.Update)
                {
                    bodyDic.MapParameters(parameters, item.Parameters);
                    bodyDic.MapValues(item.Parameters);

                    var action = entity.Actions.FirstOrDefault(x => x.Operation == item.StatementType);
                    if (action == null)
                    {
                        throw new RESTException($"{item.StatementType} Action not defined for Entity `{entity.Table.TableName}`",
                            HttpStatusCode.FailedDependency);
                    }
                    if (action.FilterAsElement)
                    {
                        bodyDic.MapFilterAsElement(item.Filters, parameters);
                    }

                    bodyDic.ValidateRequiredColumns(action.RequiredColumns);
                    _DataProvider.PostDataAsync(action.Url, bodyDic, entity).Wait();
                }

                if (item.StatementType == StatementType.Delete)
                {
                    var action = entity.Actions.FirstOrDefault(x => x.Operation == item.StatementType);
                    if (action == null)
                    {
                        throw new RESTException(
                            $"{item.StatementType} Action not defined for Entity `{entity.Table.TableName}`",
                            HttpStatusCode.FailedDependency);
                    }

                    if (action.FilterAsElement)
                    {
                        bodyDic.MapFilterAsElement(item.Filters, parameters);
                        _DataProvider.PostDataAsync(action.Url, bodyDic, entity).Wait();
                    }
                }
            }
            return 0;
        }

        private string BatchRequestItems(string sql, StatementType operationType, string batchId, DbParameterCollection parameters, DataRow[] dataRows)
        {
            var queries = _QueryParser.Parse(sql);
            List<Dictionary<string, object>> bodyList = new List<Dictionary<string, object>>();

            foreach (var item in queries)
            {
                if (item.StatementType == StatementType.Insert)
                {
                    var entity = _MetaDataProvider.GetEntityDescriptor(item);
                    foreach (DataRow dr in dataRows)
                    {
                        var bodyDic = new Dictionary<string, object>();
                        if (parameters != null)
                        {
                            for (int i = 0; i <= parameters.Count - 1; i++)
                            {
                                var parameter = item.Parameters.FirstOrDefault(x =>
                                    x.Type == TSQLTokenType.Variable &&
                                    x.Identifier.ToLower() == parameters[i].ParameterName.ToLower());
                                if (!string.IsNullOrEmpty(parameters[i].SourceColumn))
                                {
                                    bodyDic.Add(parameter.DestinationColumn.Replace("_", "."), dr[parameters[i].SourceColumn]);
                                }

                            }
                        }

                        foreach (var parameterModel in item.Parameters.Where(x => x.Type != TSQLTokenType.Variable))
                        {
                            if (parameterModel.Type == TSQLTokenType.StringLiteral)
                            {
                                var value = parameterModel.Identifier.Remove(0, 1)
                                    .Remove(parameterModel.Identifier.Length - 2, 1);
                                bodyDic.Add(parameterModel.DestinationColumn.Replace("_", "."), value);
                            }
                            else
                            {
                                bodyDic.Add(parameterModel.DestinationColumn.Replace("_", "."), parameterModel.Identifier);
                            }

                        }

                        var normalizedBody = bodyDic.Unflatten();
                        var batch = _MetaDataProvider.GetBatches(batchId.ToString(), normalizedBody.ToString(),
                            entity.EntityElement, _AuthenticationClient.Token).FirstOrDefault(x => x.Operation == item.StatementType);
                        _currentBatchRequest ??= batch;
                        bodyList.Add(JsonConvert.DeserializeObject<Dictionary<string, object>>(batch.RequestFormat));
                    }


                    //var action = entity.Actions.FirstOrDefault(x => x.Operation.ToLower() == "insert");
                    //_DataProvider.PostDataAsync(action.Url, bodyDic, entity).Wait();
                }
            }

            var batchRequest = _MetaDataProvider.GetBatchRequest().FirstOrDefault(x => x.Operation == operationType);
            var response = new Dictionary<string, object> { { batchRequest.RootObject, bodyList } };
            return JsonConvert.SerializeObject(response);

        }

        internal List<BatchResult> ExecuteBatch(string sql, StatementType operationType, DataRow[] rows, int batchSize, DbParameterCollection parameterCollection)
        {
            var splits = rows.Split(batchSize);
            var responseDict = new List<BatchResult>();
            int i = 1;
            foreach (var split in splits)
            {
                var batchRequestItem = BatchRequestItems(sql, operationType, i.ToString(), parameterCollection, split.ToArray());
                var response = _DataProvider.ExecuteBatchAsync(batchRequestItem, operationType).Result;
                var parsed = ParseResponse(split.ToArray(), response);
                responseDict.AddRange(parsed);
                i++;
            }
            return responseDict;
        }

        private List<BatchResult> ParseResponse(DataRow[] rows, string response)
        {

            var responseObject = JObject.Parse(response);
            var errorMappings = responseObject.SelectTokens(_currentBatchRequest.ErrorMapping.RootElement);
            var rowDict = new List<BatchResult>();
            foreach (var item in errorMappings)
            {
                if (item is JArray)
                {
                    var itemsList = item.ToList();
                    foreach (var subItem in itemsList)
                    {

                        var errorJToken = subItem.SelectToken($"{_currentBatchRequest.ErrorMapping.ErrorElement}");
                        if (errorJToken != null)
                        {
                            var index = itemsList.IndexOf(subItem);

                            rowDict.Add(new BatchResult()
                            {
                                Exception = new RESTException(errorJToken.ToString(), HttpStatusCode.BadRequest),
                                RowIndex = index,
                                DataRow = rows[index],
                                BatchState = BatchState.Error,
                                RawResult = errorJToken.ToString()
                            });
                        }

                    }

                }

            }
            var successMappings = responseObject.SelectTokens(_currentBatchRequest.SuccessMapping.RootElement);
            foreach (var item in successMappings)
            {
                if (item is JArray)
                {
                    var itemsList = item.ToList();
                    foreach (var subItem in itemsList)
                    {

                        var successToken = subItem.SelectToken($"{_currentBatchRequest.SuccessMapping.SuccessElement}");
                        if (successToken != null)
                        {
                            var index = itemsList.IndexOf(subItem);

                            rowDict.Add(new BatchResult()
                            {
                                RowIndex = index,
                                DataRow = rows[index],
                                BatchState = BatchState.Success,
                                RawResult = successToken.ToString()
                            });
                        }

                    }

                }

            }

            return rowDict;
        }
    }
}
