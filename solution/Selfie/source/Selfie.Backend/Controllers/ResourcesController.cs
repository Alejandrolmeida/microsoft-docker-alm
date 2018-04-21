using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Selfie.Backend.Models;

namespace Selfie.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/Resources")]
    public class ResourcesController : Controller
    {
        #region MEMBERS
        readonly HttpClient client = new HttpClient();
        string ValetKeyAPI = "";
        string BlobAPI = "";
        string Container = "";
        #endregion

        #region CONSTRUCTORS
        public ResourcesController(IConfiguration configuration)
        {
            ValetKeyAPI = configuration["Storage:valetKeyServer"] + "/api/ValetKey/";
            BlobAPI = configuration["Storage:valetKeyServer"] + "/api/Blob/";
            Container = configuration["Storage:Container"];
        }
        #endregion

        #region CONTROLLERS
        [HttpPost]
        public IActionResult Resources([FromBody] JObject form)
        {
            try
            {
                if (form == null)
                {
                    return BadRequest();
                }

                // Create a Uri with SAS Token
                var blobName = Guid.NewGuid().ToString();
                var blobObj = GetBlobSas(new Uri(ValetKeyAPI + Container + "/" + blobName));

                var master = form.ToObject<Master>();
                master.TableToken = blobName;
                master.uriFile1a = "";
                master.uriFile1b = blobObj.BlobUriSAS;

                return Ok(master);
            }
            catch (Exception ex)
            {
                return BadRequest("Error al Recibir Formulario");
            }
        }

        // GET: api/Resources/container
        [HttpGet("{container}")]
        public Dictionary<string, List<string>> Resources([FromRoute] string container)
        {
            var lista = new Dictionary<string, List<string>>();

            var result = GetBlobs(Container);

            return lista;
        }
        #endregion

        #region METHODS
        private StorageEntitySas GetBlobSas(Uri blobUri)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = client.GetStringAsync(blobUri).Result;
            var result = JsonConvert.DeserializeObject<StorageEntitySas>(json);

            return result;
        }

        private async Task<HttpResponseMessage> GetBlobs([FromRoute] string container)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await client.GetAsync(BlobAPI + Container);
        }

        #endregion          
    }
}