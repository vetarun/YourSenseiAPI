using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;


namespace YourSensei.Service
{
    public class QuizService : IQuizService
    {
        private static string AngularUrl = ConfigurationManager.AppSettings["AngularUrl"].ToString();
        private readonly YourSensei_DBEntities _context;
        private readonly ILibraryService _libraryService;
        private readonly IEmailWorkQueueService _emailWorkQueueService;
        public QuizService(YourSensei_DBEntities context, LibraryService libraryService, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _libraryService = libraryService;
            _emailWorkQueueService = emailWorkQueueService;
        }

        public async Task<List<GetQuestionListResponseViewModel>> GetQuestionListByBookID(string bookid, string userid)
        {
            try
            {
                List<GetQuestionListResponseViewModel> lstQuestion = new List<GetQuestionListResponseViewModel>();
                List<QuizAnswerAssessment> lstanswer = new List<QuizAnswerAssessment>();
                lstanswer = _context.QuizAnswerAssessments.Where(x => x.UserDetailID == new Guid(userid)).ToList();
                lstQuestion = await (from question in _context.Questions
                                     where question.IsActive == true
                                     join quiz in _context.Quizs on question.QuizID equals quiz.ID
                                     where quiz.IsPublished == true && quiz.IsActive == true
                                     join book in _context.CompanyLibraryBooks on quiz.ID equals book.QuizID
                                     where book.ID == new Guid(bookid)
                                     select new GetQuestionListResponseViewModel
                                     {
                                         quizid = question.QuizID,
                                         questionid = question.ID,
                                         question = question.QuestionName,
                                         questiontype = question.QuestionType,
                                         defaultans = question.CorrectAnswer,
                                         questionoptions = _context.QuestionOptions.Where(x => x.QuestionID == question.ID).ToList(),
                                         instruction = quiz.Instructions

                                     }).ToListAsync();
                for (int i = 0; i < lstQuestion.Count; i++)
                {
                    for (int j = 0; j < lstanswer.Count; j++)
                    {
                        if (lstQuestion[i].questionid == lstanswer[j].QuestionID)
                        {
                            lstQuestion[i].useranswer = lstanswer[j].UserAnswer;
                        }

                    }
                }
                return lstQuestion;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ResponseViewModel> SaveQuizAnswerAssessment(IEnumerable<QuizAnswerAssessmentInputViewModel> input, decimal score)
        {
            try
            {
                Data.CreditLog objcredit = new Data.CreditLog();
                List<QuizAnswerAssessment> quiz = new List<QuizAnswerAssessment>();
                var date = DateTime.UtcNow;
                string FirstName = string.Empty;
                string LastName = string.Empty;
                string CompanyID = string.Empty;
                string EmployeeID = string.Empty;
                string BookID = string.Empty;
                int quizID = 0;
                foreach (var item in input)
                {
                    FirstName = item.firstname;
                    LastName = item.lastname;
                    CompanyID = item.companyid;
                    EmployeeID = item.userdetailid;
                    BookID = item.bookid;
                    quizID = item.quizid;
                    quiz.Add(new QuizAnswerAssessment { QuizID = item.quizid, CompanyID = new Guid(item.companyid), QuestionID = item.questionid, UserDetailID = new Guid(item.userdetailid), CorrectAnswer = item.defaultanswer, UserAnswer = item.useranswer, IsActive = true, CreatedBy = new Guid(item.userdetailid), CreatedDate = date, ModifiedBy = new Guid(item.userdetailid), ModifiedDate = date });

                }
                _context.QuizAnswerAssessments.AddRange(quiz);
                await _context.SaveChangesAsync();

                Employee employeeForMember = (from e in _context.Employees
                                     join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                     where ud.ID == new Guid(EmployeeID) && e.IsActive == true
                                     select e).FirstOrDefault();

                var percentagebookcredit = ((score * (10)) / 100);
                objcredit.ID = Guid.NewGuid();
                objcredit.KeyID = new Guid(BookID);
                objcredit.KeyType = "Book";
                objcredit.UserDetailID = new Guid(EmployeeID);
                objcredit.CompanyID = new Guid(CompanyID);
                objcredit.Credit = percentagebookcredit;
                objcredit.AwardedDate = DateTime.UtcNow;
                objcredit.FirstName = FirstName;
                objcredit.LastName = LastName;
                objcredit.MemberID = employeeForMember.MemberID;
                _context.CreditLogs.Add(objcredit);
                await _context.SaveChangesAsync();
                var quizDetial = await _context.Quizs.FindAsync(quizID);
                string fullName = string.Empty;

                var readbyname = _context.Employees.Where(a => a.ID == new Guid(EmployeeID)).Select(a => a).FirstOrDefault();
                if (readbyname == null)
                {
                    readbyname = await (from e in _context.Employees
                                        join u in _context.UserDetails on e.ID equals u.EmployeeID
                                        where u.ID == new Guid(EmployeeID)
                                        select e).FirstOrDefaultAsync();
                }
                fullName = readbyname.FirstName + " " + readbyname.LastName;
                Guid empid = readbyname.ID;
                if (readbyname != null)
                {
                    CompanyLibraryBook book = _context.CompanyLibraryBooks.Find(new Guid(BookID));
                    //SendMail.QuizAssessmentEmailSendToUser(quizDetial, readbyname, percentagebookcredit, book.Title);
                    QuizAssMailToUserViewModel quizAssMailToUser = new QuizAssMailToUserViewModel()
                    {
                        Username = readbyname.FirstName+" "+ readbyname.LastName,
                        BookTitle = book.Title,
                        Score = percentagebookcredit,
                        UserEmail = readbyname.Email
                    };
                    EmailWorkQueue userEmailWorkQueue = new EmailWorkQueue
                    {
                        WorkItemType = "QuizAssessmentEmailSendToUser",
                        KeyID = "",
                        KeyType = "",
                        SendToEmployee = Guid.Empty,
                        Subject = "Quiz - " + quizDetial.Name,
                        Body = "",
                        Template = "QuizAssessmentEmailSendToUser.html",
                        TemplateContent = new JavaScriptSerializer().Serialize(quizAssMailToUser),
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(userEmailWorkQueue);


                    Employee employee = new Employee();
                    var empDetails = _context.Employees.Where(a => a.ID == readbyname.MentorId).Select(a => a).FirstOrDefault();
                    var empDetilsfrommentor = _context.Mentors.Where(a => a.ID == readbyname.MentorId).Select(a => a).FirstOrDefault();
                    employee.Email = empDetails == null ? empDetilsfrommentor.Email : empDetails.Email;
                    employee.FirstName = empDetails == null ? empDetilsfrommentor.FirstName : empDetails.FirstName;
                    employee.LastName = empDetails == null ? empDetilsfrommentor.LastName : empDetails.LastName;

                    fullName = readbyname.FirstName + " " + readbyname.LastName;
                    //SendMail.QuizAssessmentEmailSendToMentor(quizDetial, employee, percentagebookcredit, fullName, empid);
                    QuizAssMailToMentorViewModel quizAssMailToMentor = new QuizAssMailToMentorViewModel()
                    {
                        AngularUrl= AngularUrl+ "Main/viewquiz?qid=" + quizDetial.ID + "&eid=" + empid,
                        EmployeeName =fullName,
                        MentorFirstName=employee.FirstName,
                        MentorLastName=employee.LastName,
                        QuizName=quizDetial.Name,
                        MentorEmail=employee.Email
                    };
                    EmailWorkQueue emailWorkQueue = new EmailWorkQueue
                    {
                        WorkItemType = "QuizAssessmentEmailSendToMentor",
                        KeyID = "",
                        KeyType = "",
                        SendToEmployee = Guid.Empty,
                        Subject = "Quiz - " + quizDetial.Name,
                        Body = "",
                        Template = "QuizAssessmentEmailSendToMentor.html",
                        TemplateContent = new JavaScriptSerializer().Serialize(quizAssMailToMentor),
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(emailWorkQueue);


                    var companyDetials = await _context.CompanyDetails.FindAsync(new Guid(CompanyID));
                    if (companyDetials != null)
                    {

                        //SendMail.QuizAssessmentEmailSendToCompanyAdmin(quizDetial, companyDetials, percentagebookcredit, fullName);
                        QuizAssMailToAdminViewModel quizAssMailToAdmin = new QuizAssMailToAdminViewModel()
                        {
                            FullName=fullName,
                            Score=percentagebookcredit,
                            QuizName=quizDetial.Name,
                            CompanyEmail=companyDetials.email,
                            CompanyName=companyDetials.companyname
                        };
                        EmailWorkQueue emailWorkQueue1 = new EmailWorkQueue
                        {
                            WorkItemType = "QuizAssessmentEmailSendToCompanyAdmin",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee =Guid.Empty,
                            Subject = "Quiz - "+quizDetial.Name,
                            Body = "",
                            Template = "QuizAssessmentEmailSendToCompanyAdmin.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(quizAssMailToAdmin),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue1);
                    }

                    return new ResponseViewModel { Code = 200, Message = "Your Book Read info successfully!" };
                }
                return new ResponseViewModel { Code = 200, Message = "Quiz has been successfully saved!" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<QuizResponse> SaveQuiz(QuizViewModel quizViewModel)
        {
            try
            {
                int quizID = quizViewModel.quizid;
                var date = DateTime.UtcNow;
                if (quizID == 0)
                {
                    Quiz quiz = new Quiz()
                    {
                        Name = quizViewModel.quizName,
                        Descrition = quizViewModel.description,
                        IsActive = true,
                        CreatedBy = new Guid(quizViewModel.userDetailID),
                        Createddate = date,
                        ModifiedBy = new Guid(quizViewModel.userDetailID),
                        ModifiedDate = date,
                        Instructions = quizViewModel.Instructions
                    };
                    _context.Quizs.Add(quiz);
                    await _context.SaveChangesAsync();
                    quizID = quiz.ID;
                }
                else
                {
                    var quizdetail = await _context.Quizs.FindAsync(quizID);
                    quizdetail.Name = quizViewModel.quizName;
                    quizdetail.Descrition = quizViewModel.description;
                    quizdetail.ModifiedBy = new Guid(quizViewModel.userDetailID);
                    quizdetail.ModifiedDate = date;
                    quizdetail.Instructions = quizViewModel.Instructions;
                    await _context.SaveChangesAsync();
                }


                if (quizID > 0)
                {
                    List<CompanyLibraryBook> companyLibraryBooks = ((from clb in _context.CompanyLibraryBooks
                                                                     where clb.ID == new Guid(quizViewModel.bookID) && clb.IsActive == true
                                                                     select clb)
                                                                    .Union(from clb1 in _context.CompanyLibraryBooks
                                                                           where clb1.ParentBookID == new Guid(quizViewModel.bookID) && clb1.IsActive == true
                                                                           select clb1)).ToList();
                    if (companyLibraryBooks != null)
                    {
                        foreach (CompanyLibraryBook companyLibraryBook in companyLibraryBooks)
                        {
                            companyLibraryBook.QuizID = quizID;
                            companyLibraryBook.ModifiedBy = new Guid(quizViewModel.userDetailID);
                            companyLibraryBook.ModifiedDate = date;
                            _context.SaveChanges();
                        }
                    }
                }

                return new QuizResponse { QuizID = quizID, Code = 200, Message = "Saved Successfully" };
            }
            catch (Exception ex)
            {
                return new QuizResponse { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<ResponseViewModel> SaveQuestion(QuestionViewModel questionViewModel)
        {
            try
            {
                int questionID = questionViewModel.quesID;
                var date = DateTime.UtcNow;
                if (questionID == 0)
                {
                    Question question = new Question()
                    {
                        QuizID = questionViewModel.quizID,
                        QuestionName = questionViewModel.questionName,
                        QuestionType = questionViewModel.questionType,
                        CorrectAnswer = questionViewModel.correctAnswer,
                        IsActive = true,
                        CreatedBy = new Guid(questionViewModel.userDetailID),
                        CreatedDate = date,
                        ModifiedBy = new Guid(questionViewModel.userDetailID),
                        ModifiedDate = date
                    };

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    questionID = question.ID;
                }
                else
                {

                    var ques = await _context.Questions.FindAsync(questionID);
                    ques.QuizID = questionViewModel.quizID;
                    ques.QuestionName = questionViewModel.questionName;
                    ques.QuestionType = questionViewModel.questionType;
                    ques.CorrectAnswer = questionViewModel.correctAnswer;
                    ques.IsActive = true;
                    ques.ModifiedBy = new Guid(questionViewModel.userDetailID);
                    ques.ModifiedDate = date;
                }
                var quesOptions = await _context.QuestionOptions.Where(x => x.QuestionID == questionID).ToListAsync();
                _context.QuestionOptions.RemoveRange(quesOptions);
                var success = await _context.SaveChangesAsync();
                if (questionID > 0 || success > 0)
                {
                    if (!string.IsNullOrWhiteSpace(questionViewModel.option1))
                        SaveQuestionOption(questionViewModel, questionID, date, questionViewModel.option1);

                    if (!string.IsNullOrWhiteSpace(questionViewModel.option2))
                        SaveQuestionOption(questionViewModel, questionID, date, questionViewModel.option2);

                    if (!string.IsNullOrWhiteSpace(questionViewModel.option3))
                        SaveQuestionOption(questionViewModel, questionID, date, questionViewModel.option3);

                    if (!string.IsNullOrWhiteSpace(questionViewModel.option4))
                        SaveQuestionOption(questionViewModel, questionID, date, questionViewModel.option4);
                }

                return new ResponseViewModel { Code = 200, Message = "Saved Successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        private void SaveQuestionOption(QuestionViewModel questionViewModel, int questionID, DateTime date, string optionValue)
        {
            QuestionOption questionOption = new QuestionOption()
            {
                QuestionID = questionID,
                Options = optionValue,
                CorrectAnswer = questionViewModel.correctAnswer,
                IsActive = true,
                CreatedBy = new Guid(questionViewModel.userDetailID),
                CreatedDate = date,
                ModifiedBy = new Guid(questionViewModel.userDetailID),
                ModifiedDate = date
            };

            _context.QuestionOptions.Add(questionOption);
            _context.SaveChanges();
        }

        public async Task<List<GetQuizLIstResponseViewModel>> GetQuizList(string companyID, string userID, bool isIndividual)
        {
            return !isIndividual ? await (from quiz in _context.Quizs
                                          where quiz.IsActive == true
                                          join book in _context.CompanyLibraryBooks on quiz.ID equals book.QuizID into g
                                          from ct in g.DefaultIfEmpty()
                                          where ct.CompanyID == new Guid(companyID) && ct.ParentBookID == null
                                          select new GetQuizLIstResponseViewModel
                                          {
                                              quizid = quiz.ID,
                                              quizname = quiz.Name,
                                              isactive = quiz.IsActive,
                                              descrition = quiz.Descrition,
                                              bookname = ct.Title,
                                              ispublished = quiz.IsPublished,
                                              questionlist = _context.Questions.Where(x => x.QuizID == quiz.ID && x.IsActive == true).ToList()
                                          }).ToListAsync() :
                          await (from quiz in _context.Quizs
                                 where quiz.IsActive == true
                                 join book in _context.CompanyLibraryBooks on quiz.ID equals book.QuizID into g
                                 from ct in g.DefaultIfEmpty()
                                 where ct.UserDetailID == new Guid(userID) && ct.ParentBookID == null
                                 select new GetQuizLIstResponseViewModel
                                 {
                                     quizid = quiz.ID,
                                     quizname = quiz.Name,
                                     isactive = quiz.IsActive,
                                     descrition = quiz.Descrition,
                                     bookname = ct.Title,
                                     ispublished = quiz.IsPublished,
                                     questionlist = _context.Questions.Where(x => x.QuizID == quiz.ID && x.IsActive == true).ToList()
                                 }).ToListAsync();
        }

        public async Task<QuestionViewModel> GetQuestionDetails(int quesID)
        {
            try
            {
                QuestionViewModel res = new QuestionViewModel();
                var quesDetail = await _context.Questions.FindAsync(quesID);
                var quesOptions = await _context.QuestionOptions.Where(x => x.QuestionID == quesID).ToListAsync();
                if (quesDetail != null)
                {
                    res.questionName = quesDetail.QuestionName;
                    res.correctAnswer = quesDetail.CorrectAnswer;
                    res.questionType = quesDetail.QuestionType;
                }

                for (int i = 0; i < quesOptions.Count(); i++)
                {
                    res.option1 = quesOptions[0].Options;
                    res.option2 = quesOptions[1].Options;
                    res.option3 = quesOptions[2].Options;
                    res.option4 = quesOptions[3].Options;

                }
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<QuizViewModel> GetQuizDetails(int quizID, string companyID, string userID, bool isIndividual)
        {
            try
            {
                QuizViewModel res = new QuizViewModel();
                var quizdetail = await _context.Quizs.FindAsync(quizID);
                CompanyLibraryBook bookdetail = !isIndividual ? _context.CompanyLibraryBooks.Where(x => x.QuizID == quizdetail.ID && x.CompanyID == new Guid(companyID) && x.ParentBookID == null).FirstOrDefault() :
                   _context.CompanyLibraryBooks.Where(x => x.QuizID == quizdetail.ID && x.UserDetailID == new Guid(userID) && x.ParentBookID == null).FirstOrDefault();
                res.quizid = quizdetail.ID;
                res.quizName = quizdetail.Name;
                res.description = quizdetail.Descrition;
                res.bookID = Convert.ToString(bookdetail.ID);
                res.bookTitle = bookdetail.Title;
                res.Instructions = quizdetail.Instructions;

                return res;

            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<ResponseViewModel> DeleteQuiz(int quizID)
        {
            var date = DateTime.UtcNow;
            var isQuizExist = await _context.Quizs.FindAsync(quizID);
            if (isQuizExist != null)
            {
                var quesList = await _context.Questions.Where(x => x.QuizID == quizID).ToListAsync();
                foreach (var item in quesList)
                {
                    var quesoptionslist = await _context.QuestionOptions.Where(x => x.QuestionID == item.ID).ToListAsync();
                    foreach (var opt in quesoptionslist)
                    {
                        opt.IsActive = false;
                        opt.ModifiedDate = date;
                    }
                    item.IsActive = false;
                    item.ModifiedDate = date;
                }
                var booklist = await _context.CompanyLibraryBooks.Where(x => x.QuizID == quizID).ToListAsync();
                booklist.ForEach(x => x.QuizID = 0);
                isQuizExist.IsActive = false;
                isQuizExist.ModifiedDate = date;
                await _context.SaveChangesAsync();
                return new ResponseViewModel { Code = 200, Message = "Quiz Successfully Deleted!" };
            }
            else
            {
                return new ResponseViewModel { Code = 403, Message = "Quiz not Found!" };
            }
        }

        public async Task<ResponseViewModel> DeleteQuestion(int quesID)
        {
            var date = DateTime.UtcNow;
            var ques = await _context.Questions.FindAsync(quesID);
            if (ques != null)
            {

                var quesoptionslist = await _context.QuestionOptions.Where(x => x.QuestionID == ques.ID).ToListAsync();
                foreach (var opt in quesoptionslist)
                {
                    opt.IsActive = false;
                    opt.ModifiedDate = date;
                }
                ques.IsActive = false;
                ques.ModifiedDate = date;

                await _context.SaveChangesAsync();
                return new ResponseViewModel { Code = 200, Message = "Question Successfully Deleted!" };
            }
            else
            {
                return new ResponseViewModel { Code = 403, Message = "Question not Found!" };
            }
        }

        public async Task<ResponseViewModel> ChangePublishedSetting(int quizID, bool IsPublished)
        {
            try
            {
                var IsQuizExist = await _context.Quizs.FindAsync(quizID);
                if (IsQuizExist != null)
                {
                    IsQuizExist.IsPublished = IsPublished;
                    await _context.SaveChangesAsync();
                    return new ResponseViewModel { Code = 200, Message = "Quiz Successfully Published!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Quiz not Found!" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<QuizAnswerAssessmentInputViewModel>> GetQuizByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            try
            {
                Guid MentorID = new Guid(mentorID);
                Guid EmployeeID = new Guid(employeeID);
                List<QuizAnswerAssessmentInputViewModel> quizAnswerAssessmentInputViewModels = await _context.Database.SqlQuery<QuizAnswerAssessmentInputViewModel>(
                    "dbo.usp_GetQuizByMentorIDAndEmployeeID @MentorID = @mentorID, @IsActive = @isActive, @EmployeeID = @employeeID",
                    new SqlParameter("mentorID", MentorID),
                    new SqlParameter("isActive", isActive),
                    new SqlParameter("employeeID", EmployeeID)).ToListAsync();

                return quizAnswerAssessmentInputViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<QuizAnswerAssessmentInputViewModel>> GetQuizAnswerAssessmentByQuizIDAndEmployeeID(int quizID, bool isActive, string employeeID)
        {
            try
            {
                Guid EmployeeID = new Guid(employeeID);
                List<QuizAnswerAssessmentInputViewModel> quizAnswerAssessmentInputViewModels = await _context.Database.SqlQuery<QuizAnswerAssessmentInputViewModel>(
                    "dbo.usp_GetQuizAnswerAssessmentByQuizIDAndEmployeeID @QuizID = @quizID, @IsActive = @isActive, @EmployeeID = @employeeID",
                    new SqlParameter("quizID", quizID),
                    new SqlParameter("isActive", isActive),
                    new SqlParameter("employeeID", EmployeeID)).ToListAsync();

                return quizAnswerAssessmentInputViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<QuizStatusModel>> GetIncompleteQuiz()
        {
            try
            {
                var data = await _context.QuizStatus.Where(a => a.IsQuizStarted == true && a.IsQuizFinished == false).Select(a => new QuizStatusModel
                {
                    ID = a.ID,
                    UserdetailID = a.UserdetailID,
                    CompanyID = a.CompanyID.HasValue ? a.CompanyID.Value : new Guid(),
                    CompanyLibraryBookID = a.CompanyLibraryBookID,
                    QuizID = a.QuizID,
                    IsQuizStarted = a.IsQuizStarted,
                    IsQuizFinished = a.IsQuizFinished
                }).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<QuizStatusModel>> GetIncompleteQuizByUserIDandCompID(string userID, string CompanyID)
        {
            try
            {
               
                var data = await _context.QuizStatus.Where(a => a.IsQuizStarted == true && a.IsQuizFinished == false && a.UserdetailID == new Guid(userID) && a.CompanyID == new Guid(CompanyID)).Select(a => new QuizStatusModel
                {

                    ID = a.ID,
                    UserdetailID = a.UserdetailID,
                    CompanyID = a.CompanyID.HasValue ? a.CompanyID.Value : new Guid(),
                    CompanyLibraryBookID = a.CompanyLibraryBookID,
                    QuizID = a.QuizID,
                    IsQuizStarted = a.IsQuizStarted,
                    IsQuizFinished = a.IsQuizFinished,

                    QuizName = _context.Quizs.Where(q => q.ID == a.QuizID).Select(q => q.Name).FirstOrDefault(),
                    BookName = _context.CompanyLibraryBooks.Where(b => b.ID == a.CompanyLibraryBookID).Select(b => b.Title).FirstOrDefault()
                  
                    
                }).ToListAsync();
                
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> SaveQuizFinishStatus(int quizStatusID)
        {
            try
            {
                var data = await _context.QuizStatus.Where(a => a.ID == quizStatusID).Select(a => a).FirstOrDefaultAsync();
                if (data != null)
                {
                    data.IsQuizFinished = true;
                    _context.SaveChanges();
                    return new ResponseViewModel { Code = 200, Message = "Quiz Finish Status Successfully Updated!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Quiz not Found!" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> SaveQuizStartStatus(QuizStatusModel quizStartStatus)
        {
            try
            {
                var data = await _context.QuizStatus.Where(a => a.QuizID == quizStartStatus.QuizID && a.CompanyID == quizStartStatus.CompanyID && a.UserdetailID == quizStartStatus.UserdetailID && a.CompanyLibraryBookID == quizStartStatus.CompanyLibraryBookID).Select(a => a).FirstOrDefaultAsync();

                if (data == null)
                {

                    QuizStatu quiz = new QuizStatu
                    {
                        UserdetailID = quizStartStatus.UserdetailID,
                        CompanyID = quizStartStatus.CompanyID,
                        CompanyLibraryBookID = quizStartStatus.CompanyLibraryBookID,
                        QuizID = quizStartStatus.QuizID,
                        IsQuizFinished = false,
                        IsQuizStarted = true
                  
                    };
                    _context.QuizStatus.Add(quiz);
                    await _context.SaveChangesAsync();
                    return quiz.ID;
                }
                else
                {
                    return data.ID;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseViewModel> DeleteQuizStatus(int quizStatusID)
        {
            var quiz = await _context.QuizStatus.Where(a => a.ID == quizStatusID).Select(a => a).FirstOrDefaultAsync();
           
            if (quiz != null)
            {
                var quiz1 = await _context.CreditLogs.Where(b => b.KeyID == quiz.CompanyLibraryBookID && b.UserDetailID == quiz.UserdetailID).Select(b => b).FirstOrDefaultAsync();
                var quiz2 = await _context.QuizAnswerAssessments.Where(c => c.QuizID == quiz.QuizID && c.UserDetailID == quiz.UserdetailID).Select(c => c).FirstOrDefaultAsync();

                _context.QuizStatus.Remove(quiz);
                await _context.SaveChangesAsync();

                if(quiz1 != null)
                {
                    _context.CreditLogs.Remove(quiz1);
                    await _context.SaveChangesAsync();
                }

                if(quiz2 != null)
                {
                    _context.QuizAnswerAssessments.Remove(quiz2);
                    await _context.SaveChangesAsync();
                }

                return new ResponseViewModel { Code = 200, Message = "Quiz Successfully Deleted!" };
            }
            else
            {
                return new ResponseViewModel { Code = 403, Message = "Quiz not Found!" };
            }
        }
    }
}
