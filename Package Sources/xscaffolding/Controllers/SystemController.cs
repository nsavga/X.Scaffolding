using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using X.Scaffolding.Core;

namespace Manage.Controllers
{
    public class SystemController : Controller
    {
        public ActionResult Login()
        {
            return View(new NetworkCredential());
        }        

        [HttpPost]
		[ValidateInput(false)]
        public ActionResult Login(NetworkCredential credentials)
        {
            if (credentials.UserName == "test" && credentials.Password == "test")
            {
                FormsAuthentication.RedirectFromLoginPage(credentials.UserName, true);
                return Redirect("~/");
            }
            
            return View(credentials);
        }

        public ActionResult UploadFile()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize]
		[ValidateInput(false)]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            var url = String.Empty;

            if (file.ContentLength > 0)
            {
                var name = Guid.NewGuid() + Path.GetExtension(file.FileName);
                url = X.Storage.StorageManager.Instance.Upload(file.InputStream, name);
            }

            return View((object)url);
        }

        [HttpPost]
        [Authorize]
		[ValidateInput(false)]
        public ActionResult CKEditorFileUpload()
        {
            var message = "The upload is complete";
            var id = Request["CKEditorFuncNum"];
            var file = Request.Files["upload"];

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var bytes = new byte[file.InputStream.Length];
            file.InputStream.Read(bytes, 0, bytes.Length);
            
            var url = X.Storage.StorageManager.Instance.Upload(bytes, fileName);

            var callback = String.Format("{0}, '{1}', '{2}'", id, url, message);

            var sb = new StringBuilder();

            sb.AppendFormat(@"<img src=""{0}""/>
            <script type='text/javascript'>
                window.parent.CKEDITOR.tools.callFunction({1});
            </script>", url, callback);

            return Content(sb.ToString());
        }

        [Authorize]
        public ActionResult Exception()
        {
            var exception = Session["application_error"] as Exception;

            IEnumerable<String> model = new List<String> {"Errer detected"};

            if (exception != null)
            {
                model = GetExceptionDescription(exception);
            }
           

            return View(model);
        }

        [Authorize]
        public ActionResult RestartWebApplication()
        {
            X.Storage.IStorage storage = new X.Storage.StorageManager("", "");

            try
            {
                var bytes = storage.Download("web.config");
                var localPath = Path.GetTempFileName();
                System.IO.File.WriteAllBytes(localPath, bytes);
                System.IO.File.SetLastAccessTime(localPath, DateTime.Now);
                storage.Upload(System.IO.File.ReadAllBytes(localPath), "web.config");

                return Content("OK");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        private static IEnumerable<String> GetExceptionDescription(Exception ex)
        {
            var list = new List<String> { ex.Message };

            if (ex.InnerException != null)
            {
                list.AddRange(GetExceptionDescription(ex.InnerException));
            }

            return list;
        }
    }
}
