using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel.TrainingEvents
{
    public class KaizenBoardListViewModel
    {
        public Guid id { get; set; }
        public string TrainingEventName { get; set; }
        public string TrainingEventFormat { get; set; }
        public DateTime StartDate { get; set; }
        public string Leader { get; set; }
        public string Attendee { get; set; }
        public string Intials { get; set; }
        public bool Idea { get; set; }
        public bool Plan { get; set; }
        public bool Do { get; set; }
        public bool Check { get; set; }
        public bool Act { get; set; }
        public bool Complete { get; set; }
        public decimal DollarImpacted { get; set; }
        public string Notes { get; set; }
    }

    public class KaizenBoardViewModel
    {
        public Guid id { get; set; }
        public string TrainingEventName { get; set; }
        public string TrainingEventFormat { get; set; }
        public DateTime StartDate { get; set; }
        public string Leader { get; set; }
        public string Attendee { get; set; }
        public string Intials { get; set; }
        public List<Intials> Team { get; set; }
        public bool Idea { get; set; }
        public bool Plan { get; set; }
        public bool Do { get; set; }
        public bool Check { get; set; }
        public bool Act { get; set; }
        public bool Complete { get; set; }
        public decimal DollarImpacted { get; set; }
        public string Notes { get; set; }
    }

    public class Intials
    {
        public string ShortName { set; get; }
        public string FullName { set; get; }
    }
}
