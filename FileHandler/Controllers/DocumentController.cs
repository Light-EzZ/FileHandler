using FileHandler.Services;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace FileHandler.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["ViewBag.ErrorMessage"] != null)
            {
                //Якщо файл пройшов перевірку JS але не проходить maxRequestLength спрацьовує метод Application_Error у Global.asax
                ViewBag.ErrorMessage = Session["ViewBag.ErrorMessage"].ToString();

               
                Session.Remove("ViewBag.ErrorMessage");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public ActionResult Index(string keywords, HttpPostedFileBase fileUpload, bool caseSensitive = true)
        {
            
            if (fileUpload == null || fileUpload.ContentLength == 0)
            {
                ViewBag.ErrorMessage = "Будь ласка, оберіть файл.";
                return View();
            }
            string fileName = Path.GetFileName(fileUpload.FileName);
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                !fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Підтримуються тільки файли .pdf та .html.";
                return View();
            }

            
            byte[] processedData;
            try
            {
              
                processedData = _documentService.ProcessFile(fileUpload.InputStream, fileName, keywords, caseSensitive);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Сталася помилка обробки файлу: " + ex.Message;
                return View();
            }

            
            string newFileName = "highlighted_" + fileName;
            string mimeType = (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                ? "application/pdf"
                : "text/html";

            return File(processedData, mimeType, newFileName);
        }
    }
}