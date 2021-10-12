using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class BookTrackCategoryViewModel
    {
        public int trackid { get; set; }
        public string trackName { get; set; }
        public string userid { get; set; }
        public bool isDefault { get; set; }
        public List<BookSequence> categorySequesnce { get; set; }
    }

    public class BookSequence
    {
        public string bookinitials { get; set; }
        public string bookid { get; set; }
        public string booktitle { get; set; }
        public bool isbookread { get; set; }
    }
}
