using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LearnEnglish.DataModels.Models;

namespace LearnEnglish.Service.Interfaces
{
    public interface ILearnEnglishService
    {
        Task<IEnumerable<NewWord>> GetAllWords();

        //Task<NewWord> GetWord(string id);

        //// add new word document
        //Task AddWord(NewWord word);

        //// remove a single document / word
        //Task<bool> RemoveWord(string id);

        //// should be used with high cautious, only in relation with demo setup
        //Task<bool> RemoveAllWords();

        //// creates a sample index
        //Task<string> CreateIndex();
    }
}
