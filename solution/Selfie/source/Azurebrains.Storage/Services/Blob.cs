using Azurebrains.Storage.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Azurebrains.Storage.Services
{
    public class Blob
    {
        #region Members
        private string _apiversion;
        private string _uriSas;
        private string _container;
        private string _name;
        private string _contentype;        
        private byte[] _content;
        private long _blockSize;
        private int _retentionPolicy;
        private Dictionary<string, byte[]> _blockList;
        private Dictionary<string, int> _blockStatusList;
        private Dictionary<string, int> _blockErrorList;
        #endregion

        #region Properties
        public string UriSas
        {
            get
            {
                return _uriSas;
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public string ContentType
        {
            get
            {
                return _contentype;
            }
        }
        public long BlockSize
        {
            get
            {
                return _blockSize;
            }
        }
        public byte[] Content
        {
            get
            {
                return _content;
            }
        }
        public Dictionary<string, byte[]> BlockList
        {
            get
            {
                return _blockList;
            }
        }
        public Dictionary<string, int> BlockStatusList
        {
            get
            {
                return _blockStatusList;
            }
        }
        #endregion

        #region Delegates & Events
        public delegate void BlockHandler(string name, string idBlock, byte[] content, int status);
        public delegate void BlockListHandler(string name, int status);
        public delegate void FileHandler(string name, byte[] content, int status);
        public event BlockHandler BlockCompleted;
        public event BlockListHandler BlockListCompleted;
        public event FileHandler FileCompleted;
        #endregion

        #region Handlers
        private void Blob_BlockCompleted(string name, string idBlock, byte[] content, int status)
        {
            _blockStatusList[idBlock] = status;

            if (status == 200)
            {
                if (_blockStatusList.Where(p => p.Value == 200)
                    .Count<KeyValuePair<string,int>>() != _blockStatusList.Count<KeyValuePair<string,int>>())
                {
                    return;
                }

                // Solo si estan todos los paquetes enviados correctamente lanzamos 
                // la funcion de envio de lista de paquetes                
                PutBlockList();
            }
            else
            {
                // Establecemos una politica de 3 reintentos
                if (_blockErrorList.Any(p => p.Key == idBlock))
                {
                    if (_blockErrorList[idBlock] >= _retentionPolicy)
                    {
                        // Enviamos un evento de error
                        _blockErrorList[idBlock] += 1;
                        return;
                    }
                    else
                    {
                        _blockErrorList[idBlock] += 1;
                        PutBlock(idBlock);
                    }
                }
                else
                {
                    _blockErrorList.Add(idBlock, 1);
                    PutBlock(idBlock);
                }
            }
        }
        private void Blob_BlockListCompleted(string name, int status)
        {
            if (status == 200)
            {
                // Enviamos un evento de finalizacion correcta
                FileCompleted(name, Content, 200);
            }
            else
            {
                // Enviamos un evento de error
                FileCompleted(name, Content, 500);
            }
        }
        private void Blob_FileCompleted(string name, byte[] content, int status)
        {

        }
        #endregion 

        #region Constructors        
        public Blob(string uriValetKey, string container, string name, string contentype, string filePath, long blockSize, string apiversion)
        {
            // Create a Uri with SAS Token
            var uriBlob = new Uri(uriValetKey + container + "/" + name);
            var result = GetBlobSas(uriBlob);

            _uriSas = result.BlobUriSAS;
            _container = container;
            _name = name;
            _contentype = contentype;
            _content = null;
            _blockSize = blockSize;
            _apiversion = apiversion;
            _retentionPolicy = 3;

            _blockList = GetBlocks(filePath, _blockSize);
            _blockStatusList = new Dictionary<string, int>();
            _blockErrorList = new Dictionary<string, int>();
            foreach (var block in _blockList)
            {
                _blockStatusList.Add(block.Key, 0);
            }

            BlockCompleted += new BlockHandler(Blob_BlockCompleted);
            BlockListCompleted += new BlockListHandler(Blob_BlockListCompleted);
            FileCompleted += new FileHandler(Blob_FileCompleted);
        }
        public Blob(string uriValetKey, string container, string name, string contentype, byte[] content, long blockSize, string apiversion)
        {
            // Create a Uri with SAS Token
            var uriBlob = new Uri(uriValetKey + container + "/" + name);
            var result = GetBlobSas(uriBlob);

            _uriSas = result.BlobUriSAS;
            _container = container;
            _name = name;
            _contentype = contentype;
            _content = content;
            _blockSize = blockSize;
            _apiversion = apiversion;
            _retentionPolicy = 3;

            _blockList = GetBlocks(_content, _blockSize);
            _blockStatusList = new Dictionary<string, int>();
            _blockErrorList = new Dictionary<string, int>();
            foreach (var block in _blockList)
            {
                _blockStatusList.Add(block.Key, 0);
            }

            BlockCompleted += new BlockHandler(Blob_BlockCompleted);
            BlockListCompleted += new BlockListHandler(Blob_BlockListCompleted);
            FileCompleted += new FileHandler(Blob_FileCompleted);            
        }
        public Blob(string uriValetKey, string container, string name, string contentype, byte[] content, long blockSize)
            : this(uriValetKey, container, name, contentype, content, 100, "2017-07-29")
        {
        }
        public Blob(string uriValetKey, string container, string name, string contentype, byte[] content)
            : this(uriValetKey, container, name, contentype, content, 100)
        {
        }
        #endregion

        #region Public Methods
        public void Put()
        {
            try
            {
                var dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);

                Uri uri = new Uri(_uriSas);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "PUT";
                request.Headers["cache-control"] = "no-cache";
                request.Headers["x-ms-blob-type"] = "BlockBlob";
                request.ContentType = _contentype;
                request.Headers["x-ms-date"] = dateInRfc1123Format;
                request.Headers["x-ms-version"] = _apiversion;

                using (Stream requestStream = request.GetRequestStreamAsync().Result)
                {
                    requestStream.Write(_content, 0, _content.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
                {
                    String ETag = response.Headers["ETag"];
                }

                FileCompleted(_name, _content, 200);
            }
            catch
            {
                FileCompleted(_name, _content, 500);
            }
        }
        public void PutBlock(string blockID)
        {
            try
            {
                var dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);

                Uri uri = new Uri(_uriSas + "&comp=block&blockid=" + WebUtility.UrlEncode(blockID));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "PUT";
                request.Headers["cache-control"] = "no-cache";
                request.Headers["x-ms-blob-type"] = "BlockBlob";
                request.ContentType = _contentype;
                request.Headers["x-ms-date"] = dateInRfc1123Format;
                request.Headers["x-ms-version"] = _apiversion;

                request.Headers["blockid"] = blockID;

                using (Stream requestStream = request.GetRequestStreamAsync().Result)
                {
                    requestStream.Write(_blockList[blockID], 0, _blockList[blockID].Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
                {
                    String ETag = response.Headers["ETag"];
                }

                BlockCompleted(_name, blockID, _blockList[blockID], 200);
            }
            catch (Exception ex)
            {                
                BlockCompleted(_name, blockID, _blockList[blockID], 500);
                Console.Write(ex.Message);
            }
        }
        public void PutBlockList()
        {
            try
            {
                var dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);

                Uri uri = new Uri(_uriSas + "&comp=blocklist");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "PUT";

                request.ContentType = "application/json;charset=UTF-8";
                request.Headers["x-ms-date"] = dateInRfc1123Format;
                request.Headers["x-ms-version"] = _apiversion;
                request.Headers["x-ms-blob-content-type"] = _contentype;

                StringBuilder content = new StringBuilder();
                content.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                content.Append("<BlockList>");
                foreach (var block in _blockList)
                {
                    content.Append("<Latest>" + block.Key + "</Latest>");
                }
                content.Append("</BlockList>");                

                var json = ASCIIEncoding.ASCII.GetBytes(content.ToString());

                using (Stream requestStream = request.GetRequestStreamAsync().Result)
                {
                    requestStream.Write(json, 0, content.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
                {
                    String ETag = response.Headers["ETag"];
                }

                BlockListCompleted(_name, 200);
            }
            catch
            {
                BlockListCompleted(_name, 500);
            }

        }
        public void PutInBlocks()
        {
            foreach (var block in _blockList)
            {
                PutBlock(block.Key);
            }
        }
        #endregion

        #region Private Methods
        private StorageEntitySas GetBlobSas(Uri blobUri)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = client.GetStringAsync(blobUri).Result;
            var result = JsonConvert.DeserializeObject<StorageEntitySas>(json);

            return result;
        }
        private Dictionary<string, byte[]> GetBlocks(byte[] file, long sizeInKB)
        {
            const int KB = 1024 * 1000;

            return GetBlocks(SplitByteArray(file, sizeInKB * KB));
        }
        private Dictionary<string, byte[]> GetBlocks(List<byte[]> list)
        {
            var blocks = list;
            var blockList = new Dictionary<string, byte[]>();
            long i = 0;

            foreach (var block in blocks)
            {
                i++;
                byte[] blockIdBytes = BitConverter.GetBytes(i);
                string blockIdBase64 = Convert.ToBase64String(blockIdBytes);
                blockList.Add(blockIdBase64, block);
            }

            return blockList;
        }
        private Dictionary<string, byte[]> GetBlocks(string filePath, long sizeInKB)
        {
            long readBytes = 0;
            var remainingBytes = new FileInfo(filePath).Length;
            var size = remainingBytes;
            var byteList = new List<byte[]>();
            var blockList = new Dictionary<string, byte[]>();

            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, "Blob"))
            {
                while (remainingBytes > 0)
                {
                    if (remainingBytes > Int32.MaxValue)
                        size = Int32.MaxValue / 2;
                    else
                        size = remainingBytes;

                    using (MemoryMappedViewStream stream = mmf.CreateViewStream(readBytes, size))
                    {
                        var reader = new BinaryReader(stream);
                        var bytes = reader.ReadBytes(Convert.ToInt32(stream.Length));

                        byteList.AddRange(SplitByteArray(bytes, sizeInKB));
                    }

                    readBytes += size;
                    remainingBytes -= size;
                }
                return GetBlocks(byteList);
            }
        }
        private List<byte[]> SplitByteArray(byte[] bytes, long range)
        {
            // Declare variables
            const long kb = 1024;
            var byteArrayList = new List<byte>();
            var arrayList = new List<byte[]>();
            
            // Loop all bytes
            for (long i = 1; i < bytes.Length; i++)
            {
                // Put the byte into the bytes array
                byteArrayList.Add(bytes[i - 1]);

                // If we match the range, add the byte array and create new one
                if (((i * kb) % range == 0))
                {
                    // Add as array
                    arrayList.Add(byteArrayList.ToArray());

                    // Create again
                    byteArrayList = new List<byte>();
                }
            }

            // Add the final byte array (which might not be full)
            arrayList.Add(byteArrayList.ToArray());

            // Return result
            return arrayList;
        }
        #endregion                
    }
}