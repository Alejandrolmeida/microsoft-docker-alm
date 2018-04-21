using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MimeDetective;
using Azurebrains.Storage.ValetKey.Helpers;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Azurebrains.Storage.ValetKey.Controllers
{
    [Produces("application/json")]
    [Route("api/Thumbnail")]
    public class ThumbnailController : Controller
    {
    {
        #region MEMBERS
        CloudQueueClient queueClient = Azure.StorageAccount.CreateCloudQueueClient();
        CloudQueue queue;
        #endregion

        #region CONSTRUCTORS
        public ThumbnailController(IConfiguration configuration)
        {
            Container = configuration["Storage:Container"];

            // Retrieve a reference to a queue.
            queue = queueClient.GetQueueReference(Container);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExistsAsync();
        }
        #endregion

        #region CONTROLLERS
        // GET: api/Thumbnail/container/c94f3942-8ef7-4a14-88f8-411b471381b4
        [HttpGet("{container}/{name}")]
        public async Task<IActionResult> SendCreateThumbnail([FromRoute] string container, [FromRoute] string name)
        {
            try
            {
                var obj = new Dictionary<string, string>()
                {
                    { "action", "create" },
                    { "container", container },
                    { "name", name }
                };

                string json = JsonConvert.SerializeObject(obj);

                CloudQueueMessage message = new CloudQueueMessage(json);
                await queue.AddMessageAsync(message);

                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }
        #endregion
    }
}