using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using LearnEnglish.DataModels.Models;
using LearnEnglish.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LearnEnglish.API.Controllers
{
    [RoutePrefix("api/newword")]
    public class NewWordController : ApiController
    {
        private LearnEnglishService _learnEnglishService;
        public NewWordController()
        {
            _learnEnglishService = LearnEnglishService.GetSingletonInstance();
        }

        [HttpGet]
        [Route("destroy")]
        public HttpResponseMessage Destroy()
        {
            try
            {
                LearnEnglishService.DestroyDatabase();
                LearnEnglishService.Instance = null;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var words = await _learnEnglishService.GetAllWords();
            return Ok(words);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> Create(NewWordPostModel newWord)
        {            
            NewWord insertedWord= await _learnEnglishService.CreateNewWord(newWord);
            return Ok(insertedWord);
        }

        [HttpPost]
        [Route("createbatch")]
        public async Task<IHttpActionResult> CreateBatch(List<NewWordPostModel> newWordList)
        {
            IEnumerable<NewWord> insertedWords = await _learnEnglishService.CreateNewWords(newWordList);
            return Ok(insertedWords);
        }
    }
}
