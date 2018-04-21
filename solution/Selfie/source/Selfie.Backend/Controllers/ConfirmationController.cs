using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Selfie.Backend.Models;

namespace Selfie.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/Confirmation")]
    public class ConfirmationController : Controller
    {
        #region MEMBERS
        private readonly BootCampContext _context;
        readonly HttpClient client = new HttpClient();
        string DeleteBlobAPI = "";
        string CreateThumbAPI = "";
        string Container = "";
        #endregion

        #region CONSTRUCTORS
        public ConfirmationController(IConfiguration configuration, BootCampContext context)
        {
            _context = context;

            Container = configuration["Storage:Container"];
            DeleteBlobAPI = configuration["Storage:valetKeyServer"] + "/api/Blob";
            CreateThumbAPI = configuration["Storage:valetKeyServer"] + "/api/Thumbnail/" + configuration["Storage:Container"] + "/";
        }
        #endregion

        #region CONTROLLERS
        [HttpPost]
        public async Task<IActionResult> Confirmation([FromBody] JObject form)
        {
            try
            {
                if (form == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Cargamos los datos de la cabecera
                var headers = new List<string>() { "", "" };
                headers[0] = Request.Host.Host;
                //headers[1] = GetClientIpAddress(Request.Headers);

                // Cargamos los datos de formulario
                var master = form.ToObject<Master>();

                var assistant = await _context
                    .Assistants.SingleOrDefaultAsync(db => 
                        db.Email.ToLower().Trim() == master.email.ToLower().Trim());

                if (assistant == null)
                {
                    // Creamos un nuevo registro
                    assistant = new Assistant();
                    assistant.Name = master.UserID;
                    assistant.Email = master.email;
                    assistant.Telephone = master.telefono;
                    assistant.Blobname = master.TableToken;

                    _context.Assistants.Add(assistant);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    HttpResponseMessage result = DeleteBlob(Container, assistant.Blobname).Result;                    
                    if (!result.IsSuccessStatusCode)
                    {
                        //Enviar un correo a soporte indicando que no se ha podido eliminar el blob antiguo
                        Console.WriteLine("Warning: No se ha podido eliminar el blob " + assistant.Blobname);
                    }
                    result = DeleteBlob(Container + "thumb", assistant.Blobname).Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        //Enviar un correo a soporte indicando que no se ha podido eliminar el blob antiguo
                        Console.WriteLine("Warning: No se ha podido eliminar el thumbnail " + assistant.Blobname);
                    }

                    assistant.Name = master.UserID;                    
                    assistant.Telephone = master.telefono;
                    assistant.Blobname = master.TableToken;

                    // Actualizamos el registro existente
                    _context.Entry(assistant).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!(_context.Assistants.Any(bd => bd.Id == assistant.Id)))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                
                CreateThumbnail(assistant.Blobname);                

                return CreatedAtAction("GetAssistant", "Assistants", new { id = assistant.Id }, assistant);
            }
            catch (Exception ex)
            {
                return BadRequest("Error al Recibir Formulario");
            }
        }

        private void SendCreateThumbnail()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region METHODS
        private Task<HttpResponseMessage> CreateThumbnail(string blobname)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client.GetAsync(CreateThumbAPI + blobname);
        }

        private async Task<HttpResponseMessage> DeleteBlob(string container, string blobname)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await client.DeleteAsync(DeleteBlobAPI + "/" + container + "/" + blobname);
        }
        #endregion    
    }
}