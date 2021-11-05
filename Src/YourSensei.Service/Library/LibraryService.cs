using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;
using YourSensei.Utility;
using YourSensei.Service;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Text.RegularExpressions;

namespace YourSensei.Service
{
    public class LibraryService : ILibraryService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly IEmailWorkQueueService _emailWorkQueueService;

        public LibraryService(YourSensei_DBEntities context, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _emailWorkQueueService = emailWorkQueueService;
        }

        public async Task<GetBookResponseViewModel> GetBook(string companyID, string userID, bool isIndividual)
        {
            try
            {
                GetBookResponseViewModel response = new GetBookResponseViewModel();
                List<CompanyLibraryBook> newBookForSlect = new List<CompanyLibraryBook>();
                List<BookList> booklist = new List<BookList>();
                //var averageratingscore;
                List<BookRead> averageratingscore = new List<BookRead>();
                List<BookRead> allcompanyaverageratingscore = new List<BookRead>();
                // var CompanyWiseAverageBookRating = await _context.CompanySettings.AnyAsync(a => a.CompanyId == new Guid(companyID) && a.GlobalAverageBookRating == true);
                var result = !isIndividual ?
                    await _context.CompanyLibraryBooks.Where(d => d.IsActive == true && d.IsAccepted == true && d.CompanyID == new Guid(companyID)).ToListAsync()
                    : await _context.CompanyLibraryBooks.Where(d => d.IsActive == true && d.IsAccepted == true && (d.UserDetailID == new Guid(userID))).Union(_context.CompanyLibraryBooks.Where(d => d.IsActive == true && d.IsAccepted == true && (d.CompanyID == new Guid(companyID)))).ToListAsync();

                var IsGolbalBookUpdateTrue = await _context.CompanySettings.FirstOrDefaultAsync(x => x.CompanyId == new Guid(companyID));
                if (IsGolbalBookUpdateTrue != null)
                {
                    if (IsGolbalBookUpdateTrue.GlobalBookList)
                    {
                        newBookForSlect = !isIndividual ?
                            await _context.CompanyLibraryBooks.Where(d => d.IsActive == true && d.IsAccepted == null && (d.CompanyID == new Guid(companyID))).ToListAsync()
                            : await _context.CompanyLibraryBooks.Where(d => d.IsActive == true && d.IsAccepted == null && (d.UserDetailID == new Guid(userID))).ToListAsync();
                    }
                }

                foreach (var item in result)
                {
                    allcompanyaverageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID).Select(a => a).ToListAsync();
                    averageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID && a.CompanyID == item.CompanyID).Select(a => a).ToListAsync();
                    //if (CompanyWiseAverageBookRating == true)
                    //{
                    //     averageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID).Select(a => a).ToListAsync();
                    //}
                    //else
                    //{
                    //     averageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID && a.CompanyID == item.CompanyID).Select(a => a).ToListAsync();
                    //}
                    //  var averageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID).Select(a => a).ToListAsync();
                    int companybookaveragecount = allcompanyaverageratingscore.Count();
                    var companyoverallrating = 0;
                    decimal companybookrating = 0;
                    foreach (var companyratingsum in allcompanyaverageratingscore)
                    {
                        companyoverallrating += companyratingsum.Rating;
                    }
                    if (companybookaveragecount != 0)
                    {
                        companybookrating = decimal.Round((Convert.ToDecimal(companyoverallrating) / Convert.ToDecimal(companybookaveragecount)), 2);
                    }

                    int averageratingcount = averageratingscore.Count();
                    var overallrating = 0;
                    decimal bookrating = 0;
                    foreach (var ratingsum in averageratingscore)
                    {
                        overallrating += ratingsum.Rating;
                    }
                    if (averageratingcount != 0)
                    {
                        bookrating = decimal.Round((Convert.ToDecimal(overallrating) / Convert.ToDecimal(averageratingcount)), 2);
                    }

                    bool IsQuizAttended = (from qaa in _context.QuizAnswerAssessments
                                           join clb in _context.CompanyLibraryBooks on qaa.QuizID equals clb.QuizID
                                           where qaa.CompanyID == item.CompanyID && qaa.UserDetailID == new Guid(userID) && clb.ID == item.ID
                                           select qaa).Any();

                    BookList bookresponse = new BookList()
                    {
                        Id = (item.ID),
                        CoverImageUrl = string.IsNullOrWhiteSpace(item.CoverImageUrl) ? "no-image-icon-15.png" : item.CoverImageUrl,
                        Title = item.Title,
                        CompanyName = item.CompanyID != new Guid("00000000-0000-0000-0000-000000000000") ? item.CompanyID != null ? _context.CompanyDetails.Find(item.CompanyID).companyname : null : null,
                        Author = item.Author,
                        Publisher = item.Publisher,
                        Year = item.Year,
                        Rating = bookrating,
                        AverageRating = companybookrating,
                        CompanyID = (item.CompanyID),
                        IsBookRead = _context.BookReads.Any(a => a.BookID == item.ID && a.UserDetailID == new Guid(userID) && a.CompanyID == new Guid(companyID)),
                        ParentBookId = (item.ParentBookID),
                        QuizID = item.QuizID,
                        IsQuizAttended = IsQuizAttended,
                        TrackCategory = string.IsNullOrWhiteSpace(item.TrackCategory) ? "" :item.TrackCategory,
                        IsQuizPublished = _context.Quizs.Any(a => a.ID == item.QuizID && a.IsPublished == true && a.IsActive == true)
                    };
                    booklist.Add(bookresponse);
                }
                response.ListOfBooks = booklist;
                response.ListOfNewBooks = newBookForSlect;
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }


            //List<CompanyLibraryBookViewModel> booklist = new List<CompanyLibraryBookViewModel>();
            //var result = await _context.CompanyLibraryBooks.Where(d => d.IsActive == true && (d.CompanyID == new Guid(companyID) || new Guid(companyID) == new Guid("00000000-0000-0000-0000-000000000000"))).ToListAsync();
            //foreach (var item in result)
            //{               
            //    var averageratingscore = await _context.BookReads.Where(a => a.BookID == item.ID && a.CompanyID == item.CompanyID).Select(a => a).ToListAsync();
            //    var averageratingcount = averageratingscore.Count();
            //    var overallrating = 0;
            //    decimal bookrating = 0;
            //    foreach (var ratingsum in averageratingscore)
            //    {
            //        overallrating += ratingsum.Rating;
            //    }
            //    if (averageratingcount != 0) 
            //    { 
            //    bookrating = overallrating / averageratingcount;
            //    }
            //    CompanyLibraryBookViewModel bookresponse = new CompanyLibraryBookViewModel()
            //    {
            //        Id = Convert.ToString(item.ID),
            //        CoverImageUrl = item.CoverImageUrl,
            //        Title = item.Title,
            //        CompanyName = item.CompanyID != new Guid("00000000-0000-0000-0000-000000000000") ? _context.CompanyDetails.Find(item.CompanyID).companyname : null,
            //        Author = item.Author,
            //        Publisher = item.Publisher,
            //        Year = item.Year,                 
            //       Rating = bookrating,
            //        CompanyID = Convert.ToString(item.CompanyID)

            //    };
            //    booklist.Add(bookresponse);
            //}
            //return booklist;
        }

        public async Task<ResponseViewModel> AddBook(CompanyLibraryBookViewModel book)
        {
            CompanyLibraryBook obj = new CompanyLibraryBook();

            try
            {
                //Regex re = new Regex(@"([a-zA-Z]+)(\d+)");

                int trackCategoryInitialCount = 0;

                trackCategoryInitialCount = !book.isIndividual ?
                    await _context.CompanyLibraryBooks.CountAsync(x => x.TrackCategory.Contains(book.TrackCategory + "-") && x.CompanyID == new Guid(book.CompanyID)) :
                    await _context.CompanyLibraryBooks.CountAsync(x => x.TrackCategory.Contains(book.TrackCategory + "-") && x.UserDetailID == new Guid(book.UserId));
                if (book.Id == null)
                {

                    obj = new CompanyLibraryBook();
                    obj.ID = Guid.NewGuid();
                    obj.IsActive = true;
                    obj.Author = book.Author;
                    obj.CoverImageUrl = book.CoverImageUrl;
                    obj.CoverImageOriginalFileName = book.CoverImageOriginalFileName;
                    obj.CreatedDate = DateTime.UtcNow;
                    if (book.isIndividual)
                    {
                        obj.UserDetailID = new Guid(book.UserId);
                    }
                    else
                    {
                        obj.CompanyID = new Guid(book.CompanyID);
                    }
                    obj.Credit = 10;
                    obj.Publisher = book.Publisher;
                    obj.Rating = 0;
                    obj.Title = book.Title;
                    obj.Year = book.Year;
                    obj.SubTitle = book.SubTitle;
                    obj.BookCategoryID = new Guid(book.Category);
                    obj.BookUrl = book.BookUrl;
                    obj.BookOriginalFileName = book.BookOriginalFileName;
                    obj.ModifiedDate = DateTime.UtcNow;
                    obj.CreatedBy = new Guid(book.UserId);
                    obj.IsAccepted = true;
                    obj.ParentBookID = null;
                    obj.QuizID = 0;
                    obj.IsVersion = false;
                    obj.TrackCategory = string.IsNullOrWhiteSpace(book.TrackCategory) ? "" :  book.TrackCategory + "-" + Convert.ToString(trackCategoryInitialCount + 1);
                    if (!string.IsNullOrEmpty(book.ParentBookId))
                    {
                        obj.ParentBookID = new Guid(book.ParentBookId);
                        obj.QuizID = _context.CompanyLibraryBooks.Find(new Guid(book.ParentBookId)).QuizID;
                        obj.IsVersion = true;
                    }
                    obj.IsModified = false;


                    var response = _context.CompanyLibraryBooks.Add(obj);

                    await _context.SaveChangesAsync();

                    if (book.CompanyID == "00000000-0000-0000-0000-000000000000" && !book.isIndividual)
                    {
                        ReplicateGlobalBook(obj, "add");
                    }


                    return new ResponseViewModel { Code = 200, Message = "Your book has been created successfully" };
                }
                else
                {
                    Guid bookid = new Guid(book.Id);
                    var result = await _context.CompanyLibraryBooks.FirstOrDefaultAsync(d => d.ID == bookid);
                    if (result != null)
                    {
                        result.IsActive = true;
                        result.Author = book.Author;
                        result.CoverImageUrl = book.CoverImageUrl;
                        result.CoverImageOriginalFileName = book.CoverImageOriginalFileName;
                        result.CompanyID = new Guid(book.CompanyID);
                        result.Publisher = book.Publisher;
                        result.Title = book.Title;
                        result.Year = book.Year;
                        result.SubTitle = book.SubTitle;
                        result.BookCategoryID = new Guid(book.Category);
                        result.ModifiedBy = new Guid(book.UserId);
                        result.BookUrl = book.BookUrl;
                        result.BookOriginalFileName = book.BookOriginalFileName;
                        result.ModifiedDate = DateTime.UtcNow;
                        result.ModifiedBy = new Guid(book.UserId);
                        result.IsModified = true;
                        if (result.TrackCategory.Split('-')[0] == book.TrackCategory)
                        {
                            result.TrackCategory = book.TrackCategory + "-" + result.TrackCategory.Split('-')[1];
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(result.TrackCategory))
                            {
                                var trackIniTial = result.TrackCategory.Split('-')[0];
                                var sequenceNumber = result.TrackCategory.Split('-')[1];
                                ChangeBookTrackCategorySequence(book.isIndividual,book.UserId,book.CompanyID,trackIniTial,sequenceNumber);
                            }
                            result.TrackCategory = book.TrackCategory + "-" + Convert.ToString(trackCategoryInitialCount + 1);
                        }

                        var response = await _context.SaveChangesAsync();
                        if (response > 0)
                        {
                            if (book.CompanyID == "00000000-0000-0000-0000-000000000000" && !book.isIndividual)
                            {
                                ReplicateGlobalBook(result, "update");
                            }
                        }
                        return new ResponseViewModel { Code = 200, Message = "Your book has been updated successfully" };
                    }

                    return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
                }
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        private void ChangeBookTrackCategorySequence(bool isIndividual, string UserId, string CompanyID, string trackIniTial, string sequenceNumber)
        {
            
            //await _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-")).ToListAsync();
            var trackCategoryInitialCountforupdate = !isIndividual ?
     _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.CompanyID == new Guid(CompanyID)).ToList() :
     _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.UserDetailID == new Guid(UserId)).ToList();
            for (int i = 0; i < trackCategoryInitialCountforupdate.Count(); i++)
            {
                int cuurentTracksequence = Convert.ToInt32(trackCategoryInitialCountforupdate[i].TrackCategory.Split('-')[1]);
                if (cuurentTracksequence > Convert.ToInt32(sequenceNumber))
                {
                    trackCategoryInitialCountforupdate[i].TrackCategory = trackIniTial + "-" + Convert.ToString(cuurentTracksequence - 1);
                    _context.SaveChanges();
                }
               

            }
            
        }

        public async Task<CompanyLibraryBook> GetBookById(string id)
        {
            try
            {
                Guid bookid = new Guid(id);
                var result = await _context.CompanyLibraryBooks.FirstOrDefaultAsync(d => d.IsActive == true && d.ID == bookid);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> DeleteBook(string id)
        {
            Guid bookid = new Guid(id);
            try
            {
                var result = await _context.CompanyLibraryBooks.FirstOrDefaultAsync(d => d.ID == bookid);
                result.IsActive = false;
                var trackIniTial = result.TrackCategory.Split('-')[0];
                var sequenceNumber = result.TrackCategory.Split('-')[1];
                var trackCategoryInitialCount = result.UserDetailID == null ?
                    await _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.CompanyID == result.CompanyID).ToListAsync() :
                    await _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.UserDetailID == result.UserDetailID).ToListAsync();

                for (int i = 0; i < trackCategoryInitialCount.Count(); i++)
                {
                    int cuurentTracksequence = Convert.ToInt32(trackCategoryInitialCount[i].TrackCategory.Split('-')[1]);
                    if (cuurentTracksequence > Convert.ToInt32(sequenceNumber))
                    {
                        trackCategoryInitialCount[i].TrackCategory = result.TrackCategory.Split('-')[0] + "-" + Convert.ToString(cuurentTracksequence - 1);
                    }
                }
                var response = await _context.SaveChangesAsync();
                if (response > 0)
                {
                    if (result.CompanyID == new Guid("00000000-0000-0000-0000-000000000000") && result.UserDetailID == null)
                    {
                        ReplicateGlobalBook(result, "delete");
                    }

                }
                return new ResponseViewModel { Code = 200, Message = "Your Book record has been deleted successfully!" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }

        }

        public async Task<List<BookCategory>> GetBookCategory()
        {

            try
            {
                List<BookCategory> bookcatList = new List<BookCategory>();
                bookcatList = await _context.BookCategories.Where(d => d.IsActive == true && d.Category != "Other").ToListAsync();
                bookcatList.Add(await _context.BookCategories.Where(d => d.IsActive == true && d.Category == "Other").FirstOrDefaultAsync());
                return bookcatList;
            }
            catch
            {
                throw;
            }

        }

        public async Task<ResponseViewModel> UpdateBook(CompanyLibraryBookViewModel book)
        {
            CompanyLibraryBook obj = new CompanyLibraryBook();
            try
            {
                obj.IsActive = true;
                obj.Author = book.Author;
                obj.CoverImageUrl = book.CoverImageUrl;
                obj.CreatedDate = DateTime.UtcNow;
                obj.CompanyID = new Guid(book.CompanyID);
                obj.Publisher = book.Publisher;
                obj.Rating = 0;
                obj.Title = book.Title;
                obj.Year = book.Year;
                obj.SubTitle = book.SubTitle;
                obj.Credit = book.Credit;

                obj.ParentBookID = new Guid(book.ParentBookId);


                await _context.SaveChangesAsync();
                return new ResponseViewModel { Code = 200, Message = "Your book has been updated successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<ResponseViewModel> MarkBookRead(BookReadViewModel bookReadViewModel)
        {
            BookRead objbr = new BookRead();
            Data.CreditLog objcredit = new Data.CreditLog();

            try
            {
                // SendMail.BookReadEmailSendToMentor("abc", "Tarun Arora");
                var data = await _context.BookReads.Where(a => a.BookID == new Guid(bookReadViewModel.BookID) && a.UserDetailID == new Guid(bookReadViewModel.EmployeeID) && a.CompanyID == new Guid(bookReadViewModel.CompanyID)).ToListAsync();
                if (data.Count == 0)
                {
                    objbr.ID = Guid.NewGuid();
                    objbr.BookID = new Guid(bookReadViewModel.BookID);
                    objbr.UserDetailID = new Guid(bookReadViewModel.EmployeeID);
                    objbr.Completeddate = DateTime.UtcNow;
                    objbr.Rating = bookReadViewModel.Rating;
                    objbr.Comments = bookReadViewModel.Comments;
                    objbr.CompanyID = new Guid(bookReadViewModel.CompanyID);
                    _context.BookReads.Add(objbr);
                    await _context.SaveChangesAsync();

                    var bookcredit = await _context.CompanyLibraryBooks.FindAsync(new Guid(bookReadViewModel.BookID));
                    if (bookcredit != null)
                    {
                        if (bookcredit.QuizID == 0)
                        {
                            Employee employee = (from e in _context.Employees
                                                 join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                 where ud.ID == new Guid(bookReadViewModel.EmployeeID) && e.IsActive == true
                                                 select e).FirstOrDefault();

                            var percentagebookcredit = ((bookcredit.Credit * (100)) / 100);
                            objcredit.ID = Guid.NewGuid();
                            objcredit.KeyID = new Guid(bookReadViewModel.BookID);
                            objcredit.KeyType = "Book";
                            objcredit.UserDetailID = new Guid(bookReadViewModel.EmployeeID);
                            objcredit.CompanyID = new Guid(bookReadViewModel.CompanyID);
                            objcredit.Credit = percentagebookcredit;
                            objcredit.AwardedDate = DateTime.UtcNow;
                            objcredit.FirstName = bookReadViewModel.FirstName;
                            objcredit.LastName = bookReadViewModel.LastName;
                            objcredit.MemberID = employee.MemberID;
                            _context.CreditLogs.Add(objcredit);
                            await _context.SaveChangesAsync();
                        }
                    }


                    string fullName = string.Empty;
                    var readbyname = _context.Employees.Where(a => a.ID == new Guid(bookReadViewModel.EmployeeID)).Select(a => a).FirstOrDefault();
                    if (readbyname == null)
                    {
                        readbyname = await (from e in _context.Employees
                                            join u in _context.UserDetails on e.ID equals u.EmployeeID
                                            where u.ID == new Guid(bookReadViewModel.EmployeeID)
                                            select e).FirstOrDefaultAsync();
                    }
                    fullName = readbyname.FirstName + " " + readbyname.LastName;
                    if (readbyname != null)
                    {

                        BookReadToEmployeeViewModel bookReadToEmployeeViewModel = new BookReadToEmployeeViewModel()
                        {
                            ToEmployeeEmail = readbyname.Email,
                            ToEmployeeName = readbyname.FirstName + " " + readbyname.LastName,
                            BookTitle = bookcredit.Title,

                            AngularURL = ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/library/"
                        };

                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = bookcredit.QuizID == 0 ? "BookReadToEmployee" : "BookReadWithQuizToEmployee",
                            KeyID = bookcredit.ID.ToString(),
                            KeyType = "Book",
                            SendToEmployee = Guid.Empty,
                            Subject = "Book Read - " + bookcredit.Title,
                            Body = "",
                            Template = bookcredit.QuizID == 0 ? "BookReadToEmployee.html" : "BookReadWithQuizToEmployee.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(bookReadToEmployeeViewModel),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //SendMail.BookReadEmailSendToEmployee(bookcredit, readbyname);

                        Employee employee = new Employee();
                        var empDetails = _context.Employees.Where(a => a.ID == readbyname.MentorId).Select(a => a).FirstOrDefault();
                        var empDetilsfrommentor = _context.Mentors.Where(a => a.ID == readbyname.MentorId).Select(a => a).FirstOrDefault();
                        fullName = readbyname.FirstName + " " + readbyname.LastName;
                        if (empDetails != null)
                        {
                            BookReadToMentorViewModel bookReadToMentorViewModel = new BookReadToMentorViewModel()
                            {
                                ToEmployeeEmail = empDetails.Email,
                                ToEmployeeName = empDetails.FirstName + " " + empDetails.LastName,
                                BookTitle = bookcredit.Title,
                                StudentName = fullName,
                                Subject = "Book Read - " + bookcredit.Title,
                                AngularURL = ConfigurationManager.AppSettings["AngularUrl"].ToString()
                            };

                            EmailWorkQueue emailWorkQueueformentor = new EmailWorkQueue()
                            {
                                WorkItemType = "BookReadToMentor",
                                KeyID = bookcredit.ID.ToString(),
                                KeyType = "Book",
                                SendToEmployee = Guid.Empty,
                                Subject = "Book Read - " + bookcredit.Title,
                                Body = "",
                                Template = "BookReadToMentor.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(bookReadToMentorViewModel),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueueformentor);

                            //employee.Email = empDetails.Email;
                            //employee.FirstName = empDetails.FirstName;
                            //employee.LastName = empDetails.LastName;
                            //SendMail.BookReadEmailSendToMentor(employee, bookcredit, fullName);
                        }
                        else if (empDetilsfrommentor != null)
                        {
                            BookReadToMentorViewModel bookReadToMentorViewModel = new BookReadToMentorViewModel()
                            {
                                ToEmployeeEmail = empDetilsfrommentor.Email,
                                ToEmployeeName = empDetilsfrommentor.FirstName + " " + empDetilsfrommentor.LastName,
                                BookTitle = bookcredit.Title,
                                StudentName = fullName,
                                Subject = "Book Read - " + bookcredit.Title,
                                AngularURL = ConfigurationManager.AppSettings["AngularUrl"].ToString()
                            };

                            EmailWorkQueue emailWorkQueueformentor = new EmailWorkQueue()
                            {
                                WorkItemType = "BookReadToMentor",
                                KeyID = bookcredit.ID.ToString(),
                                KeyType = "Book",
                                SendToEmployee = Guid.Empty,
                                Subject = "Book Read - " + bookcredit.Title,
                                Body = "",
                                Template = "BookReadToMentor.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(bookReadToMentorViewModel),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueueformentor);

                            //employee.Email = empDetilsfrommentor.Email;
                            //employee.FirstName = empDetilsfrommentor.FirstName;
                            //employee.LastName = empDetilsfrommentor.LastName;
                            //SendMail.BookReadEmailSendToMentor(employee, bookcredit, fullName);

                        }
                        //employee.Email = empDetails == null ? empDetilsfrommentor.Email : empDetails.Email;
                        //employee.FirstName = empDetails == null ? empDetilsfrommentor.FirstName : empDetails.FirstName;
                        //employee.LastName = empDetails == null ? empDetilsfrommentor.LastName : empDetails.LastName;

                        //fullName = readbyname.FirstName + " " + readbyname.LastName;
                        //SendMail.BookReadEmailSendToMentor(employee, bookcredit, fullName);


                        var companyDetials = await (from cd in _context.CompanyDetails
                                                    join e in _context.Employees on cd.email equals e.Email
                                                    where cd.ID == new Guid(bookReadViewModel.CompanyID)
                                                    select e).FirstOrDefaultAsync();

                        if (companyDetials != null)
                        {
                            BookReadToMentorViewModel bookReadToMentorViewModel = new BookReadToMentorViewModel()
                            {
                                ToEmployeeEmail = companyDetials.Email,
                                ToEmployeeName = companyDetials.FirstName + " " + companyDetials.LastName,
                                BookTitle = bookcredit.Title,
                                StudentName = fullName,
                                Subject = "Book Read - " + bookcredit.Title
                            };

                            EmailWorkQueue emailWorkQueueforadmin = new EmailWorkQueue()
                            {
                                WorkItemType = "BookReadToCompanyAdmin",
                                KeyID = bookcredit.ID.ToString(),
                                KeyType = "Book",
                                SendToEmployee = Guid.Empty,
                                Subject = "Book Read - " + bookcredit.Title,
                                Body = "",
                                Template = "BookReadToCompanyAdmin.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(bookReadToMentorViewModel),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueueforadmin);

                            //SendMail.BookReadEmailSendToCompanyAdmin(companyDetials, bookcredit, fullName);
                        }
                        return new ResponseViewModel { Code = 200, Message = "Your Book Read info successfully!" };
                    }

                    return new ResponseViewModel { Code = 200, Message = "Your Book Read info successfully!" };

                }
                return new ResponseViewModel { Code = 201, Message = "Your Book Already read!" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }
        }

        public async Task<Boolean> IsBookRead(string bookid, string employeeid)
        {
            try
            {
                var data = await _context.BookReads.AnyAsync(a => a.BookID == new Guid(bookid) && a.UserDetailID == new Guid(employeeid));
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> AddBookFromGlobalBook(SelectedCompanyLibraryBookLogsViewModel selectedCompanyLibraryBookLogsViewModel)
        {
            try
            {
                foreach (int selectedID in selectedCompanyLibraryBookLogsViewModel.SelectedIds)
                {
                    CompanyLibraryBookLog companyLibraryBookLog = await _context.CompanyLibraryBookLogs.FindAsync(selectedID);
                    CompanyLibraryBook companyLibraryBook = (from clb in _context.CompanyLibraryBooks
                                                             where clb.CompanyID == new Guid(selectedCompanyLibraryBookLogsViewModel.CompanyID)
                                                              && clb.IsActive == true && clb.IsModified == true && clb.IsVersion == false
                                                              && clb.ParentBookID == companyLibraryBookLog.CompanyLibraryBookID
                                                             select clb).FirstOrDefault();

                    CompanyLibraryBook newCompanyLibraryBook = new JavaScriptSerializer().Deserialize<CompanyLibraryBook>(companyLibraryBookLog.NewObject);

                    if (companyLibraryBookLog.Action == "UPDATED")
                    {
                        int trackCategoryInitialCount = 0;

                        trackCategoryInitialCount = _context.CompanyLibraryBooks.Count(x => x.TrackCategory.Contains(newCompanyLibraryBook.TrackCategory + "-") && x.CompanyID == newCompanyLibraryBook.CompanyID);
                        companyLibraryBook.CoverImageUrl = newCompanyLibraryBook.CoverImageUrl;
                        companyLibraryBook.CoverImageOriginalFileName = newCompanyLibraryBook.CoverImageOriginalFileName;
                        companyLibraryBook.Author = newCompanyLibraryBook.Author;
                        companyLibraryBook.Title = newCompanyLibraryBook.Title;
                        companyLibraryBook.Publisher = newCompanyLibraryBook.Publisher;
                        companyLibraryBook.Year = newCompanyLibraryBook.Year;
                        companyLibraryBook.Rating = newCompanyLibraryBook.Rating;
                        companyLibraryBook.ModifiedDate = newCompanyLibraryBook.ModifiedDate;
                        companyLibraryBook.ModifiedBy = newCompanyLibraryBook.ModifiedBy;
                        companyLibraryBook.BookCategoryID = newCompanyLibraryBook.BookCategoryID;
                        companyLibraryBook.SubTitle = newCompanyLibraryBook.SubTitle;
                        companyLibraryBook.BookUrl = newCompanyLibraryBook.BookUrl;
                        companyLibraryBook.BookOriginalFileName = newCompanyLibraryBook.BookOriginalFileName;
                        companyLibraryBook.Credit = newCompanyLibraryBook.Credit;
                        companyLibraryBook.QuizID = newCompanyLibraryBook.QuizID;
                        companyLibraryBook.IsModified = false;
                        //companyLibraryBook.TrackCategory = newCompanyLibraryBook.TrackCategory;
                        if (companyLibraryBook.TrackCategory.Split('-')[0] == newCompanyLibraryBook.TrackCategory)
                        {
                            companyLibraryBook.TrackCategory = newCompanyLibraryBook.TrackCategory + "-" + companyLibraryBook.TrackCategory.Split('-')[1];
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(companyLibraryBook.TrackCategory))
                            {
                                var trackIniTial = companyLibraryBook.TrackCategory.Split('-')[0];
                                var sequenceNumber = companyLibraryBook.TrackCategory.Split('-')[1];
                                await _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-")).ToListAsync();
                                var trackCategoryInitialCountforupdate = _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.CompanyID == newCompanyLibraryBook.CompanyID).ToList();

                                for (int i = 0; i < trackCategoryInitialCountforupdate.Count(); i++)
                                {
                                    int cuurentTracksequence = Convert.ToInt32(trackCategoryInitialCountforupdate[i].TrackCategory.Split('-')[1]);
                                    if (cuurentTracksequence > Convert.ToInt32(sequenceNumber))
                                    {
                                        trackCategoryInitialCountforupdate[i].TrackCategory = companyLibraryBook.TrackCategory.Split('-')[0] + "-" + Convert.ToString(cuurentTracksequence - 1);
                                    }
                                }
                            }
                            companyLibraryBook.TrackCategory = newCompanyLibraryBook.TrackCategory;
                        }
                        _context.SaveChanges();

                        CompanyLibraryBookLogTransaction companyLibraryBookLogTransaction = new CompanyLibraryBookLogTransaction()
                        {
                            CompanyID = new Guid(selectedCompanyLibraryBookLogsViewModel.CompanyID),
                            CompanyLibraryBookLogsID = selectedID,
                            IsActive = true,
                            AcceptedBy = new Guid(selectedCompanyLibraryBookLogsViewModel.UserDetailID),
                            AcceptedDate = DateTime.UtcNow
                        };
                        _context.CompanyLibraryBookLogTransactions.Add(companyLibraryBookLogTransaction);
                        _context.SaveChanges();
                    }
                }
                return new ResponseViewModel { Code = 200, Message = "Successfully Updated" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> IsAverageBookRating(string companyID)
        {
            try
            {
                var data = await _context.CompanySettings.AnyAsync(a => a.CompanyId == new Guid(companyID) && a.GlobalAverageBookRating == true);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReplicateGlobalBook(CompanyLibraryBook book, string action)
        {
            try
            {
                CompanyLibraryBook companyLibraryBook = null;
                var companyList = _context.CompanyDetails.ToList();

                foreach (var item in companyList)
                {
                    if (action == "add")
                    {
                        companyLibraryBook = new CompanyLibraryBook();
                        companyLibraryBook.ID = Guid.NewGuid();
                        companyLibraryBook.IsActive = true;
                        companyLibraryBook.Author = book.Author;
                        companyLibraryBook.CoverImageUrl = book.CoverImageUrl;
                        companyLibraryBook.CoverImageOriginalFileName = book.CoverImageOriginalFileName;
                        companyLibraryBook.CreatedDate = DateTime.UtcNow;
                        companyLibraryBook.CompanyID = item.ID;
                        companyLibraryBook.UserDetailID = null;
                        companyLibraryBook.Publisher = book.Publisher;
                        companyLibraryBook.Rating = 0;
                        companyLibraryBook.Title = book.Title;
                        companyLibraryBook.Year = book.Year;
                        companyLibraryBook.SubTitle = book.SubTitle;
                        companyLibraryBook.BookCategoryID = book.BookCategoryID;
                        companyLibraryBook.BookUrl = book.BookUrl;
                        companyLibraryBook.BookOriginalFileName = book.BookOriginalFileName;
                        companyLibraryBook.ModifiedDate = DateTime.UtcNow;
                        companyLibraryBook.CreatedBy = book.CreatedBy;
                        companyLibraryBook.Credit = book.Credit;
                        companyLibraryBook.ModifiedBy = book.ModifiedBy;
                        companyLibraryBook.IsAccepted = true;
                        companyLibraryBook.ParentBookID = book.ID;
                        companyLibraryBook.IsModified = false;
                        companyLibraryBook.TrackCategory = book.TrackCategory;
                        companyLibraryBook.QuizID = book.QuizID;
                        companyLibraryBook.IsVersion = false;
                        _context.CompanyLibraryBooks.Add(companyLibraryBook);
                        _context.SaveChanges();
                    }
                    else if (action == "update")
                    {
                        bool isGlobalBookUpdate = (from cs in _context.CompanySettings
                                                   where cs.CompanyId == item.ID
                                                   select cs.GlobalBookList).SingleOrDefault();
                        if (isGlobalBookUpdate)
                        {
                            companyLibraryBook = (from clb in _context.CompanyLibraryBooks
                                                  where clb.CompanyID == item.ID
                                                    && clb.IsActive == true
                                                    && clb.ParentBookID == book.ID
                                                    && clb.IsModified == false
                                                    && clb.IsVersion == false
                                                  select clb).FirstOrDefault();
                            if (companyLibraryBook != null)
                            {
                                companyLibraryBook.Author = book.Author;
                                companyLibraryBook.CoverImageUrl = book.CoverImageUrl;
                                companyLibraryBook.CoverImageOriginalFileName = book.CoverImageOriginalFileName;
                                companyLibraryBook.Publisher = book.Publisher;
                                companyLibraryBook.Title = book.Title;
                                companyLibraryBook.Year = book.Year;
                                companyLibraryBook.SubTitle = book.SubTitle;
                                companyLibraryBook.BookCategoryID = book.BookCategoryID;
                                companyLibraryBook.BookUrl = book.BookUrl;
                                companyLibraryBook.BookOriginalFileName = book.BookOriginalFileName;
                                companyLibraryBook.ModifiedDate = DateTime.UtcNow;
                                companyLibraryBook.Credit = book.Credit;
                                companyLibraryBook.ModifiedBy = book.ModifiedBy;
                                companyLibraryBook.IsAccepted = true;
                                companyLibraryBook.ParentBookID = book.ID;
                                companyLibraryBook.QuizID = book.QuizID;
                                //companyLibraryBook.TrackCategory = book.TrackCategory;
                                if (companyLibraryBook.TrackCategory.Split('-')[0] == book.TrackCategory)
                                {
                                    companyLibraryBook.TrackCategory = book.TrackCategory;
                                }
                                else
                                {
                                   
                                    if (!string.IsNullOrWhiteSpace(companyLibraryBook.TrackCategory))
                                    {
                                        var trackIniTial = companyLibraryBook.TrackCategory.Split('-')[0];
                                        var sequenceNumber = companyLibraryBook.TrackCategory.Split('-')[1];

                                        ChangeBookTrackCategorySequence(false, "", Convert.ToString(companyLibraryBook.CompanyID), trackIniTial, sequenceNumber);
                                    }
                                    companyLibraryBook.TrackCategory = book.TrackCategory;
                                }
                                _context.SaveChanges();
                            }
                            else
                            {
                                var OldObject = new JavaScriptSerializer().Serialize(companyLibraryBook);
                                var NewObject = new JavaScriptSerializer().Serialize(book);
                                CompanyLibraryBookLog booklogs = new CompanyLibraryBookLog()
                                {
                                    CompanyLibraryBookID = book.ID,
                                    OldObject = OldObject,
                                    NewObject = NewObject,
                                    Action = "UPDATED",
                                    ActionDate = DateTime.UtcNow,
                                    Description = "Book Updated",
                                    IsActive = true,
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = book.CreatedBy
                                };
                                _context.CompanyLibraryBookLogs.Add(booklogs);
                                _context.SaveChanges();
                            }
                        }
                    }
                    else if (action == "delete")
                    {
                        companyLibraryBook = (from clb in _context.CompanyLibraryBooks
                                              where clb.CompanyID == item.ID
                                                && clb.IsActive == true
                                                && clb.ParentBookID == book.ID
                                                && clb.IsVersion == false
                                              select clb).FirstOrDefault();
                        if (companyLibraryBook != null)
                        {
                            bool isBookRead = (from br in _context.BookReads
                                               where br.CompanyID == item.ID
                                                && br.BookID == companyLibraryBook.ID
                                               select br).Any();
                            if (!isBookRead)
                            {
                                companyLibraryBook.IsActive = false;
                                companyLibraryBook.ModifiedDate = DateTime.UtcNow;
                                companyLibraryBook.ModifiedBy = book.ModifiedBy;
                                var trackIniTial = companyLibraryBook.TrackCategory.Split('-')[0];
                                var sequenceNumber = companyLibraryBook.TrackCategory.Split('-')[1];
                                var trackCategoryInitialCount = _context.CompanyLibraryBooks.Where(x => x.TrackCategory.Contains(trackIniTial + "-") && x.CompanyID == companyLibraryBook.CompanyID).ToList();


                                for (int i = 0; i < trackCategoryInitialCount.Count(); i++)
                                {
                                    int cuurentTracksequence = Convert.ToInt32(trackCategoryInitialCount[i].TrackCategory.Split('-')[1]);
                                    if (cuurentTracksequence > Convert.ToInt32(sequenceNumber))
                                    {
                                        trackCategoryInitialCount[i].TrackCategory = companyLibraryBook.TrackCategory.Split('-')[0] + "-" + Convert.ToString(cuurentTracksequence - 1);
                                    }
                                }
                                _context.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void ReplicateGlobalBookForCompanyIndividual(CompanyLibraryBook book, Guid? companyID, Guid? userID)
        {
            if (companyID != null && companyID != Guid.Empty)
            {
                CompanyLibraryBook obj = new CompanyLibraryBook();

                obj = new CompanyLibraryBook();
                obj.ID = Guid.NewGuid();
                obj.IsActive = true;
                obj.Author = book.Author;
                obj.CoverImageUrl = book.CoverImageUrl;
                obj.CoverImageOriginalFileName = book.CoverImageOriginalFileName;
                obj.CreatedDate = DateTime.UtcNow;
                obj.CompanyID = companyID;
                obj.UserDetailID = null;
                obj.Publisher = book.Publisher;
                obj.Rating = 0;
                obj.Title = book.Title;
                obj.Year = book.Year;
                obj.SubTitle = book.SubTitle;
                obj.BookCategoryID = book.BookCategoryID;
                obj.BookUrl = book.BookUrl;
                obj.BookOriginalFileName = book.BookOriginalFileName;
                obj.ModifiedDate = DateTime.UtcNow;
                obj.CreatedBy = book.CreatedBy;
                obj.Credit = book.Credit;
                obj.ModifiedBy = book.ModifiedBy;
                obj.IsAccepted = true;
                obj.ParentBookID = book.ID;
                obj.IsModified = false;
                obj.IsVersion = false;
                obj.QuizID = book.QuizID;
                _context.CompanyLibraryBooks.Add(obj);
                _context.SaveChanges();
            }
        }

        public async Task<List<CompanyLibraryBook>> GetBookByCompanyID(string companyID, string userID, bool isIndividual)
        {
            try
            {
                List<CompanyLibraryBook> companyLibraryBooks = !isIndividual ? await _context.CompanyLibraryBooks.Where(c => c.CompanyID == new Guid(companyID) &&
                    c.IsActive == true && c.QuizID == 0 && c.ParentBookID == null).OrderBy(c => c.Title).ToListAsync() :
                    await _context.CompanyLibraryBooks.Where(c => c.UserDetailID == new Guid(userID) &&
                    c.IsActive == true && c.QuizID == 0 && c.ParentBookID == null).OrderBy(c => c.Title).ToListAsync();
                return companyLibraryBooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<BookReadViewModel>> GetBookReadEventByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            try
            {
                Guid MentorID = new Guid(mentorID);
                Guid EmployeeID = new Guid(employeeID);
                List<BookReadViewModel> bookReadViewModels = await _context.Database.SqlQuery<BookReadViewModel>(
                    "dbo.usp_GetBookReadByMentorIDAndEmployeeID @MentorID = @mentorID, @IsActive = @isActive, @EmployeeID = @employeeID",
                    new SqlParameter("mentorID", MentorID),
                    new SqlParameter("isActive", isActive),
                    new SqlParameter("employeeID", EmployeeID)).ToListAsync();

                return bookReadViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CompanyLibraryBook>> GetCompanyLibraryBooksByMentorID(string mentorID, bool isActive, bool isAccepted)
        {
            try
            {
                Guid MentorID = new Guid(mentorID);
                List<CompanyLibraryBook> companyLibraryBooks = await _context.Database.SqlQuery<CompanyLibraryBook>(
                    "dbo.usp_GetCompanyLibraryBooksByMentorID @MentorID = @mentorID, @IsActive = @isActive, @IsAccepted = @isAccepted",
                    new SqlParameter("mentorID", MentorID),
                    new SqlParameter("isActive", isActive),
                    new SqlParameter("isAccepted", isAccepted)).ToListAsync();

                foreach (CompanyLibraryBook companyLibraryBook in companyLibraryBooks)
                {
                    companyLibraryBook.CoverImageUrl = string.IsNullOrWhiteSpace(companyLibraryBook.CoverImageUrl) ? "no-image-icon-15.png" : companyLibraryBook.CoverImageUrl;
                }

                return companyLibraryBooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CompanyLibraryBook>> GetGlobalBook()
        {
            var globalbooklist = await _context.CompanyLibraryBooks.Where(x => x.CompanyID == new Guid("00000000-0000-0000-0000-000000000000")).ToListAsync();
            return globalbooklist;
        }

        public async Task<List<BookList>> GetCompanyLibraryBooks(string companyID, string userID, bool isIndividual)
        {
            try
            {
                //Guid CompanyID = new Guid(companyID);
                //Guid UserID = new Guid(userID);
                //List<BookList> companyLibraryBooks = new List<BookList>();
                List<BookList> companyLibraryBooks = await _context.Database.SqlQuery<BookList>(
                    "dbo.usp_GetBook @CompanyID = @companyID, @UserID = @userID, @IsIndividual = @isIndividual",
                    new SqlParameter("companyID", companyID),
                    new SqlParameter("userID", userID),
                    new SqlParameter("isIndividual", isIndividual)).ToListAsync();

                //foreach (CompanyLibraryBook companyLibraryBook in companyLibraryBooks)
                //{
                //    companyLibraryBook.CoverImageUrl = string.IsNullOrWhiteSpace(companyLibraryBook.CoverImageUrl) ? "no-image-icon-15.png" : companyLibraryBook.CoverImageUrl;
                //}

                return companyLibraryBooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<CompanyLibraryBookLogsViewModel>> GetNotAcceptedCompanyLibraryBookLogs(string companyID, bool isActive)
        {
            try
            {
                Guid CompanyID = new Guid(companyID);
                List<CompanyLibraryBookLogsViewModel> CompanyLibraryBookLogsViewModel = await _context.Database.SqlQuery<CompanyLibraryBookLogsViewModel>(
                    "dbo.usp_GetNotAcceptedCompanyLibraryBookLogs @CompanyID = @companyID, @IsActive = @isActive",
                    new SqlParameter("companyID", CompanyID),
                    new SqlParameter("isActive", isActive)).ToListAsync();

                return CompanyLibraryBookLogsViewModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<List<BookTrackCategoryViewModel>> GetTrackList(string userID, string companyid, bool isIndividual)
        {
            try
            {
                List<BookTrackCategoryViewModel> list = new List<BookTrackCategoryViewModel>();


                var tracklist = await _context.BookTrackCategories.Where(x => x.UserID == new Guid(userID) && x.IsActive == true).Union(_context.BookTrackCategories.Where(x => x.UserID == new Guid("00000000-0000-0000-0000-000000000000") && x.IsActive == true)).ToListAsync();
                foreach (var item in tracklist)
                {
                    List<BookSequence> seqlist = new List<BookSequence>();
                    BookTrackCategoryViewModel mod = new BookTrackCategoryViewModel();
                    mod.trackName = item.TrackName;
                    mod.trackid = item.TrackId;
                    mod.userid = Convert.ToString(item.UserID);
                    mod.isDefault = item.IsDefault;
                    string[] booksequenceList = item.CategorySequesnce.Split(',');
                    foreach (var initial in booksequenceList)
                    {
                        BookSequence seq = new BookSequence();
                        seq.bookinitials = initial;
                        var bookdetails = !isIndividual ?
                            _context.CompanyLibraryBooks.Where(x => x.TrackCategory == initial && x.IsActive == true && x.CompanyID == new Guid(companyid)).FirstOrDefault()
                       : _context.CompanyLibraryBooks.Where(x => (x.TrackCategory == initial && x.IsActive == true && x.UserDetailID == new Guid(userID)) || (x.TrackCategory == initial && x.IsActive == true && x.CompanyID == Guid.Empty)).FirstOrDefault();

                        seq.bookid = bookdetails != null ? Convert.ToString(bookdetails.ID) : "Book Not Available";
                        seq.booktitle = bookdetails != null ? bookdetails.Title : "Book Not Available";
                        seq.isbookread = bookdetails != null ?
                        _context.BookReads.Any(a => a.BookID == bookdetails.ID && a.UserDetailID == new Guid(userID) && a.CompanyID == new Guid(companyid))
                        : false;


                        seqlist.Add(seq);
                    }
                    mod.categorySequesnce = seqlist;
                    list.Add(mod);
                }
                return list;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<ResponseViewModel> AddTrackCategory(TrackCategoryViewModel trackcategory)
        {
            BookTrackCategory obj = new BookTrackCategory();

            try
            {
                if (trackcategory.TrackId == null)
                {
                    obj = new BookTrackCategory();
                    obj.TrackName = trackcategory.TrackName;
                    obj.UserID = new Guid(trackcategory.UserID);
                    obj.IsActive = true;
                    obj.IsDefault = false;
                    obj.CreatedBy = new Guid(trackcategory.UserID);
                    obj.CreatedDate = DateTime.Now;
                    obj.CategorySequesnce = trackcategory.TrackCategories;
                    var response = _context.BookTrackCategories.Add(obj);

                    await _context.SaveChangesAsync();
                    return new ResponseViewModel { Code = 200, Message = "Your track has been created successfully" };
                }
                else if (trackcategory.UserID != null && trackcategory.TrackId != null)
                {
                    int trackId = trackcategory.TrackId.Value;
                    var result = await _context.BookTrackCategories.FirstOrDefaultAsync(d => d.TrackId == trackId);
                    if (result != null)
                    {
                        result.IsActive = true;
                        result.IsDefault = false;
                        result.TrackName = trackcategory.TrackName;
                        result.CategorySequesnce = trackcategory.TrackCategories;
                        result.UserID = new Guid(trackcategory.UserID);
                        result.ModifiedBy = new Guid(trackcategory.UserID);
                        result.ModifiedDate = DateTime.Now;
                        var response = await _context.SaveChangesAsync();

                        return new ResponseViewModel { Code = 200, Message = "Your track has been updated successfully" };
                    }
                }
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };

            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<BookTrackCategory> GetTrackCategoryById(string id)
        {
            try
            {
                var result = await _context.BookTrackCategories.FirstOrDefaultAsync(d => d.IsActive == true && d.TrackId.ToString() == id);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CompanyLibraryBook>> GetBookByTrack(string trackcategory, string userid, string companyid, bool isIndividual)
        {
            try
            {
                var result = isIndividual ?
                    await _context.CompanyLibraryBooks.Where(d => d.TrackCategory.Contains(trackcategory + "-") && d.UserDetailID == new Guid(userid)).ToListAsync()
                    : await _context.CompanyLibraryBooks.Where(d => d.TrackCategory.Contains(trackcategory + "-") && d.CompanyID == new Guid(companyid)).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> DeleteTrack(string id)
        {
            try
            {
                var result = await _context.BookTrackCategories.FirstOrDefaultAsync(d => d.TrackId.ToString() == id);
                result.IsActive = false;
                // _context.BookTrackCategories.Remove(result);
                var response = await _context.SaveChangesAsync();

                return new ResponseViewModel { Code = 200, Message = "Your track record has been deleted successfully!" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }

        }

        public async Task<ResponseViewModel> ChangeInUSeTRack(int trackid, bool inuse)
        {
            try
            {
                var IstrackExist = await _context.BookTrackCategories.FindAsync(trackid);


                if (IstrackExist != null)
                {
                    IstrackExist.IsDefault = inuse;
                    await _context.SaveChangesAsync();
                    return new ResponseViewModel { Code = 200, Message = "track in use successfully saved!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "track not Found!" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
