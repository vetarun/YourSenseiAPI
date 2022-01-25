using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.ViewModel;

namespace YourSensei.Controllers
{
    [Authorize]
    [RoutePrefix("Quiz")]
    public class QuizController : ApiController
    {
        private readonly IQuizService _service;

        public QuizController(IQuizService service)
        {
            _service = service;
        }
        [HttpGet]
        [Route("GetQuestionListByBookID")]
        public async Task<IHttpActionResult> GetQuestionListByBookID(string bookId, string userID)
        {
            var result = await _service.GetQuestionListByBookID(bookId, userID);
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveQuizAnswerAssessment")]
        public async Task<IHttpActionResult> SaveQuizAnswerAssessment(IEnumerable<QuizAnswerAssessmentInputViewModel> input, decimal score)
        {
            var result = await _service.SaveQuizAnswerAssessment(input,score);
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveQuiz")]
        public async Task<IHttpActionResult> SaveQuiz(QuizViewModel quizViewModel)
        {
            var result = await _service.SaveQuiz(quizViewModel);
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveQuestion")]
        public async Task<IHttpActionResult> SaveQuestion(QuestionViewModel questionViewModel)
        {
            var result = await _service.SaveQuestion(questionViewModel);
            return Ok(result);
        }
       
        [HttpGet]
        [Route("GetQuizList")]
        public async Task<IHttpActionResult> GetQuizList(string companyID, string userID, bool isIndividual)
        {
            var result = await _service.GetQuizList(companyID,userID,isIndividual);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetQuestionDetails")]
        public async Task<IHttpActionResult> GetQuestionDetails(int quesID)
        {
            var result = await _service.GetQuestionDetails(quesID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetQuizDetails")]
        public async Task<IHttpActionResult> GetQuizDetails(int quizID, string companyID, string userID, bool isIndividual)
        {
            var result = await _service.GetQuizDetails(quizID,companyID,userID,isIndividual);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteQuiz")]
        public async Task<IHttpActionResult> DeleteQuiz(int quizID)
        {
            var result = await _service.DeleteQuiz(quizID);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteQuestion")]
        public async Task<IHttpActionResult> DeleteQuestion(int quesID)
        {
            var result = await _service.DeleteQuestion(quesID);
            return Ok(result);
        }

        [HttpPost]
        [Route("ChangePublishedSetting")]
        public async Task<IHttpActionResult> ChangePublishedSetting(int quizID, bool IsPublished)
        {
            var result = await _service.ChangePublishedSetting(quizID,IsPublished);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetQuizByMentorIDAndEmployeeID")]
        public async Task<IHttpActionResult> GetQuizByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            var result = await _service.GetQuizByMentorIDAndEmployeeID(mentorID, isActive, employeeID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetQuizAnswerAssessmentByQuizIDAndEmployeeID")]
        public async Task<IHttpActionResult> GetQuizAnswerAssessmentByQuizIDAndEmployeeID(int quizID, bool isActive, string employeeID)
        {
            var result = await _service.GetQuizAnswerAssessmentByQuizIDAndEmployeeID(quizID, isActive, employeeID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetIncompleteQuiz")]
        public async Task<IHttpActionResult> GetIncompleteQuiz()
        {
            var result = await _service.GetIncompleteQuiz();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetIncompleteQuizByUserIDandCompID")]
        public async Task<IHttpActionResult> GetIncompleteQuizByUserIDandCompID(string userID, string CompanyID)
        {
            var result = await _service.GetIncompleteQuizByUserIDandCompID(userID, CompanyID);
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveQuizFinishStatus")]
        public async Task<IHttpActionResult> SaveQuizFinishStatus (int quizStatusID)
        {
            var result = await _service.SaveQuizFinishStatus(quizStatusID);
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveQuizStartStatus")]
        public async Task<IHttpActionResult> SaveQuizStartStatus(QuizStatusModel quizStartStatus)
        {
            var result = await _service.SaveQuizStartStatus(quizStartStatus);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteQuizStatus")]
        public async Task<IHttpActionResult> DeleteQuizStatus(int quizStatusID)
        {
            var result = await _service.DeleteQuizStatus(quizStatusID);
            return Ok(result);
        }
    }
}
