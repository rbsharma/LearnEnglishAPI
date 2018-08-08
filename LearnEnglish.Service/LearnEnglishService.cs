using System;
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

                InitialSetup();
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
                        Id = "1",
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
                                Options = new AnswerChoice[]
                                {
                                    new AnswerChoice()
                                    {
                                        Id=1,Text="Coffee"
                                    },new AnswerChoice()
                                    {
                                        Id=2,Text="Start"
                                    },new AnswerChoice()
                                    {
                                        Id=3,Text="End"
                                    },new AnswerChoice()
                                    {
                                        Id=4,Text="Sleep"
                                    }
                                }
                            },new Question()
                            {
                                Id =2,
                                Text ="What is the synonym of Start?",
                                Key =4,
                                Options = new AnswerChoice[]
                                {
                                    new AnswerChoice()
                                    {
                                        Id=1,Text="Finish"
                                    },new AnswerChoice()
                                    {
                                        Id=2,Text="Complete"
                                    },new AnswerChoice()
                                    {
                                        Id=3,Text="Begin"
                                    },new AnswerChoice()
                                    {
                                        Id=4,Text="End"
                                    }
                                }
                            },new Question()
                            {
                                Id =3,
                                Text ="An antonym of start is ",
                                Key =3,
                                Options = new AnswerChoice[]
                                {
                                    new AnswerChoice()
                                    {
                                        Id=1,Text="Begin"
                                    },new AnswerChoice()
                                    {
                                        Id=2,Text="Origin"
                                    },new AnswerChoice()
                                    {
                                        Id=3,Text="Stop"
                                    },new AnswerChoice()
                                    {
                                        Id=4,Text="Kick-Start"
                                    }
                                }
                            },
                            new Question()
                            {
                                Id =4,
                                Text ="Fill in the blank. In order to learn a new language, one has to ______ conversing in it.",
                                Key =2,
                                Options = new AnswerChoice[]
                                {
                                    new AnswerChoice()
                                    {
                                        Id=1,Text="Prey"
                                    },new AnswerChoice()
                                    {
                                        Id=2,Text="Start"
                                    },new AnswerChoice()
                                    {
                                        Id=3,Text="Stop"
                                    },new AnswerChoice()
                                    {
                                        Id=4,Text="Teach"
                                    }
                                }
                            },
                            new Question()
                            {
                                Id =5,
                                Text ="Which of the choices can refer to starting a new activity?",
                                Key =2,
                                Options = new AnswerChoice[]
                                {
                                    new AnswerChoice()
                                    {
                                        Id=1,Text="Shanky has completed his homework."
                                    },new AnswerChoice()
                                    {
                                        Id=2,Text="Ruby is going to meet his boss today."
                                    },new AnswerChoice()
                                    {
                                        Id=3,Text="I just watched a movie."
                                    },new AnswerChoice()
                                    {
                                        Id=4,Text="Eating that pie was a delicious task!!"
                                    }
                                }
                            }
                        }
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
                    return result;
                }
                else
                {
                    //send two after last id sent;
                    ObjectId lastSentDocumentId = GetObjectId(_lastSentDocumentId);
                    var result = await _collection.AsQueryable<NewWord>()
                        .Where(c => c.InternalId > lastSentDocumentId)
                        .Take(2)
                        .ToListAsync();
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
                    Id = Convert.ToString((wordsInDb + 1)),
                    Text = _newWord.Text,
                    Meaning = _newWord.Meaning,
                    Examples = _newWord.Examples,
                    Tips = _newWord.Tips
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
                        Id = Convert.ToString((currentWordId)),
                        Text = word.Text,
                        Meaning = word.Meaning,
                        Examples = word.Examples,
                        Tips = word.Tips
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

    }
}
