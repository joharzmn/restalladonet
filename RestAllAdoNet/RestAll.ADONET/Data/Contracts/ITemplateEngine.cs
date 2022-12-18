namespace RESTAll.Data.Contracts;

public interface ITemplateEngine
{
    string Parse(string template, object cb, object input, object token);
    string PopulateFields(string template, IList<string> fileds);
}