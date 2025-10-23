using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Net.Mail;
using MimeKit;
using MailKit.Net.Smtp;

namespace LocationFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private const string emailFromId = "";
        private const string emailFromName = "";
        private const string emailFromPassword = "";

        public ImageController(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        // GET api/image/{photoId}
        [HttpGet("{photoId}")]
        public async Task<IActionResult> GetImage(string photoId)
        {
            var header = Request.Headers;
            header["RequestDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var headersJson = JsonConvert.SerializeObject(header.ToDictionary(
                h => h.Key,
                h => h.Value.ToString()
            ), Formatting.Indented);

            //var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress.ToString();
            //var userAgent = Request.Headers["User-Agent"].ToString();
            //var location = await GetUserLocationAsync(ipAddress);


            // Path to your images folder. You can store images inside the "wwwroot/images" directory
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "images", photoId + ".jpg");

            // Check if the file exists
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            // Return the image file
            var imageFile = System.IO.File.ReadAllBytes(imagePath);
            return File(imageFile, "image/jpeg"); // You can adjust MIME type based on the image type
        }

        //private async Task<string> GetUserLocationAsync(string ipAddress)
        //{
        //    var httpClient = new HttpClient();
        //    var response = await httpClient.GetStringAsync($"http://ipinfo.io/{ipAddress}/json");
        //    var jsonResponse = JObject.Parse(response);
        //    var location = jsonResponse["city"]?.ToString(); // You can change this to country, region, etc.
        //    return location;
        //}

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                var emailMessage = await CreateEmailMessage(emailFromName, emailFromId, emailRequest.toEmail, emailRequest.subject, emailRequest.detail, emailRequest.imageUrl);

                using (var client = new SmtpClient())
                {
                    // Connect to SMTP server (Gmail example, you can replace with your SMTP server)
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync(emailFromId, emailFromPassword); // Use app password if 2FA is enabled
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                return Ok(new { message = "Email sent successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Method to create email message
        private async Task<MimeMessage> CreateEmailMessage(string fromName, string fromEmail, string recipientEmail, string subject, string detail, string imageUrl)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            emailMessage.Subject = subject;

            // Create the HTML body with the image URL
            // $"<html><body><p>Here is the image loaded from the URL:</p><img src=\"{imageUrl}\" /></body></html>"
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = detail + $"<br/><img src=\"{imageUrl}\" />"
            };

            // Set the email body
            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }
    }

    public class EmailRequest
    {
        public required string toEmail { get; set; }
        public required string subject { get; set; }

        public required string detail { get; set; }
        public required string imageUrl { get; set; }
    }
}
