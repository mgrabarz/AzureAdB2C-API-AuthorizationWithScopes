using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EternalSolutions.Samples.B2C.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EternalSolutions.Samples.B2C.Common.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;

namespace EternalSolutions.Samples.B2C.Client.NotesServiceClient.Controllers
{
    public class NotesController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var token = await AcquireToken(new[] { $"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceReadNotesScope}" });

            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44397/api/notes/");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewData["Notes"] = JsonConvert.DeserializeObject<List<Note>>(content);
                return View();
            }
            else
                throw new Exception($"Status:{response.StatusCode}, Message:{response.ReasonPhrase}");
        }

        [HttpPost]
        public async Task<ActionResult> Create(string text)
        {
            var token = await AcquireToken(new [] { $"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceWriteNotesScope}" });

            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44397/api/notes/");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{ Text: \"" + text + "\"}", Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var token = await AcquireToken(new[] { $"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceWriteNotesScope}" });

            var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, "https://localhost:44397/api/notes/" + id);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> AcquireToken(string[] scopes)
        {
            string signedInUserID = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                Startup.Configuration["Authentication:AzureAdB2C:ClientId"],
                string.Format("https://login.microsoftonline.com/tfp/{0}/{1}", Startup.Configuration["Authentication:AzureAdB2C:TenantName"], Startup.Configuration["Authentication:AzureAdB2C:SignInPolicyName"]),
                Startup.Configuration["Authentication:AzureAdB2C:CallbackPath"],
                new ClientCredential(Startup.Configuration["Authentication:AzureAdB2C:ClientSecret"]),
                userTokenCache, null);

            AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.FirstOrDefault());
            return result.AccessToken;
        }
    }
}