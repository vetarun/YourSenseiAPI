using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class InitialAssessmentService : IInitialAssessmentService
    {
        private readonly YourSensei_DBEntities _context;

        public InitialAssessmentService(YourSensei_DBEntities context)
        {
            _context = context;

        }

        public async Task<List<GetInitialAssessmentResponseViewModel>> GetInitialAssessment()
        {
            try
            {
                List<GetInitialAssessmentResponseViewModel> lstQuestion = new List<GetInitialAssessmentResponseViewModel>();
                List<QuizAnswerAssessment> lstanswer = new List<QuizAnswerAssessment>();
                //lstanswer = _context.QuizAnswerAssessments.Where(x => x.UserDetailID == new Guid(userid)).ToList();
                lstQuestion = await (from question in _context.InitialAssessmentQuestions
                                     join category in _context.InitialAssessmentCategories on question.InitialAssessmentCategoryID equals category.ID
                                     select new GetInitialAssessmentResponseViewModel
                                     {
                                         catid = category.ID,
                                         catname = category.Name,
                                         questionid = question.ID,
                                         question = question.QuestionName,
                                         questionoptions = _context.InitialAssessmentOptions.Where(x => x.InitialAssessmentQuestionID == question.ID).ToList(),


                                     }).ToListAsync();

                return lstQuestion;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ResponseViewModel> SaveAssessmentAnswer(IEnumerable<AssessmentAnswerInputViewModel> input)
        {
            try
            {

                List<InitialAssessmentAnswer> quiz = new List<InitialAssessmentAnswer>();
                var date = DateTime.UtcNow;
                string userid = string.Empty;
                string compny = string.Empty;

                UserDetail userDetail = null;
                Employee employee = null;

                if (input.FirstOrDefault().userdetailid == null)
                {
                    userid = "00000000-0000-0000-0000-000000000000";
                    compny = "00000000-0000-0000-0000-000000000000";
                    var email = input.FirstOrDefault().email;
                    userDetail = _context.UserDetails.Where(a => a.UserName == email).FirstOrDefault();
                    if (userDetail != null)
                        userid = userDetail.ID.ToString();

                    employee = _context.Employees.Where(a => a.Email == email).FirstOrDefault();
                    if (employee != null)
                        compny = employee.CompanyId.ToString();
                }
                else
                {
                    userid = input.FirstOrDefault().userdetailid;
                    compny = input.FirstOrDefault().companyid;

                }

                int? sequenceNumber = _context.InitialAssessmentAnswers.Max<InitialAssessmentAnswer, int?>(a => a.SequenceNumber);
                sequenceNumber = sequenceNumber == null ? 1 : sequenceNumber + 1;
                foreach (var item in input)
                {
                    quiz.Add(new InitialAssessmentAnswer {
                        CompanyID = new Guid(compny),
                        FirstName = item.firstname,
                        LastName = item.lastname,
                        PhoneNumber = item.phone == null ? "" : item.phone,
                        Email = item.email,
                        InitialAssessmentQuestionID = item.questionid,
                        UserDetailID = new Guid(userid),
                        InitialAssessmentOptionID = item.optionid,
                        IsActive = true,
                        CreatedBy = new Guid(userid),
                        CreatedDate = date,
                        ModifiedBy = new Guid(userid),
                        ModifiedDate = date,
                        Score = item.score,
                        SequenceNumber = Convert.ToInt32(sequenceNumber)
                    });

                }
                _context.InitialAssessmentAnswers.AddRange(quiz);

                await _context.SaveChangesAsync();

                return new ResponseViewModel { Code = 200, Message = "Your Assessment info successfully saved!" };


            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<List<InitialAssessmentAnswerViewModel>> GetInitialAssessmentAnswer(int sequenceNumber, bool isActive)
        {
            try
            {
                List<InitialAssessmentAnswerViewModel> initialAssessmentAnswerViewModels = await _context.Database.SqlQuery<InitialAssessmentAnswerViewModel>(
                    "dbo.usp_GetInitialAssessmentAnswer @SequenceNumber = @sequenceNumber, @IsActive = @isActive",
                    new SqlParameter("sequenceNumber", sequenceNumber),
                    new SqlParameter("isActive", isActive)).ToListAsync();
                initialAssessmentAnswerViewModels = initialAssessmentAnswerViewModels.OrderByDescending(x => x.CreatedDate).ToList();
                return initialAssessmentAnswerViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
