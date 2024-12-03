using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            //Criar uma lista de livros
            List<Livro> ListaLivros = context.Livro.ToList();

            //Verificar se o livro tem reserva ou não
            var LivrosReservados = context.LivroReserva.ToDictionary(Livro => Livro.LivroID, Livror => Livror.DtReserva);

            ViewBag.Livros = ListaLivros;
            ViewBag.LivrosReservados = ListaLivros;

            return View();
        }

        [Route("Cadastro")]
        //Método que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin");

            ViewBag.Categorias = context.Categoria.ToList();

            //retorna a view de cadastro
            return View();
        }

        //Método para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {

            //Primeira Parte: Cadastrar um livro na tabela livro
            Livro novoLivro = new Livro();

            //O que meu usuário escrever no formulario será atribuido ao novoLivro
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();

            //Trabalhar com imagens:
            if (form.Files.Count > 0)
            {
                //Pimeiro passo:
                //Armazenamos o arquivo/foto enviado pelo usuário
                var arquivo = form.Files[0];

                //Segundo passo:
                //Criar variável do caminho da minha pasta para colocar as fotos dos livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");

                //Validaremos se a pasta que será armazenada as imagens, existe. Caso não exista. criaremos uma nova pasta
                if (!Directory.Exists(pasta))
                {
                    //Criar a pasta:
                    Directory.CreateDirectory(pasta);

                    //Terceiro Passo:
                    //Criar a variável para armazenar o caminho em que meu arquivo estará, alé do nome dele
                    var caminho = Path.Combine(pasta, arquivo.FileName);

                    using (var stream = new FileStream(caminho, FileMode.Create))
                    {
                        //Copiou o arquivo para o meu diretorio
                        arquivo.CopyTo(stream);
                    }

                    novoLivro.Imagem = arquivo.FileName;

            } else {
                    novoLivro.Imagem = "Padrão.png";
            }
            


            //img
            context.Livro.Add(novoLivro);
            context.SaveChanges();

            //Segunda Parte: Adicionar dentro da LivroCategoria a categoria que pertence ao novoLivro
            List<LivroCategoria> listaCategorias = new List<LivroCategoria>(); //Lista as categorias 

            //Array que possui as categorias selecionada pelo usuário
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');

            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;

                // Adicionamos o obj LivroCategoria dentro da lista ListaLivroCategorias
                listaCategorias.Add(livroCategoria);
            }
            //peguei a coleção de ListaLivroCategorias e coloquei na tabela LivroCategoria
            context.LivroCategoria.AddRange(listaCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");
        }
    }

    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View("Error!");
    // }
}