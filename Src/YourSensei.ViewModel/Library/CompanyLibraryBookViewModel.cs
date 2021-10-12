using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CompanyLibraryBookViewModel
    {
        public string Id { get; set; }
        public string CompanyID { get; set; }       
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
        public decimal Credit { get; set; }
        public bool isIndividual { get; set; }
        public string ParentBookId { get; set; }
        public string TrackCategory { get; set; }
    }
}
