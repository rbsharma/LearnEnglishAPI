using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using LearnEnglish.DataModels.Models;
using LearnEnglish.Service;
using MongoDB.Bson;
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
        [NonAction]
        [Route("destroy")]
        public HttpResponseMessage Destroy()
        {
            try
            {
                LearnEnglishService.DestroyDatabase();
                LearnEnglishService.Instance = null;
                LearnEnglishService.InsatanceCounter = 0;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IHttpActionResult> Get()
        {
            var words = await _learnEnglishService.GetAllWords();
            return Ok(words);
        }

        [HttpGet]
        [Route("dailyupdate")]
        public async Task<IHttpActionResult> GetDailyUpdate(string lastReceivedItemId = null, string lastReceivedTime = null)
        {
            if (String.IsNullOrEmpty(lastReceivedTime))
            {
                var words = await _learnEnglishService.GetDailyUpdate();
                return Ok(words);
            }

            //string lastReceivedTime = "2018-08-02T01:29:43.138Z";
            DateTime previousTime = DateTime.Parse(lastReceivedTime, System.Globalization.CultureInfo.InvariantCulture);
            DateTime currentTime = DateTime.Now;
            var timeDifference = (currentTime - previousTime).TotalSeconds;

            var timeRequiredForUpdate = int.Parse(ConfigurationManager.AppSettings["UpdateTimeInSeconds"]);

            if (timeDifference > timeRequiredForUpdate)
            {
                var words = await _learnEnglishService.GetDailyUpdate(lastReceivedItemId);
                return Ok(words);
            }
            else
            {
                return Ok("Today's updates already sent");
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> Create(NewWordPostModel newWord)
        {
            NewWord insertedWord = await _learnEnglishService.CreateNewWord(newWord);
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
