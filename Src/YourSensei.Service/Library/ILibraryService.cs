using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface ILibraryService
    {
        Task<GetBookResponseViewModel> GetBook(string companyID, string userID, bool isIndividual);
         Task<ResponseViewModel> AddBook(CompanyLibraryBookViewModel book);
        Task<ResponseViewModel> DeleteBook(string bookid);
        Task<CompanyLibraryBook> GetBookById(string bookid);
        Task<List<BookCategory>> GetBookCategory();
        Task<ResponseViewModel> UpdateBook(CompanyLibraryBookViewModel book);
        Task<ResponseViewModel> MarkBookRead(BookReadViewModel bookReadViewModel);
        Task<Boolean> IsBookRead(string bookid, string employeeid);
        Task<ResponseViewModel> AddBookFromGlobalBook(SelectedCompanyLibraryBookLogsViewModel selectedCompanyLibraryBookLogsViewModel);
        Task<Boolean> IsAverageBookRating(string companyID);
        void ReplicateGlobalBook(CompanyLibraryBook book, string action);
        void ReplicateGlobalBookForCompanyIndividual(CompanyLibraryBook book,Guid? companyID, Guid? userID);

        Task<List<CompanyLibraryBook>> GetBookByCompanyID(string companyID, string userID, bool isIndividual);

        Task<List<BookTrackCategoryViewModel>> GetTrackList(string userID, string companyid, bool isIndividual);

        Task<List<BookReadViewModel>> GetBookReadEventByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID);

        Task<List<CompanyLibraryBook>> GetCompanyLibraryBooksByMentorID(string mentorID, bool isActive, bool isAccepted);
        Task<List<CompanyLibraryBook>> GetGlobalBook();
        Task<List<BookList>> GetCompanyLibraryBooks(string companyID, string userID, bool isIndividual);
        Task<List<CompanyLibraryBookLogsViewModel>> GetNotAcceptedCompanyLibraryBookLogs(string companyID, bool isActive);
        Task<ResponseViewModel> AddTrackCategory(TrackCategoryViewModel trackcategory);
        Task<BookTrackCategory> GetTrackCategoryById(string id);
        Task<List<CompanyLibraryBook>> GetBookByTrack(string trackcategory, string userid, string companyid, bool isIndividual);
        Task<ResponseViewModel> DeleteTrack(string id);
        Task<ResponseViewModel> ChangeInUSeTRack(int trackid, bool inuse);

    }
}
