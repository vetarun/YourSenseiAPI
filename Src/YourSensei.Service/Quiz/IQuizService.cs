using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IQuizService
    {
        Task<List<GetQuestionListResponseViewModel>> GetQuestionListByBookID(string bookid, string userid);
        Task<ResponseViewModel> SaveQuizAnswerAssessment(IEnumerable<QuizAnswerAssessmentInputViewModel> input,decimal score);
        Task<QuizResponse> SaveQuiz(QuizViewModel quizViewModel);
        Task<ResponseViewModel> SaveQuestion(QuestionViewModel questionViewModel);
        Task<List<GetQuizLIstResponseViewModel>> GetQuizList(string companyID, string userID, bool isIndividual);
        Task<QuestionViewModel> GetQuestionDetails(int quesID);
        Task<QuizViewModel> GetQuizDetails(int quizID, string companyID, string userID, bool isIndividual);
        Task<ResponseViewModel> DeleteQuiz(int quizID);
        Task<ResponseViewModel> DeleteQuestion(int quesID);
        Task<ResponseViewModel> ChangePublishedSetting(int quizID, bool IsPublished);
        Task<List<QuizAnswerAssessmentInputViewModel>> GetQuizByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID);
        Task<List<QuizAnswerAssessmentInputViewModel>> GetQuizAnswerAssessmentByQuizIDAndEmployeeID(int quizID, bool isActive, string employeeID);
        Task<List<QuizStatusModel>> GetIncompleteQuiz();
        Task<List<QuizStatusModel>> GetIncompleteQuizByUserIDandCompID(string userID, string CompanyID);
        Task<ResponseViewModel> SaveQuizFinishStatus(int quizStatusID);
        Task<int> SaveQuizStartStatus(QuizStatusModel quizStartStatus);
        Task<ResponseViewModel> DeleteQuizStatus(int quizStatusID);
    }
}
