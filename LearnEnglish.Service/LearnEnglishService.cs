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

        public async Task<NewWord> CreateNewWord(NewWordPostModel _newWord)
        {
            try
            {
                long wordsInDb = _collection.CountDocuments(new BsonDocument());
                NewWord newWord = new NewWord()
                {   
                    Id=Convert.ToString((wordsInDb + 1)),
                    Text = _newWord.Text,
                    Meaning=_newWord.Meaning,
                    Examples = _newWord.Examples,
                    Tips=_newWord.Tips
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
                        Text = "Initial",
                        Meaning="Some Meaning",
                        Examples = new string[] { "It is an initial commit", "Initial values are default" },
                        Tips= new string[] { "It is tip 1", "It is tip 2" },
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
    }
}
