using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class TrackCategoryViewModel
    {
        public int? TrackId { get; set; }
        public string UserID { get; set; }
        public string TrackName { get; set; }
//        public string CategorySequesnce { get; set; }
        public string TrackCategories { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

    }
}
