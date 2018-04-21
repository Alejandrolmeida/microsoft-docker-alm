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

namespace Azurebrains.Storage.ValetKey.Controllers
{
    [Produces("application/json")]
    [Route("api/Thumbnail")]
    public class ThumbnailController : Controller
    {
        #region MEMBERS
        CloudBlobClient blobclient = Azure.StorageAccount.CreateCloudBlobClient();
        CloudQueueClient queueClient = Azure.StorageAccount.CreateCloudQueueClient();
        CloudQueue queue;
        #endregion

        #region CONTROLLERS
        // GET: api/Thumbnail/container/c94f3942-8ef7-4a14-88f8-411b471381b4
        [HttpGet("{container}/{name}")]
        public async Task<IActionResult> CreateThumbnail([FromRoute] string container, [FromRoute] string name)
        {
            // Find blob            
            var _container = blobclient.GetContainerReference(container);
            var _blob = _container.GetBlockBlobReference(name);
            if (!_blob.ExistsAsync().Result) return NotFound();

            // Get original image from blob
            byte[] blobBytes = await DownloadFile(_blob.Uri.AbsoluteUri);
            //var result = await _blob.DownloadToByteArrayAsync(blobBytes, 0);
            try
            {
                var uri = "";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    uri = await CreateThumbnailWindows(blobBytes, container + "thumb", name);
                }
                else
                {
                    uri = await CreateThumbnailLinux(blobBytes, container + "thumb", name);
                }
                return Ok(uri);
            }
            catch
            {
                return NotFound();
            }
        }
        #endregion

        #region METHODS
        private async Task<string> CreateThumbnailLinux(byte[] imageBytes, string container, string blobname)
        {
            try
            {
                var size = "100";                
                // Creamos un fichero con la imagen original
                var originalPath = Path.GetTempFileName();
                await System.IO.File.WriteAllBytesAsync(originalPath, imageBytes);

                // Detectamos el formato del fichero
                var ext = imageBytes.GetFileType().Extension;
                FileInfo file = new FileInfo(originalPath);
                file.MoveTo(originalPath.Replace(".tmp", "." + ext));

                // Definimos los nombres
                originalPath = file.FullName;
                var thumbnailPath = originalPath.Replace("." + ext, ".thumb." + ext);

                // Creamos un thumbnail desde Bash
                var script = "convert -thumbnail " + size + " " + originalPath + " " + thumbnailPath;
                var output = script.Bash();

                // Enviamos el thumbanil al blob
                string uri = await UpdloadFile(thumbnailPath, container, blobname);

                if (System.IO.File.Exists(originalPath))
                {
                    System.IO.File.Delete(originalPath);
                }
                if (System.IO.File.Exists(thumbnailPath))
                {
                    System.IO.File.Delete(thumbnailPath);
                }

                return uri;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Este metodo funciona solamente en Windows (no usar en contenedores linux)
        /// </summary>
        private async Task<string> CreateThumbnailWindows(byte[] imageBytes, string container, string blobname)
        {
            try
            {
                const int size = 75;
                const int quality = 90;

                var thumbnailPath = Path.GetTempFileName();


                if (imageBytes != null)
                {
                    //using (var image = new Bitmap(System.Drawing.Image.FromFile(inputPath)))
                    var stream = new MemoryStream(imageBytes);
                    await System.IO.File.WriteAllBytesAsync(thumbnailPath, imageBytes);
                    var bmp = Image.FromFile(thumbnailPath);

                    //using (var image = new Bitmap(Image.FromStream(new MemoryStream(imageBytes))))
                    using (var image = new Bitmap(bmp))
                    {
                        var imgPalette = image.Palette;

                        var imgSize = image.Width;
                        Bitmap croppedImage;
                        if (image.Width != image.Height)
                        {
                            imgSize = Math.Min(image.Width, image.Height);
                            croppedImage = new Bitmap(imgSize, imgSize);
                            var croppedRect = new Rectangle(0, 0, imgSize, imgSize);
                            using (Graphics g = Graphics.FromImage(croppedImage))
                            {
                                g.DrawImage(image, new Rectangle(0, 0, imgSize, imgSize), croppedRect, GraphicsUnit.Pixel);
                            }
                        }
                        else
                        {
                            croppedImage = image;
                        }
                        var croppedImagePalette = croppedImage.Palette;

                        var resized = new Bitmap(size, size);
                        var resizedPalette = resized.Palette;
                        using (var graphics = Graphics.FromImage(resized))
                        {
                            graphics.CompositingQuality = CompositingQuality.HighSpeed;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.DrawImage(croppedImage, 0, 0, size, size);
                            using (var output = System.IO.File.Open(thumbnailPath, FileMode.Create))
                            {
                                resized.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                        }
                    }
                }

                string uri = await UpdloadFile(thumbnailPath, container, blobname);

                if (System.IO.File.Exists(thumbnailPath))
                {
                    System.IO.File.Delete(thumbnailPath);
                }

                return uri;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<string> UpdloadFile(string filePath, string container, string name)
        {
            // Get a blob reference
            var _container = blobclient.GetContainerReference(container);
            await _container.CreateIfNotExistsAsync();
            var blob = _container.GetBlockBlobReference(name);

            // Upload file to blob
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filePath);

            return blob.Uri.AbsoluteUri;
        }

        public static async Task<byte[]> DownloadFile(string url)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                }
            }
            return null;
        }
        #endregion
    }
}