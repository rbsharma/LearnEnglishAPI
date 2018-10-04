using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using Newtonsoft.Json.Linq;

namespace LearnEnglish.DataModels.Models
{
    public class LoggingData
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public LoggingData(DateTime _date, string _requestBody, string _queryString, string _controllerName, string _actionName)
        {
            dateTime = _date;
            requestBody = _requestBody;
            queryString = _queryString;
            controllerName = _controllerName;
            actionName = _actionName;
        }
        public DateTime dateTime { get; set; }
        public string requestBody { get; set; }
        public string queryString { get; set; }
        public string controllerName { get; set; }
        public string actionName { get; set; }
    }
}
