using Broadcasting;
using Broadcasting.Models;
using Microsoft.AspNetCore.Mvc;
using OpenTokSDK;
using OpenTokSDK.Util;
using System;
using System.Collections.Generic;
using System.Dynamic;
using static OpenTokSDK.Broadcast;

namespace HelloWorld.Controllers
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
            locals.Token = _opentokService.Session.GenerateToken(Role.PUBLISHER, 0, null, new List<string>(new string[] { "focus" }));
            locals.InitialBroadcastId = _opentokService.broadcastId;
            locals.FocusStreamId = _opentokService.focusStreamId;
            locals.InitialLayout = OpenTokUtils.convertToCamelCase(_opentokService.layout.ToString());


            return View(locals);
        }

        [HttpGet("participant")]
        public IActionResult Participant()
        {
            dynamic locals = new ExpandoObject();

            locals.ApiKey = _opentokService.OpenTok.ApiKey.ToString();
            locals.SessionId = _opentokService.Session.Id;
            locals.Token = _opentokService.Session.GenerateToken();
            locals.FocusStreamId = _opentokService.focusStreamId;
            locals.Layout = OpenTokUtils.convertToCamelCase(_opentokService.layout.ToString());


            return View(locals);
        }

        [HttpPost("start")]
        public IActionResult Start(StartRequestModel form)
        {
            bool horizontal = form.Layout == "horizontalPresentation";
            BroadcastLayout layoutType = new BroadcastLayout(horizontal ? BroadcastLayout.LayoutType.HorizontalPresentation : BroadcastLayout.LayoutType.VerticalPresentation);
            int maxDuration = 7200;
            if (form.MaxDuration != null)
            {
                maxDuration = int.Parse(form.MaxDuration);
            }
            Broadcast broadcast = _opentokService.OpenTok.StartBroadcast(
                _opentokService.Session.Id,
                hls: true,
                rtmpList: null,
                resolution: form.Resolution,
                maxDuration: maxDuration,
                layout: layoutType
            );
            _opentokService.broadcastId = broadcast.Id.ToString();
            return Ok(broadcast);
        }

        [HttpPost("stop/{id}")]
        public IActionResult Stop([FromRoute]string id)
        {
            Broadcast broadcast = _opentokService.OpenTok.StopBroadcast(id);
            _opentokService.broadcastId = "";
            return Ok(broadcast);
        }

        [HttpGet("broadcast")]
        public IActionResult Broadcast()
        {
            if (!string.IsNullOrEmpty(_opentokService.broadcastId))
            {
                try
                {
                    Broadcast broadcast = _opentokService.OpenTok.GetBroadcast(_opentokService.broadcastId);
                    if (broadcast.Status == BroadcastStatus.STARTED)
                    {
                        return Redirect(broadcast.Hls);
                    }
                    else
                    {
                        return Ok("Broadcast not in progress.");
                    }
                }
                catch (Exception ex)
                {
                    return Ok("Could not get broadcast " + _opentokService.broadcastId + ". Exception Message: " + ex.Message);
                }
            }
            else
            {
                return Ok("There's no broadcast running right now.");
            }
        }

        [HttpGet("broadcast/{id}/layot/{layout}")]
        public IActionResult BroadcastWithLoayout(string id, string layout)
        {
            bool horizontal = layout == "horizontalPresentation";
            BroadcastLayout broadcastLayout = new BroadcastLayout(horizontal ? BroadcastLayout.LayoutType.HorizontalPresentation : BroadcastLayout.LayoutType.VerticalPresentation);
            _opentokService.OpenTok.SetBroadcastLayout(id, broadcastLayout);
            return Ok();
        }

        [HttpPost("focus")]
        public IActionResult Focus([FromBody]dynamic form)
        {
            string focusStreamId = form.Focus;
            _opentokService.focusStreamId = focusStreamId;
            StreamList streamList = _opentokService.OpenTok.ListStreams(_opentokService.Session.Id);
            List<StreamProperties> streamPropertiesList = new List<StreamProperties>();
            foreach (Stream stream in streamList)
            {
                StreamProperties streamProperties = new StreamProperties(stream.Id, null);
                if (focusStreamId.Equals(stream.Id))
                {
                    streamProperties.addLayoutClass("focus");
                }
                streamPropertiesList.Add(streamProperties);
            }
            _opentokService.OpenTok.SetStreamClassLists(_opentokService.Session.Id, streamPropertiesList);
            return Ok();
        }
    }
}
