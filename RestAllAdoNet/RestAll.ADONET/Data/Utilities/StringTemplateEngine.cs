using Antlr4.StringTemplate;
using RESTAll.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Utilities
{

    public class StringTemplateEngine : ITemplateEngine
    {
        public string Parse(string template,object cb,dynamic input,object token)
        {
            var group = new TemplateGroupString("group", "delimiters \"^\", \"^\"");
            //var group = new TemplateGroupString("group");
            var renderer = new AdvancedRenderer();
            group.RegisterRenderer(typeof(DateTime), renderer);
            group.RegisterRenderer(typeof(double), renderer);
            
            group.DefineTemplate("template", template, new[] {"Connection", "Input","Token" });

            var stringTemplate = group.GetInstanceOf("template");
            stringTemplate.Add("Connection", cb);
            stringTemplate.Add("Input", input);
            stringTemplate.Add("Token", token);
            return stringTemplate.Render();
        }

        public string PopulateFields(string template, IList<string> fileds)
        {
            var group = new TemplateGroupString("group", "delimiters \"$\", \"$\"");
            //var group = new TemplateGroupString("group");
            
            group.DefineTemplate("template", template);

            var stringTemplate = group.GetInstanceOf("template");
            stringTemplate.Add("Fields", new { Fields= string.Join(",", fileds) });
            var result= stringTemplate.Render();
            return result;
        }
    }
}
