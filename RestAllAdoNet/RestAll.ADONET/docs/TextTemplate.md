```csharp
            var modelTemplate = "ID: <Model.Id>" +
                                "CreatedDate: <Model.CreatedDate; format=\"dd.MM.yyyy HH:mm\">" +
                                "Name: <Model.Name>" +
                                "Price: <Model.Price; format=\"0.00\"$>" +
                                "Items:<Model.Items:{item |" +
                                " item <i> - <item.Key>  <if(item.Value)>enabled<else>-<endif>" +
                                "}>";
            var model = new
            {
                Id = 10,
                CreatedDate = DateTime.Now,
                Name = "Name1",
                Price = 123.45356,
                Items = new List<KeyValuePair<string, bool>> {
                    new KeyValuePair<string, bool>("Item1", false),
                    new KeyValuePair<string, bool>("Item2", true),
                    new KeyValuePair<string, bool>("Item3", false),
                    new KeyValuePair<string, bool>("Item4", false),
                    new KeyValuePair<string, bool>("Item5", true)
                }
            };
            var strinTemplateEnding = new StringTemplateEngine();
            var parsed = strinTemplateEnding.Parse(modelTemplate, model);

```

##### Output
```output
ID: 10CreatedDate: 03.12.2022 16:40Name: Name1Price: 123.45Items:item 1 - Item1  -item 2 - Item2  enableditem 3 - Item3  -item 4 - Item4  -item 5 - Item5  enabled

```