using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            //Pegar as informações da session que são necessárias para que apareça os detalhes do meu usuário
            int id = int.Parse(HttpContext.Session.GetString("UsuarioID")!);
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            //Busquei o usuário que está logado (beatriz)
            Usuario UsuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == id)!;
            //Se não for encontrado ninguém
            if (UsuarioEncontrado == null){
                return NotFound();
            } 

            //Procurar o curso que meu UsuarioEncontrado está cadastrado
            Curso CursoEncontrado = context.Curso.FirstOrDefault(Curso => Curso.CursoID == UsuarioEncontrado.CursoID)!;

            //Verificar se o usuario possui ou nao o curso
            if (CursoEncontrado == null){
                // O usuário não possui curso cadastrado

                ViewBag.curso = "O usuário não possui curso cadastrado";
            } else {
                // O usuário possui o curso cadastrado

                ViewBag.curso = CursoEncontrado.Nome;
            }

            ViewBag.Nome = UsuarioEncontrado.Nome;
            ViewBag.Email = UsuarioEncontrado.Email;
            ViewBag.ZapZap = UsuarioEncontrado.Contato;
            ViewBag.DtNasc = UsuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");

            return View();
        }



        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}