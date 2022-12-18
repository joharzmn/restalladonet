```csharp

            var metaProvider = new MetaDataProvider(cb);
            var entityDescriptor = new EntityDescriptor();
            entityDescriptor.Actions.Add(new DataAction() { Url = @"<[URL]>/services/data/v54.0/query/?q=Select FIELDS(ALL) From Account LIMIT 200", Operation = "Select" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "Id", Key = true, Path = "Id" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "IsDeleted", Key = false, Path = "IsDeleted" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "Name", Key = false, Path = "Name" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingStreet", Key = false, Path = "BillingStreet" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingCity", Key = false, Path = "BillingCity" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingState", Key = false, Path = "BillingState" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingPostalCode", Key = false, Path = "BillingPostalCode" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingCountry", Key = false, Path = "BillingCountry" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingLatitude", Key = false, Path = "BillingLatitude" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingLongitude", Key = false, Path = "BillingLongitude" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingGeocodeAccuracy", Key = false, Path = "BillingGeocodeAccuracy" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "BillingAddress", Key = false, Path = "BillingAddress" });
            entityDescriptor.Table.Fields.Add(new DataField() { DataType = DataTypes.String, Field = "ParentId", Key = false, Path = "ParentId" });
            entityDescriptor.Table.Input.Add(new DataInput() { Column = "Id" });
            entityDescriptor.RepeatElement = "$.records";
            entityDescriptor.AutoBuild = true;
            entityDescriptor.Table.TableName = "Account";
            entityDescriptor.Table.FilterColumns = "TableName";
            entityDescriptor.Table.Schema = "SalesForce";
            entityDescriptor.Table.Description = "This is to Get Table Columns From SalesForce";
            foreach (DataRow dr in dt.Rows)
            {
                var entityDescriptor = new EntityDescriptor();
                entityDescriptor.Actions.Add(new DataAction() { Url = $@"https://d2w000007nm8eeas-dev-ed.my.salesforce.com//services/data/v56.0/query/?q=Select FIELDS(ALL) From {dr["Name"]} LIMIT 200", Operation = "Select" });
                cmd.CommandText = $"Select * from sys_Columns where TableName='{dr["Name"]}'";
                var dtColumns = new DataTable();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    var name = reader.GetValue("Name");
                    entityDescriptor.Table.Fields.Add(new DataField()
                    {
                        DataType = DataTypes.String,
                        Field = name.ToString(),
                        Path = name.ToString()
                    });
                }
                entityDescriptor.AutoBuild = false;
                entityDescriptor.Table.TableName = dr["Name"].ToString();
                entityDescriptor.RepeatElement = "$.records";
                entityDescriptor.Table.Schema = "SalesForce";
                entityDescriptor.Table.Description = dr["Label"].ToString();
                metaProvider.GenerateEntityDescriptor(entityDescriptor);
            }
```
It generates XML files schemas to be used for designed entity like `Salesforce` in this example