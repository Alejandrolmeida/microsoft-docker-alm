using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Azurebrains.Storage.ValetKey.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class BlobController : Controller
    {
        #region MEMBERS
        CloudBlobClient client = Azure.StorageAccount.CreateCloudBlobClient();
        #endregion

        #region CONTROLLERS
        // GET: api/Blob/Container
        [HttpGet("{container}")]
        public Dictionary<string,List<string>> GetBlobs([FromRoute] string container)
        {
            var client = Azure.StorageAccount.CreateCloudBlobClient();
            var _containerImages = client.GetContainerReference(container);
            var _containerThumbs = client.GetContainerReference(container+"thumb");

            var list = new Dictionary<string, List<string>>();

            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = _containerImages.ListBlobsSegmentedAsync(token).Result;
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        list.Add(blob.Name,
                                 new List<string>() {
                                     blob.Uri.ToString(),
                                     blob.Uri.ToString().Replace(container, container + "thumb")
                                 });
                    }
                }
            } while (token != null);
            
            return list;
        }

        // DELETE: api/Blob/container/c94f3942-8ef7-4a14-88f8-411b471381b4
        [HttpDelete("{container}/{name}")]
        public async Task<IActionResult> DeleteBlob([FromRoute] string container, [FromRoute] string name)
        {
            // Buscamos el blob
            var client = Azure.StorageAccount.CreateCloudBlobClient();
            var _container = client.GetContainerReference(container);
            var _blob = _container.GetBlockBlobReference(name);

            // Creamos el contenedor si no existe
            await _container.CreateIfNotExistsAsync();

            // Eliminamos el blob
            await _blob.DeleteAsync();

            return NoContent();
        }
        #endregion
    }
}