using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class GetBookResponseViewModel
    {
        public List<CompanyLibraryBook> ListOfNewBooks { get; set; }
        public List<BookList> ListOfBooks { get; set; }
    }
    public class BookList
    {
        public Guid Id { get; set; }
        public Nullable<System.Guid> CompanyID { get; set; }
        public string CoverImageUrl { get; set; }
        public string CoverImageOriginalFileName { get; set; }
        public string BookOriginalFileName { get; set; }
        public string BookUrl { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string SubTitle { get; set; }
        public string Category { get; set; }
        public string Year { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public decimal? Rating { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal Credit { get; set; }
        public bool IsBookRead { get; set; }
        public Nullable<System.Guid> ParentBookId { get; set; }
        public int QuizID { get; set; }
        public bool IsQuizAttended { get; set; }
        public bool IsQuizPublished { get; set; }
        public string TrackCategory { get; set; }
    }
}
