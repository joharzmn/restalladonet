# RestAll Ado.NET
This Project aims to facilitate rest services in ADO.NET
## Scope
A user can write Sql Query in Command Line and Code will fetch data in tabulare format from any json or rest endpoint
It can add, update, delete or select data in Sql way.

## Current Stage
Code has been tested with QBO and Salesforce
### Quickbooks Online Scenarios
Code was tested to select, Insert and Batch Insert in Quickbooks online

### Salesforce
Code tested for Select Operations only

## Mile Stones
* Pagination
* Update
* Cross Resource entities integrations with Single Database store to any target
* for example we can write config for two Entities like Salesforce and Quickbooks online and Then Target Data Update or Insert into each other base on query like `Select Name Into [SF].Accounts From [QBO].Accounts`
* Documentation for Code Usage
