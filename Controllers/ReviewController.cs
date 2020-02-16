using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BooksCatalogue.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System;


namespace BooksCatalogue.Controllers
{
    public class ReviewController : Controller
    {
        private string apiEndpoint = "https://smbooksapi.azurewebsites.net/api/";
        private readonly HttpClient client;
        HttpClientHandler clientHandler = new HttpClientHandler();

        public ReviewController() {
            // Use this client handler to bypass ssl policy errors
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
        }

        // GET: Review/AddReview/2
        public async Task<IActionResult> AddReview(int? bookId)
        {
            if (bookId == null)
            {
                return NotFound();
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint + "books/" + bookId);

            HttpResponseMessage response = await client.SendAsync(request);

            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    string responseString = await response.Content.ReadAsStringAsync();
                    var book = JsonSerializer.Deserialize<Book>(responseString);

                    ViewData["BookId"] = bookId;
                    return View("Add");
                case HttpStatusCode.NotFound:
                    return NotFound();
                default:
                    return ErrorAction("Error. Status code = " + response.StatusCode + ": " + response.ReasonPhrase);
            }
        }

        // TODO: Tambahkan fungsi ini untuk mengirimkan atau POST data review menuju API
        // POST: Review/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([Bind("Id,BookId,ReviewerName,Rating,Comment")] Review review)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            content.Add(new StringContent(review.BookId.ToString()), "bookid");
            content.Add(new StringContent(review.ReviewerName), "reviewername");
            content.Add(new StringContent(review.Rating.ToString()), "rating");
            content.Add(new StringContent(review.Comment), "comment");

            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiEndpoint + "Review");
            request.Content = content;
            HttpResponseMessage response = await client.SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Created:
                    return new RedirectResult("/Books/Details/" + review.BookId);
                default:
                    return ErrorAction("Error. Status code = " + response.StatusCode + "; " + response.ReasonPhrase);
            }
            return new RedirectResult("/Books/Details/" + review.BookId);
        }

        private ActionResult ErrorAction(string message)
        {
            return new RedirectResult("/Home/Error?message=" + message);
        }
    }
}