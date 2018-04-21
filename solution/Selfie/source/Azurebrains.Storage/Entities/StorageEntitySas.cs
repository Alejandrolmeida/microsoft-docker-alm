using System;
using System.Collections.Generic;
using System.Text;

namespace Azurebrains.Storage.Entities
{
    public class StorageEntitySas
    {
        public string Credentials;
        public Uri BlobUri;
        public string BlobUriSAS;
    }
}
