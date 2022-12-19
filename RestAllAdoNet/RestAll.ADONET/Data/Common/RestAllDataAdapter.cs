using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Exceptions;
using RESTAll.Data.Models;
using RESTAll.Data.Utilities;
using StatementType = System.Data.StatementType;

#nullable disable
namespace RESTAll.Data.Common
{
    [Designer("RestAll.DesignTime.DataAdapterDesigner, RestAll")]
    [ToolboxItem("RestAll.DesignTime.DataAdapterToolboxItem, RestAll")]
    [DefaultEvent("RowUpdated")]
    public class RestAllDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
    {
        private static readonly object _EventRowUpdated = new object();
        private static readonly object _EventRowUpdating = new object();

        private RestAllCommand _selectCommand;
        private RestAllCommand _insertCommand;
        private RestAllCommand _updateCommand;
        private RestAllCommand _deleteCommand;
        private ILogger<RestAllDataAdapter> _logger;
        private DataUtility _DataUtility;
        public RestAllDataAdapter()
        {
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllDataAdapter>>();
            _DataUtility = ServiceContainer.ServiceProvider.GetRequiredService<DataUtility>();
        }

        public RestAllDataAdapter(string commandText, RestAllConnection connection)
        {
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllDataAdapter>>();
            _DataUtility = ServiceContainer.ServiceProvider.GetRequiredService<DataUtility>();
        }

        public RestAllDataAdapter(RestAllDataAdapter adapter) : base(adapter)
        {
            _logger = ServiceContainer.ServiceProvider.GetRequiredService<ILogger<RestAllDataAdapter>>();
            _DataUtility = ServiceContainer.ServiceProvider.GetRequiredService<DataUtility>();
        }

        private IDbDataAdapter _IDbDataAdapter => (IDbDataAdapter)this;

        [DefaultValue(null)]
        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public new RestAllCommand SelectCommand
        {
            get => _selectCommand;
            set => _selectCommand = value;
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get => _selectCommand;
            set => _selectCommand = (RestAllCommand)value;
        }



        [DefaultValue(null)]
        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public new RestAllCommand InsertCommand
        {
            get => _insertCommand;
            set => _insertCommand = value;
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get => _insertCommand;
            set => _insertCommand = (RestAllCommand)value;
        }

        [DefaultValue(null)]
        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public new RestAllCommand UpdateCommand
        {
            get => _updateCommand;
            set => _updateCommand = value;
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get => _updateCommand;
            set => _updateCommand = (RestAllCommand)value;
        }

        [DefaultValue(null)]
        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public new RestAllCommand DeleteCommand
        {
            get => this._deleteCommand;
            set => this._deleteCommand = value;
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get => _deleteCommand;
            set => _deleteCommand = (RestAllCommand)value;
        }

        [DefaultValue(1)]
        public override int UpdateBatchSize { get; set; }

        protected override int Update(DataRow[] dataRows, DataTableMapping tableMapping)
        {
            if (dataRows.Any(x => x.RowState == DataRowState.Added) && InsertCommand == null)
            {
                throw new RESTException("Insert Command Not Specified or null", HttpStatusCode.ExpectationFailed);
            }
            else if (InsertCommand != null && string.IsNullOrEmpty(InsertCommand.CommandText))
            {
                throw new RESTException("Insert Command Text is Not supplied", HttpStatusCode.ExpectationFailed);
            }
            else if (dataRows.Length > 0 && InsertCommand != null)
            {
                var insertRows = dataRows.Where(x => x.RowState == DataRowState.Added);
                var response = _DataUtility.ExecuteBatch(this.InsertCommand.CommandText, Models.StatementType.Insert, dataRows, this.UpdateBatchSize,
                    this.InsertCommand.Parameters);
                foreach (var item in response)
                {
                    var rowUpdatedEvent =
                        new RestAllDataAdapterRowUpdatedEventArgs(item.DataRow, InsertCommand, StatementType.Insert, tableMapping)
                        {
                            RawResponse = item.RawResult
                        };

                    if (item.BatchState == BatchState.Error)
                    {
                        rowUpdatedEvent.Errors = item.Exception;
                        rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                    }

                    OnRowUpdated(rowUpdatedEvent);
                }
                return response.Count;
            }

            return 0;
        }



        protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping, object rawResponse)
        {
            return new RestAllDataAdapterRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new RestAllDataAdapterRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            RestAllDataAdapterRowUpdatingEventHandler handler = (RestAllDataAdapterRowUpdatingEventHandler)Events[_EventRowUpdating];
            if ((null != handler) && (value is RestAllDataAdapterRowUpdatingEventArgs args))
            {
                handler(this, args);
            }
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            RestAllDataAdapterRowUpdatedEventHandler handler = (RestAllDataAdapterRowUpdatedEventHandler)Events[_EventRowUpdated];
            if ((null != handler) && (value is RestAllDataAdapterRowUpdatedEventArgs args))
            {
                handler(this, args);
            }
        }

        public event RestAllDataAdapterRowUpdatingEventHandler RowUpdating
        {
            add => Events.AddHandler(_EventRowUpdating, value);
            remove => Events.RemoveHandler(_EventRowUpdating, value);
        }


        public event RestAllDataAdapterRowUpdatedEventHandler RowUpdated
        {
            add => Events.AddHandler(_EventRowUpdated, value);
            remove => Events.RemoveHandler(_EventRowUpdated, value);
        }

        object Clone()
        {
            return new RestAllDataAdapter(this);
        }


    }

    public delegate void RestAllDataAdapterRowUpdatingEventHandler(object sender, RestAllDataAdapterRowUpdatingEventArgs e);
    public delegate void RestAllDataAdapterRowUpdatedEventHandler(object sender, RestAllDataAdapterRowUpdatedEventArgs e);

    public class RestAllDataAdapterRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public RestAllDataAdapterRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping)
        {
        }

        // Hide the inherited implementation of the command property.
        public new RestAllCommand Command
        {
            get => (RestAllCommand)base.Command;
            set => base.Command = value;
        }

        public object RawRequest { set; get; }
    }

    public class RestAllDataAdapterRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public RestAllDataAdapterRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
            : base(row, command, statementType, tableMapping)
        {
        }

        // Hide the inherited implementation of the command property.
        public new RestAllCommand Command => (RestAllCommand)base.Command;
        public object RawResponse { set; get; }
    }
}
