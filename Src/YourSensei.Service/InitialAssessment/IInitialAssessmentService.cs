using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IInitialAssessmentService
    {
        Task<List<GetInitialAssessmentResponseViewModel>> GetInitialAssessment();
        Task<ResponseViewModel> SaveAssessmentAnswer(IEnumerable<AssessmentAnswerInputViewModel> input);
        Task<List<InitialAssessmentAnswerViewModel>> GetInitialAssessmentAnswer(int sequenceNumber, bool isActive);
    }
}
