using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LearnEnglish.DataModels.Models;

namespace LearnEnglish.Service.Interfaces
{
    public interface ILearnEnglishService
    {
        /// <summary>
        /// Get All Words from database;
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<NewWord>> GetAllWords();

        /// <summary>
        /// add a single new word document
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        Task<NewWord> CreateNewWord(NewWordPostModel word);

        /// <summary>
        /// add multiple new word documents
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        Task<IEnumerable<NewWord>> CreateNewWords(List<NewWordPostModel> words);

        /// <summary>
        /// Get new word suggestions after 24 hours;
        /// </summary>
        /// <param name="lastid"></param>
        /// <returns></returns>
        Task<IEnumerable<NewWord>> GetDailyUpdate(string lastid);

        //// remove a single document / word
        //Task<bool> RemoveWord(string id);

        //// should be used with high cautious, only in relation with demo setup
        //Task<bool> RemoveAllWords();

        //// creates a sample index
        //Task<string> CreateIndex();
    }
}
