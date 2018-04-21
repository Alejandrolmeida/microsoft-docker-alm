using System;
using System.ComponentModel.DataAnnotations;

namespace Selfie.Backend.Models
{
   public class Master
    {
        public string UserID { get; set; }
        public bool ValidID { get; set; }
        public string email { get; set; }
        public bool ValidEmail { get; set; }
        public string telefono { get; set; }
        public string uriForm { get; set; }
        public string TableToken { get; set; }
        public string uriFile1a { get; set; }
        public string uriFile1b { get; set; }
        public bool uriFile1_result { get; set; }
        public string uriFile2a { get; set; }
        public string uriFile2b { get; set; }
        public bool uriFile2_result { get; set; }
        public string browser { get; set; }
    }

    public class StorageEntitySas
    {
        public string Credentials;
        public Uri BlobUri;
        public string BlobUriSAS;
    }
}