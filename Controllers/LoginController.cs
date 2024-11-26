using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec.Controllers
{
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            return View();
        }

        [Route("controller")]
        public IActionResult Logar(IFormCollection form)
        {
            string emailInformado = form["Email"];
            string senhaInformada = form["Senha"];


            Usuario UsuarioBuscado = context.Usuario.FirstOrDefault
            (Usuario => Usuario.Email == emailInformado && Usuario.Senha == senhaInformada)!;

            // List<Usuario> ListaUsuario = context.Usuario.ToList();

            // foreach(Usuario usuario_ in ListaUsuario){
            //     if.(usuario_.Email == emailInformado && usuario__.Senha == senhaInformada){
            //         //usuario logado
            //     } else {
            //         //usuario nao encontrado
            //     }
            // }

            if(UsuarioBuscado == null){
                Console.WriteLine($"Dados Inválidos!");
                return LocalRedirect("~/Login");
            } else {
                Console.WriteLine($"Eba você entrou!");
                return LocalRedirect("~/Livro");
            }


            return View();
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}