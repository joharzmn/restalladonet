using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RESTAll.Data.Models
{
#nullable disable
    [Serializable]
    public class BatchRequest
    {
        [XmlAttribute("RootObject")]
        public string RootObject { set; get; }
        [XmlElement("RequestFormat")]
        public string RequestFormat { set; get; }
        [XmlAttribute("Endpoint")]
        public string Endpoint { set; get; }
        [XmlAttribute("Method")]
        public string Method { set; get; }
        [XmlAttribute("ContentType")]
        public string ContentType { set; get; }
        [XmlAttribute("ResponseMapping")]
        public string ResponseMapping { set; get; }
        [XmlElement("ErrorMapping")]
        public BatchErrorMap ErrorMapping { set; get; }
        [XmlElement("SuccessMapping")]
        public BatchSuccessMap SuccessMapping { set; get; }
    }

    [Serializable]
    public class BatchErrorMap
    {
        [XmlAttribute("RootElement")]
        public string RootElement { set; get; }
        [XmlAttribute("ErrorElement")]
        public string ErrorElement { set; get; }
    }

    [Serializable]
    public class BatchSuccessMap
    {
        [XmlAttribute("RootElement")]
        public string RootElement { set; get; }
        [XmlAttribute("SuccessElement")]
        public string SuccessElement { set; get; }
    }
}
