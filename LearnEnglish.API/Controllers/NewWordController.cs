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
    /// <summary>
    /// Controller for word section. 
    /// </summary>
    [RoutePrefix("api/newword")]
    public class NewWordController : ApiController
    {
        private LearnEnglishService _learnEnglishService;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewWordController()
        {
            _learnEnglishService = LearnEnglishService.GetSingletonInstance();
        }

        /// <summary>
        /// Should be used in exception cases. This method drops database in non restore form.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns all words stored in database.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getall")]
        public async Task<IHttpActionResult> Get()
        {
            var words = await _learnEnglishService.GetAllWords();
            return Ok(words);
        }

        /// <summary>
        /// Returns new words based on user's usage history.
        /// Takes id of last word user received alongwith the timestamp of receiving last id.
        /// Sends new words if new timestamp has 24 hour difference.
        /// </summary>
        /// <param name="lastReceivedItemId"></param>
        /// <param name="lastReceivedTime"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Insert a new word in database.
        /// </summary>
        /// <param name="newWord"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> Create(NewWordPostModel newWord)
        {
            NewWord insertedWord = await _learnEnglishService.CreateNewWord(newWord);
            return Ok(insertedWord);
        }

        /// <summary>
        /// Insert multiple new words in database in one go.
        /// </summary>
        /// <param name="newWordList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("createbatch")]
        public async Task<IHttpActionResult> CreateBatch(List<NewWordPostModel> newWordList)
        {
            IEnumerable<NewWord> insertedWords = await _learnEnglishService.CreateNewWords(newWordList);
            return Ok(insertedWords);
        }

        /// <summary>
        /// Get Stats related to words present in database.
        /// 
        /// </summary>
        /// <param name="model">Optional</param>
        /// <returns></returns>
        [AcceptVerbs("Get", "Post")]
        [Route("stats")]
        public async Task<IHttpActionResult> GetStats(StatsPostModel model)
        {
            if (ModelState.IsValid)
            {
                var stats = await _learnEnglishService.GetStats(model);
                return Ok(stats);
            }
            else
            {
                return Ok("Today's updates already sent");
            }
        }
    }
}

