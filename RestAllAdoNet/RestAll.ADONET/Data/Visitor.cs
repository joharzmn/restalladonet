using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Models;
using RESTAll.Data.Utilities;
using RESTAll.Parser;
using RESTAll.Parser.Extensions;
using TSQL;
using TSQL.Clauses;
using TSQL.Elements;
using TSQL.Expressions;
using TSQL.Statements;
using TSQL.Tokens;
using StatementType = RESTAll.Data.Models.StatementType;
#nullable disable
namespace RESTAll.Data
{
    public class Visitor
    {
        public List<QueryDescriptor> queryDescriptor = new();
        private QueryDescriptor _currentQuery = new();
        private ColumnDescriptor _currentColumn = new();
        private FilterDescriptor _currentFilter = new();
        private List<ParameterModel> _parameters = new();
        public Visitor()
        {

        }
        public QueryDescriptor VisitStatement(TSQLStatement statement)
        {
            if (statement.Type == TSQLStatementType.Select)
            {
                VisitSelect(statement.AsSelect);

                return _currentQuery;
            }

            if (statement.Type == TSQLStatementType.Insert)
            {
                VisitInsert(statement.AsInsert);
                _currentQuery.StatementType = StatementType.Insert;
                if (statement.AsInsert.Select != null)
                {
                    VisitSelect(statement.AsInsert.Select);
                    _currentQuery.StatementType = StatementType.InsertWithSelect;
                }
            }

            if (statement.Type == TSQLStatementType.Update)
            {
                _currentQuery.StatementType = StatementType.Update;
                VisitUpdateStatement(statement.AsUpdate);
            }

            return _currentQuery;
        }

        private void VisitUpdate(TSQLUpdateClause updateClause)
        {
            var tableName = updateClause.Tokens.FirstOrDefault(x => x.Type == TSQLTokenType.Identifier);
            _currentQuery.TargetTable = tableName.Text;
        }

        private void VisitUpdateStatement(TSQLUpdateStatement updateStatement)
        {
            VisitUpdate(updateStatement.Update);
            VisitSet(updateStatement.Set);
            VisitWhere(updateStatement.Where);
            _currentQuery.Parameters = _parameters;
        }

        private void VisitSet(TSQLSetClause setClause)
        {
            var filtered = setClause.Tokens.Where(x => !x.IsKeyword(TSQLKeywords.SET) && x.Type != TSQLTokenType.Operator && !x.IsCharacter(TSQLCharacters.Comma));
            var parameter = new ParameterModel();
            foreach (var tsqlToken in filtered)
            {
                if (tsqlToken.Type == TSQLTokenType.Identifier)
                {
                    parameter.DestinationColumn = tsqlToken.Text;
                }
                else
                {
                    parameter.Identifier = tsqlToken.Text;
                    parameter.Type = tsqlToken.Type;
                    _parameters.Add(parameter);
                    parameter = new ParameterModel();
                }
            }
        }

        public void VisitInsert(TSQLInsertStatement insertStatement)
        {
            var columnNames = ExtractInsertColumns(insertStatement.Insert);
            _currentQuery.TargetTable = GetTableName(insertStatement.Insert.Tokens);
            if (insertStatement.Values == null)
            {
                return;
            }
            _parameters = ExtractValuesFromInsert(insertStatement.Values, columnNames);
            _currentQuery.Parameters = _parameters;

        }

        public string GetTableName(List<TSQLToken> tokens)
        {
            var startToken = tokens.FirstOrDefault(x => x.IsKeyword(TSQLKeywords.INTO));
            var lastToken = tokens.FirstOrDefault(x => x.IsCharacter(TSQLCharacters.OpenParentheses));
            var filterdTokens = tokens.GetBetween(startToken, lastToken).Select(x => x.Text);
            return string.Join("", filterdTokens);
        }

        public List<TSQLToken> ExtractInsertColumns(TSQLInsertClause insertClause)
        {
            var firstToken = insertClause.Tokens.FirstOrDefault(x => x.IsCharacter(TSQLCharacters.OpenParentheses));
            var lastToken = insertClause.Tokens.LastOrDefault(x => x.IsCharacter(TSQLCharacters.CloseParentheses));
            return insertClause.Tokens.GetBetween(firstToken, lastToken, TSQLCharacters.Comma);

        }
        public List<ParameterModel> ExtractValuesFromInsert(TSQLValues values, List<TSQLToken> columnTokens)
        {

            var parameterModel = new List<ParameterModel>();
            var firstToken = values.Tokens.FirstOrDefault(x => x.IsCharacter(TSQLCharacters.OpenParentheses));
            var lastToken = values.Tokens.LastOrDefault(x => x.IsCharacter(TSQLCharacters.CloseParentheses));
            var tokens = values.Tokens.GetBetween(firstToken, lastToken, TSQLCharacters.Comma);
            if (columnTokens.Count != tokens.Count)
            {
                throw new ArgumentOutOfRangeException("Parameters and Columns count mismatch");
            }
            int i = 0;
            foreach (var tsqlToken in tokens)
            {
                parameterModel.Add(new ParameterModel()
                {
                    Identifier = tsqlToken.Text,
                    Type = tsqlToken.Type,
                    DestinationColumn = columnTokens[i].Text
                });
                i++;
            }

            return parameterModel;
        }

        public void VisitSelect(TSQLSelectStatement select)
        {
            VisitColumns(select.Select);
            VisitFromClause(select.From);
            VisitWhere(select.Where);
        }

        public void VisitLimit(TSQLKeyword keyword)
        {

        }

        public void VisitColumns(TSQLSelectClause selectClause)
        {
            foreach (var selectClauseColumn in selectClause.Columns)
            {
                VisitColumn(selectClauseColumn);
                _currentQuery.Columns.Add(_currentColumn);
                _currentColumn = new ColumnDescriptor();
            }
        }

        public void VisitColumn(TSQLSelectColumn selectColumn)
        {
            switch (selectColumn.Expression.Type)
            {
                case TSQLExpressionType.Multicolumn:
                    _currentColumn.Name = "ALL";
                    break;
                case TSQLExpressionType.Column:
                    _currentColumn.Name = selectColumn.Expression.AsColumn.Column.Name;
                    break;
                default: break;

            }
            //if (selectColumn.Expression.Type == TSQLExpressionType.Multicolumn)
            //{
            //    _currentColumn.Name = "All";
            //}
            //else if (selectColumn.Expression.Type == TSQLExpressionType.Function)
            //{

            //}
            //else
            //{
            //    _currentColumn.Name = selectColumn.Expression.AsColumn.Column.Name;
            //    VisitAlias(selectColumn.ColumnAlias);
            //}
        }

        public void VisitAlias(TSQLIdentifier alias)
        {
            _currentColumn.Alias = alias?.Name;
        }

        public void VisitWhere(TSQLWhereClause whereClause)
        {

            //var _Tokenizer = new TSQLTokenizer(text);
            //TSQLTokenParserHelper.ReadUntilStop(_Tokenizer,ele, new List<TSQLFutureKeywords>(),new List<TSQLKeywords>()
            //{
            //    TSQLKeywords.AND,
            //    TSQLKeywords.OR
            //},false);
            if (whereClause != null)
            {
                var splittedClause = SplitClause(whereClause);
                foreach (var item in splittedClause)
                {
                    var firstToken = item.Value.FirstOrDefault();
                    while (firstToken != null)
                    {
                        firstToken = item.Value.NextToken(firstToken);
                        if (firstToken != null)
                        {
                            SetFilter(firstToken);
                            if (firstToken.Text == "(")
                            {
                                var text = item.Value.ToStringStatement(item.Value.FirstOrDefault(x => x.IsKeyword(TSQLKeywords.SELECT)), item.Value.LastOrDefault(x => x.IsCharacter(TSQLCharacters.CloseParentheses)));

                                if (!string.IsNullOrEmpty(text))
                                {
                                    var statment = TSQLStatementReader.ParseStatements(text).FirstOrDefault();
                                    if (statment is TSQLSelectStatement)
                                    {
                                        var vistor = new Visitor();
                                        var descriptor = vistor.VisitStatement(statment);
                                        queryDescriptor.Add(descriptor);
                                        firstToken = item.Value.FirstOrDefault(x =>
                                            x.IsCharacter(TSQLCharacters.CloseParentheses));
                                        firstToken = item.Value.NextToken(firstToken);
                                    }
                                }
                                else
                                {
                                    firstToken = item.Value.NextToken(firstToken);
                                }
                                //_currentFilter.Value = ReadSubQuery(whereClause.Tokens, ref firstToken);
                                //_currentQuery.Filters.Add(_currentFilter);
                                //_currentFilter = new();

                            }
                        }
                    }
                }
            }

        }

        public Dictionary<int, List<TSQLToken>> SplitClause(TSQLWhereClause whereClause)
        {
            var dic = new Dictionary<int, List<TSQLToken>>();
            var tokenList = new List<TSQLToken>();
            int counter = 1;
            foreach (var toke in whereClause.Tokens)
            {
                if (toke.IsKeyword(TSQLKeywords.AND) || toke.IsKeyword(TSQLKeywords.OR))
                {
                    dic.Add(counter, tokenList);
                    counter++;
                    tokenList = new();
                }
                else
                {
                    tokenList.Add(toke);
                }

            }
            dic.Add(counter, tokenList);
            return dic;
        }
        private void SetFilter(TSQLToken startToken)
        {
            switch (startToken.Type)
            {
                case TSQLTokenType.Identifier:
                    _currentFilter.ColumnName = startToken.Text;
                    break;
                case TSQLTokenType.Operator:
                    _currentFilter.Operator = startToken.Text;
                    break;
                case TSQLTokenType.Character:
                    _currentFilter.FilterType = QueryFilterType.SubQuery;
                    break;
                case TSQLTokenType.Variable:
                    _currentFilter.FilterType = QueryFilterType.Parameter;
                    _currentFilter.Value = startToken.Text;
                    _currentQuery.Filters.Add(_currentFilter);
                    _currentFilter = new();
                    break;
                case TSQLTokenType.StringLiteral:
                    _currentFilter.Value = startToken.Text;
                    _currentQuery.Filters.Add(_currentFilter);
                    _currentFilter = new();
                    break;
                case TSQLTokenType.NumericLiteral:
                    _currentFilter.Value = startToken.Text;
                    _currentQuery.Filters.Add(_currentFilter);
                    _currentFilter = new();
                    break;

                default: break;
            }
        }


        public void VisitFromClause(TSQLFromClause fromClause)
        {
            if (fromClause != null)
            {
                if (fromClause.Tokens.Exists(x => x.IsCharacter(TSQLCharacters.Period)))
                {
                    _currentQuery.Schema = fromClause.Tokens.FirstOrDefault(x => x.Type == TSQLTokenType.Identifier).Text;

                    _currentQuery.TableName = fromClause.Tokens.LastOrDefault(x => x.Type == TSQLTokenType.Identifier).Text;
                }
                else
                {
                    _currentQuery.TableName = fromClause.Tokens.FirstOrDefault(x => x.Type == TSQLTokenType.Identifier).Text;
                }

            }
        }
    }
}
