using System;
using System.Collections.Generic;
using Azurebrains.Storage.Services;

namespace Azurebrains.Storage
{
    public class Client
    {
        #region Members
        private Dictionary<Guid, List<Blob>> _packages;
        private string _URI_SAS_Service;
        #endregion

        #region Properties
        public Dictionary<Guid, List<Blob>> Packages
        {
            get
            {
                return _packages;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Blob Service Client
        /// </summary>
        /// <param name="URI_SAS_Service">Endpoint of SAS Service</param>
        public Client(string URI_SAS_Service) 
        {
            _packages = new Dictionary<Guid, List<Blob>>();
            _URI_SAS_Service = URI_SAS_Service;
        }
        public Client()
        {                
        }
        #endregion

        #region Public methods
        public List<Blob> Load(string container, Dictionary<string, byte[]> files)
        {
            return Load(container, files, 100);
        }
        public List<Blob> Load(string container, Dictionary<string, byte[]> files, long blockSize)
        {
            return Load(container, files, blockSize, "2017-07-29");
        }
        /// <summary>
        /// This method load sender files and create a blob write SAS token for each one.
        /// </summary>
        /// <param name="files">A list with the names and content of sender files</param>
        /// <returns></returns>
        public List<Blob> Load(string container, Dictionary<string, byte[]> files, long blockSize, string apiVersion)
        {
            try
            {
                // Create a blob package with files information 
                var blobList = new List<Blob>();
                foreach (var file in files)
                {
                    var blob = new Blob(_URI_SAS_Service,
                                        container,
                                        file.Key,
                                        "image/png",
                                        file.Value,
                                        blockSize,
                                        apiVersion);
                    blobList.Add(blob);
                }

                var packageID = new Guid();
                _packages.Add(packageID, blobList);

                return blobList;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }
        /// <summary>
        /// This method load sender files and create a blob write SAS token for each one.
        /// </summary>
        /// <param name="files">A list with the names and filepath of sender files</param>
        /// <returns></returns>
        public List<Blob> Load(string container, Dictionary<string, string> files)
        {
            return Load(container, files, 100);
        }
        public List<Blob> Load(string container, Dictionary<string, string> files, long blockSize)
        {
            return Load(container, files, blockSize, "2017-07-29");
        }
        public List<Blob> Load(string container, Dictionary<string, string> files, long blockSize, string apiVersion)
        {
            try
            {
                // Create a blob package with files information 
                var blobList = new List<Blob>();
                foreach (var file in files)
                {
                    var blob = new Blob(_URI_SAS_Service,
                                        container,
                                        file.Key,
                                        "image/png",
                                        file.Value,
                                        blockSize,
                                        apiVersion);
                    blobList.Add(blob);
                }

                var packageID = Guid.NewGuid();
                _packages.Add(packageID, blobList);

                return blobList;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }

        /// <summary>
        /// Send file to Azure Blob
        /// </summary>
        /// <param name="container">Name of container</param>
        /// <param name="name">Name of file</param>
        /// <param name="content">File content in bytes</param>
        public void Send(string container, string name, byte[] content)
        {
            var files = new Dictionary<string, byte[]>()
            {
                { name, content}
            };
            
            Send(Load(container, files));
        }
        /// <summary>
        /// Send list of files to Azure Blobs
        /// </summary>
        /// <param name="blobs"></param>
        public void Send(List<Blob> blobs)
        {
            try
            {
                foreach (var blob in blobs)
                {
                    blob.PutInBlocks();
                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }
        #endregion
    }
}
