using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EternalSolutions.Samples.B2C.Common.Contracts;

namespace EternalSolutions.Samples.B2C.Client.NotesServiceClient.Controllers
{
    public class NotesController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44397/api/notes/");
            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewData["Notes"] = JsonConvert.DeserializeObject<List<Note>>(content);
                return View();
            }
            else
                return
                    new RedirectResult("/Error?message=" + $"Status:{response.StatusCode}, Message:{response.ReasonPhrase}");
        }

        [HttpPost]
        public async Task<ActionResult> Create(string text)
        {
            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44397/api/notes/");
            request.Content = new StringContent("{ Text: \"" + text + "\"}", Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, "https://localhost:44397/api/notes/" + id);
            var response = await client.SendAsync(request);
            return RedirectToAction(nameof(Index));
        }
    }
}