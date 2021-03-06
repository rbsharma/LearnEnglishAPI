﻿using System;
using System.Collections.Generic;
using System.Text;
using LearnEnglish.Service.Interfaces;
using LearnEnglish.DataModels.Models;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;
using LearnEnglish.Service.Models;
using System.Diagnostics;
using MongoDB.Driver.Linq;

namespace LearnEnglish.Service
{
    public class LearnEnglishService : ILearnEnglishService
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        protected static IMongoCollection<NewWord> _collection;
        protected static IMongoCollection<LoggingData> _Logcollection;
        protected static Settings settings = new Settings();
        public static LearnEnglishService Instance = null;
        public static int InsatanceCounter = 0;
        public static LearnEnglishService GetSingletonInstance()
        {
            if (Instance == null)
            {
                Instance = new LearnEnglishService();
                Debug.WriteLine("New LearnEnglishService Instance Created, {0} Instance live", ++InsatanceCounter);
            }
            return Instance;
        }
        private LearnEnglishService()
        {
            try
            {
                settings.ConnectionString = Convert.ToString(ConfigurationManager.AppSettings["MongoConnectionString"]);
                settings.Database = Convert.ToString(ConfigurationManager.AppSettings["Database"]);
                settings.DailyUpdateWordCount = Convert.ToString(ConfigurationManager.AppSettings["DailyUpdateWordCount"]);

                _client = new MongoClient(settings.ConnectionString);
                _database = _client.GetDatabase(settings.Database);
                _collection = _database.GetCollection<NewWord>("NewWords");
                _Logcollection = _database.GetCollection<LoggingData>("Logs");

                //InitialSetup();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static void InitialSetup()
        {
            try
            {
                long documentCount = _collection.CountDocuments(new BsonDocument());
                if (documentCount <= 0)
                {
                    NewWord newWord1 = new NewWord
                    {
                        Id = 1,
                        Text = "Start",
                        Meaning = "The beginning of anything.",
                        Examples = new string[] { "Never too late for a fresh start.", "Every day is a beginning. Take a deep breath and start again." },
                        Tips = new string[] { "Derived from old english : styrtan" },
                        Questions = new Question[]
                        {
                            new Question()
                            {
                                Id =1,
                                Text ="Fill in the blank. Never too late for a fresh ______.",
                                Key =2,
                                Options = new string[]
                                {
                                    "Coffee","Start","End","Sleep"
                                }
                            },new Question()
                            {
                                Id =2,
                                Text ="What is the synonym of Start?",
                                Key =4,
                                Options = new string[]
                                {
                                    "Finish","Complete","Begin","End"
                                }
                            },new Question()
                            {
                                Id =3,
                                Text ="An antonym of start is ",
                                Key =3,
                                Options = new string[]
                                {"Begin","Origin","Stop","Kick-Start" }
                            },
                            new Question()
                            {
                                Id =4,
                                Text ="Fill in the blank. In order to learn a new language, one has to ______ conversing in it.",
                                Key =2,
                                Options = new string[]
                                { "Prey","Start","Stop","Teach" }
                            },
                            new Question()
                            {
                                Id =5,
                                Text ="Which of the choices can refer to starting a new activity?",
                                Key =2,
                                Options = new string[]
                                {
                                    "Shanky has completed his homework.",
                                    "Ruby is going to meet his boss today.",
                                    "I just watched a movie.",
                                    "Eating that pie was a delicious task!!"
                                }
                            }
                        },
                        Stats = new Stats { HitCount = 0 }
                    };
                    _collection.ReplaceOneAsync(
                        x => x.Id == newWord1.Id,
                        newWord1,
                        new UpdateOptions { IsUpsert = true });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DestroyDatabase()
        {
            try
            {
                _client.DropDatabase(settings.Database);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Get ObjectId;
        public ObjectId GetObjectId(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId internalId))
                internalId = ObjectId.Empty;
            return internalId;
        }

        public async Task<IEnumerable<NewWord>> GetAllWords()
        {
            try
            {
                var filter = new BsonDocument();
                var result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<IEnumerable<NewWord>> GetDailyUpdate(string _lastSentDocumentId = "")
        {
            try
            {
                int collectionReturnSize = int.Parse(settings.DailyUpdateWordCount);

                if (String.IsNullOrEmpty(_lastSentDocumentId))
                {
                    //send first two;
                    var result = _collection.AsQueryable<NewWord>().Take(2);
                    this.UpdateStats(result.ToList());
                    return result;
                }
                else
                {
                    //ObjectId abc = GetObjectId(_lastSentDocumentId);
                    //send two after last id sent;
                    //ObjectId lastSentDocumentId = GetObjectId(_lastSentDocumentId);
                    var id = Convert.ToInt32(_lastSentDocumentId);
                    var result = await _collection.AsQueryable<NewWord>()
                        .Where(c => c.Id > id)
                        .Take(2)
                        .ToListAsync();
                    this.UpdateStats(result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<NewWord> CreateNewWord(NewWordPostModel _newWord)
        {
            try
            {
                long wordsInDb = _collection.CountDocuments(new BsonDocument());
                NewWord newWord = new NewWord()
                {
                    Id = Convert.ToInt32(wordsInDb) + 1,
                    Text = _newWord.Text,
                    Meaning = _newWord.Meaning,
                    Examples = _newWord.Examples,
                    Tips = _newWord.Tips,
                    Questions = _newWord.Questions,
                    Stats = new Stats { HitCount = 0 }
                };
                await _collection.InsertOneAsync(newWord);
                return newWord;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<IEnumerable<NewWord>> CreateNewWords(List<NewWordPostModel> _newWords)
        {
            try
            {
                long wordsInDb = _collection.CountDocuments(new BsonDocument());
                long currentWordId = wordsInDb + 1;
                List<NewWord> newWordsToInsert = new List<NewWord>();
                foreach (NewWordPostModel word in _newWords)
                {
                    NewWord singleWord = new NewWord()
                    {
                        Id = Convert.ToInt32((currentWordId)),
                        Text = word.Text,
                        Meaning = word.Meaning,
                        Examples = word.Examples,
                        Tips = word.Tips,
                        Questions = word.Questions,
                        Stats = new Stats { HitCount = 0 }
                    };
                    newWordsToInsert.Add(singleWord);
                    currentWordId++;
                }

                await _collection.InsertManyAsync(newWordsToInsert);
                return newWordsToInsert;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool UpdateStats(List<NewWord> words)
        {
            try
            {
                bool UpdatesSuccessful = false;
                foreach (var word in words)
                {
                    word.Stats.HitCount++;
                    var item = _collection.ReplaceOne(x => x.Id == word.Id, word, new UpdateOptions { IsUpsert = true });
                    UpdatesSuccessful = true;
                }

                return UpdatesSuccessful;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<StatsResult>> GetStats(StatsPostModel model)
        {
            try
            {
                var allWords = new List<NewWord>();
                if (model != null && model.Id > 0)
                {
                    allWords = await _collection.AsQueryable<NewWord>().Where(x => x.Id == model.Id).ToListAsync();
                }
                if (allWords.Count <= 0 && model != null && !string.IsNullOrEmpty(model.Text))
                {
                    allWords = await _collection.AsQueryable<NewWord>().Where(x => x.Text == model.Text).ToListAsync();
                }
                if (allWords.Count <= 0)
                {
                    allWords = await _collection.Find(new BsonDocument()).ToListAsync();
                }
                //var allWords = await _collection.Find(filter).ToListAsync();
                IList<StatsResult> statsResult = new List<StatsResult>();
                foreach (var item in allWords)
                {
                    statsResult.Add(new StatsResult
                    {
                        WordId = item.Id,
                        Text = item.Text,
                        HitCount = item.Stats.HitCount
                    });
                }
                return statsResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<LoggingData> InsertLog(LoggingData newLog)
        {
            try
            {
                await _Logcollection.InsertOneAsync(newLog);
                return newLog;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<LoggingData>> GetAllLogs()
        {
            try
            {
                var filter = new BsonDocument();
                var result = await _Logcollection.Find(filter).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> ClearLogs()
        {
            try
            {
                await _database.DropCollectionAsync("Logs");
                _Logcollection = _database.GetCollection<LoggingData>("Logs");
                LoggingData newLog = new LoggingData(DateTime.Now, String.Empty, string.Empty, "NewWord", "ClearLogs");
                var loggedData = InsertLog(newLog);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}