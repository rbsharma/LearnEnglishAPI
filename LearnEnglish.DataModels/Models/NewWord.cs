using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnEnglish.DataModels.Models
{
    public class NewWord
    {
        [BsonId]
        public ObjectId InternalId { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public string Meaning { get; set; }
        public string[] Examples { get; set; }
        public string[] Tips { get; set; }
        public Question[] Questions { get; set; }
        public Stats Stats { get; set; }
    }

    public class NewWordPostModel
    {
        public string Text { get; set; }
        public string Meaning { get; set; }
        public string[] Examples { get; set; }
        public string[] Tips { get; set; }
        public Question[] Questions { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        //public AnswerChoice[] Options { get; set; }
        public string[] Options { get; set; }
        public string Text { get; set; }
        public int Key { get; set; }

        //public bool IsMultipleChoice { get; set; }

    }

    public class AnswerChoice
    {
        public int Id { get; set; }
        public string Text { get; set; }

        //public bool IsCorrect { get; set; }
    }

    public class Stats
    {
        public int HitCount { get; set; }
    }

    public class StatsResult
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public int HitCount { get; set; }
    }

    public class StatsPostModel
    {
        public int? Id { get; set; }
        public string Text { get; set; }
    }
}
