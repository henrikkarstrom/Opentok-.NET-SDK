using Archiving.Models;
using Microsoft.AspNetCore.Mvc;
using OpenTokSDK;
using System;
using System.Dynamic;

namespace Archiving.Controllers
{
    public class HomeController : Controller
    {
        private readonly OpenTokService _opentokService;

        public HomeController(OpenTokService opentokService)
        {
            _opentokService = opentokService;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("host")]
        public IActionResult Host()
        {
            dynamic locals = new ExpandoObject();

            locals.ApiKey = _opentokService.OpenTok.ApiKey.ToString();
            locals.SessionId = _opentokService.Session.Id;
            locals.Token = _opentokService.Session.GenerateToken();

            return View(locals);
        }

        [HttpGet("participant")]
        public IActionResult Participant()
        {
            dynamic locals = new ExpandoObject();

            locals.ApiKey = _opentokService.OpenTok.ApiKey.ToString();
            locals.SessionId = _opentokService.Session.Id;
            locals.Token = _opentokService.Session.GenerateToken();

            return View(locals);
        }

        [HttpGet("history")]
        public IActionResult History(int? page)
        {
            var pageValue = page.HasValue ? (int)page : 1;
            var offset = (pageValue - 1) * 5;
            ArchiveList archives = _opentokService.OpenTok.ListArchives(offset, 5);

            string showPrevious = page > 1 ? "/history?page=" + (page - 1).ToString() : null;
            string showNext = archives.TotalCount > (offset + 5) ? "/history?page=" + (page + 1).ToString() : null;

            dynamic locals = new ExpandoObject();
            locals.Archives = archives;
            locals.ShowPrevious = showPrevious;
            locals.ShowNext = showNext;
            return View(locals);
        }

        [HttpGet("download/{id}")]
        public IActionResult Download([FromRoute]string id)
        {
            Archive archive = _opentokService.OpenTok.GetArchive(id);
            return Redirect(archive.Url);
        }

        [HttpPost("start")]
        public IActionResult Start(StartRequestModel form)
        {
            if (!bool.TryParse(form.HasAudio, out var hasAudio))
            {
                hasAudio = form.HasAudio.Equals("on", StringComparison.InvariantCultureIgnoreCase);
            }


            if (!bool.TryParse(form.HasVideo, out var hasVideo))
            {
                hasVideo = form.HasVideo.Equals("on", StringComparison.InvariantCultureIgnoreCase);
            }

            Archive archive = _opentokService.OpenTok.StartArchive(
                _opentokService.Session.Id,
                name: ".NET Archiving Sample App",
                hasAudio: hasAudio,
                hasVideo: hasVideo,
                outputMode: (form.OutputMode == "composed" ? OutputMode.COMPOSED : OutputMode.INDIVIDUAL)
            );
            return Ok(archive);
        }

        [HttpGet("stop/{id}")]
        public IActionResult Stop(string id)
        {
            Archive archive = _opentokService.OpenTok.StopArchive(id);
            return Ok(archive);
        }

        [HttpGet("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _opentokService.OpenTok.DeleteArchive(id);
            return Redirect("/history");
        }
    }
}
