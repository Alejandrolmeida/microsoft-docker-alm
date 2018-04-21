using System;
using System.Net.Http;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

namespace Azurebrains.Storage.ValetKey.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValetKeyController : Controller
    {
        #region Controllers
        // GET: api/ValetKey/container/fichero.jpg
        [HttpGet("{container}/{name}", Name ="Get(container,name)")]
        public StorageEntitySas Get(string container, string name)
        {
            try
            {
                var blobSas = GetSharedAccessReferenceForUpload(container, name);
                Trace.WriteLine(string.Format("Blob Uri: {0} - Shared Access Signature: {1}", blobSas.BlobUri, blobSas.Credentials));
                
                return blobSas;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw new HttpRequestException(ex.Message, ex);
            }
        }

        // GET: api/ValetKey/container
        [HttpGet("{container}", Name = "Get(container)")]
        public StorageEntitySas Get(string container)
        {
            try
            {
                var name = Guid.NewGuid().ToString();

                var blobSas = GetSharedAccessReferenceForUpload(container, name);
                Trace.WriteLine(string.Format("Blob Uri: {0} - Shared Access Signature: {1}", blobSas.BlobUri, blobSas.Credentials));

                return blobSas;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw new HttpRequestException(ex.Message, ex);
            }
        }
        #endregion

        #region Private Methods
        private StorageEntitySas GetSharedAccessReferenceForUpload(string containerName, string blobName)
        {
            var client = Azure.StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);

            var credentials = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Write,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30)
            });

            var EntitySAS = new StorageEntitySas {
                Credentials = credentials,
                BlobUri = blob.Uri,
                BlobUriSAS = blob.Uri.AbsoluteUri + credentials
            };

            return EntitySAS;
        }
        #endregion

        #region Entities
        public class StorageEntitySas
        {
            public string Credentials;
            public Uri BlobUri;
            public string BlobUriSAS;
        }
        #endregion 
    }
}
