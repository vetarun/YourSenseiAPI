using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.ViewModel;

namespace YourSensei.Controllers
{
    //[Authorize]
    [RoutePrefix("Library")]
    public class LibraryController : ApiController
    {
        private readonly ILibraryService _service;

        public LibraryController(LibraryService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetBook")]
        public async Task<IHttpActionResult> GetBook(string companyID, string userID, bool isIndividual)
        {
            var result = await _service.GetCompanyLibraryBooks(companyID,userID,isIndividual);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddBook")]
        public async Task<IHttpActionResult> AddBook(CompanyLibraryBookViewModel obj)
        {
            var result = await _service.AddBook(obj);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeleteBook")]
        public async Task<IHttpActionResult> DeleteBook(string id)
        {
            var result = await _service.DeleteBook(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetBookCategory")]
        public async Task<IHttpActionResult> GetBookCategory()
        {
            var result = await _service.GetBookCategory();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetBookById")]
        public async Task<IHttpActionResult> GetBookById(string id)
        {
            var result = await _service.GetBookById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateBookCategory")]
        public async Task<IHttpActionResult> UpdateBookCategory()
        {
            var result = await _service.GetBookCategory();
            return Ok(result);
        }

        [HttpPost]
        [Route("MarkBookRead")]
        public async Task<IHttpActionResult> MarkBookRead(BookReadViewModel bookReadViewModel)
        {
            var result = await _service.MarkBookRead(bookReadViewModel);
            return Ok(result);
        }

        [HttpPost]
        [Route("UploadImage")]
        public string UploadImage()
        {
            string imageName = null;           
            var httpRequest = HttpContext.Current.Request;
            var postedFile = httpRequest.Files["filekey"];
            if (postedFile != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);
                var filePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
                postedFile.SaveAs(filePath);
            }

            return imageName;
        }

        [HttpPost]
        [Route("UploadBook")]
        public string UploadBook()
        {
            string imageName = null;
            var httpRequest = HttpContext.Current.Request;
            var postedFilebook = httpRequest.Files["fileKeybook"];
            if (postedFilebook != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(postedFilebook.FileName);
                var filePath = HttpContext.Current.Server.MapPath("~/Books/" + imageName);
                postedFilebook.SaveAs(filePath);
            }

            return imageName;
        }

        [HttpGet]
        [Route("IsBookRead")]
        public async Task<Boolean> IsBookRead(string bookid, string employeeid)
        {
            var result = await _service.IsBookRead(bookid,employeeid);
            return result;          
        }

        [HttpPost]
        [Route("AddBookFromGlobalBook")]
        public async Task<IHttpActionResult> AddBookFromGlobalBook(SelectedCompanyLibraryBookLogsViewModel selectedCompanyLibraryBookLogsViewModel)
        {
            var result = await _service.AddBookFromGlobalBook(selectedCompanyLibraryBookLogsViewModel);
            return Ok(result);
        }

        [HttpGet]
        [Route("IsAverageBookRating")]
        public async Task<Boolean> IsAverageBookRating(string companyID)
        {
            var result = await _service.IsAverageBookRating(companyID);
            return result;
        }

        [HttpGet]
        [Route("GetBookByCompanyID")]
        public async Task<IHttpActionResult> GetBookByCompanyID(string companyID, string userID, bool isIndividual)
        {
            var result = await _service.GetBookByCompanyID(companyID,userID,isIndividual);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetBookReadEventByMentorIDAndEmployeeID")]
        public async Task<IHttpActionResult> GetBookReadEventByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            var result = await _service.GetBookReadEventByMentorIDAndEmployeeID(mentorID, isActive, employeeID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCompanyLibraryBooksByMentorID")]
        public async Task<IHttpActionResult> GetCompanyLibraryBooksByMentorID(string mentorID, bool isActive, bool isAccepted)
        {
            var result = await _service.GetCompanyLibraryBooksByMentorID(mentorID, isActive, isAccepted);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetGlobalBook")]
        public async Task<IHttpActionResult> GetGlobalBook()
        {
            var result = await _service.GetGlobalBook();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetNotAcceptedCompanyLibraryBookLogs")]
        public async Task<IHttpActionResult> GetNotAcceptedCompanyLibraryBookLogs(string companyID, bool isActive)
        {
            var result = await _service.GetNotAcceptedCompanyLibraryBookLogs(companyID, isActive);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetTrackList")]
        public async Task<IHttpActionResult> GetTrackList(string userID, string companyid, bool isIndividual)
        {
            var result = await _service.GetTrackList(userID, companyid,isIndividual);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddTrackCategory")]
        public async Task<IHttpActionResult> AddTrackCategory(TrackCategoryViewModel trackcategory)
        {
            var result = await _service.AddTrackCategory(trackcategory);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetTrackCategoryById")]
        public async Task<IHttpActionResult> GetTrackCategoryById(string id)
        {
            var result = await _service.GetTrackCategoryById(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetBookByTrack")]
        public async Task<IHttpActionResult> GetBookByTrack(string trackcategory, string userid, string companyid, bool isIndividual)
        {
            var result = await _service.GetBookByTrack(trackcategory,userid,companyid,isIndividual);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeleteTrack")]
        public async Task<IHttpActionResult> DeleteTrack(string id)
        {
            var result = await _service.DeleteTrack(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ChangeInUSeTRack")]
        public async Task<IHttpActionResult> ChangeInUSeTRack(int trackid, bool inuse)
        {
            var result = await _service.ChangeInUSeTRack(trackid, inuse);
            return Ok(result);
        }
     
    }
}
