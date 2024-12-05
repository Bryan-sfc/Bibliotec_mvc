using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Bibliotec_mvc.Controllers
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
            List<Livro> listaLivros = context.Livro.ToList();

            //Verificar se o livro tem reserva ou não
            //ToDictionay(chave, valor)
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }

        [Route("Cadastro")]
        //Método que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();
            //Retorna a View de cadastro:
            return View();
        }

        // Método para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {

            //PRIMEIRA PARTE: Cadastrar um livro na tabela Livro
            Livro novoLivro = new Livro();

            //O que meu usuário escrever no formulário será atribuido ao novoLivro
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();

            //Trabalhar com imagens:

            if (form.Files.Count > 0)
            {
                //Primeiro passo:
                // Armazenaremos o arquivo/foto enviado pelo usuário
                var arquivo = form.Files[0];

                //Segundo passo:
                //Criar variavel do caminho da minha pasta para colocar as fotos dos livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                //Validaremos se a pasta que será armazenada as imagens, existe. Caso não exista, criaremos uma nova pasta.
                if (!Directory.Exists(pasta))
                {
                    //Criar a pasta:
                    Directory.CreateDirectory(pasta);
                }
                //Terceiro passo:
                //Criar a variavel para armazenar o caminho em que meu arquivo estara, além do nome dele
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    //Copiou o arquivo para o meu diretório
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;
            }
            else
            {
                novoLivro.Imagem = "padrao.png";
            }


            context.Livro.Add(novoLivro);
            context.SaveChanges();

            // SEGUNDA PARTE: É adicionar dentro de LivroCategoria a categoria que pertence ao novoLivro
            //Lista a tabela LivroCategoria:
            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();

            //Array que possui as categorias selecionadas pelo usuário
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');
            //Ação, terror, suspense
            //3, 5, 7

            foreach (string categoria in categoriasSelecionadas)
            {
                //string categoria possui a informação do id da categoria ATUAL selecionada.
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                //Adicionamos o obj livroCategoria dentro da lista listaLivroCategorias
                listaLivroCategorias.Add(livroCategoria);
            }

            //Peguei a coleção da listaLivroCategorias e coloquei na tabela LivroCategoria
            context.LivroCategoria.AddRange(listaLivroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Livro/Cadastro");

        }


        [Route("Editar/{id}")]
        public IActionResult Editar(int id)
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.CategoriasDoSistema = context.Categoria.ToList();

            // LivroID == 3

            //Buscar quem é o tal do id numero 3:
            Livro livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            //Buscar as categorias que o livroEncontrado possui
            var categoriasDoLivroEncontrado = context.LivroCategoria
            .Where(identificadorLivro => identificadorLivro.LivroID == id)
            .Select(livro => livro.Categoria)
            .ToList();

            //Quero pegar as informaçoes do meu livro selecionado e mandar para a minha View
            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = categoriasDoLivroEncontrado;

            return View();
        }

        [Route("Atualizar")]
        public IActionResult Atualizar(IFormCollection form, int id, IFormFile Imagem)
        {
            //Buscar um livro especifico pelo ID
            Livro LivroAtualizado = context.Livro.FirstOrDefault(Livro => Livro.LivroID == id)!;

            LivroAtualizado.Nome = form["Nome"];
            LivroAtualizado.Escritor = form["Escritor"];
            LivroAtualizado.Editora = form["Editora"];
            LivroAtualizado.Descricao = form["Desricao"];
            LivroAtualizado.Idioma = form["Idioma"];

            //Upload de imagem
            if (Imagem != null && Imagem.Length > 0)
            {
                //Definir o caminho da minha imagem
                var CaminhoImagem = Path.Combine("wwwroot/images/livros", Imagem.FileName);
                //Verificar se o usuario colocou uma imagem para atualizar o livro
                if (string.IsNullOrEmpty(LivroAtualizado.Imagem))
                {
                    //caso exista, ela irá sera apagada
                    var CaminhoImagemAntiga = Path.Combine("wwwroot/images/livros", LivroAtualizado.Imagem);
                    //ver se existe uma imagem no cmainho antigo
                    if (System.IO.File.Exists(CaminhoImagemAntiga))
                    {
                        System.IO.File.Delete(CaminhoImagemAntiga);
                    }
                }

                using(var stream = new FileStream(CaminhoImagem, FileMode.Create)){
                    Imagem.CopyTo(stream);
                }

                //Subir essa mudança para o meu banco de dados
                LivroAtualizado.Imagem = Imagem.FileName;
            }

            //Categorias:
            //Primeiro: Precisamos pegar as categorias selecionadas do usuário
            var categoriasSelecionadas = form["Categoria"].ToList();

            //Segundo: Pegaremos as categorias ATUAIS do livro
            var categoriasAtuais = context.LivroCategoria.Where(Livro => Livro.LivroID == id);

            //Terceiro: Removeremos as categorias antigas
            foreach(var categoria in categoriasAtuais){
                if(!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){
                    //Nos vamos remover a categoria do nosso context

                    context.LivroCategoria.Remove(categoria);
                }
            }

            //Quarto: Adicionaremos as novas categorias
            foreach(var categoria in categoriasSelecionadas){
                //Verificando se não existe a categoria nesse livro
                if(categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria)){
                    context.LivroCategoria.Add(new LivroCategoria{
                        LivroID = id,
                        CategoriaID = int.Parse(categoria)
                    });
                }
            }

            context.SaveChanges();

            return LocalRedirect("/livro");
        }

        //Método de excluir o livro
        [Route("Excluir")]
        public IActionResult Excluir(int id){
            //Buscar qual o livro do id que precisamos excluir
            Livro LivroEncontrado = context.Livro.FirstOrDefault(Livro => Livro.LivroID == id)!;

            //Buscar as categorias desse livro
            var CategoriasDoLivro = context.LivroCategoria.Where(Livro => Livro.LivroID == id).ToList();

            //Precisa excluir primeiro o registro da tabela intermediária
            foreach(var categoria in CategoriasDoLivro){
                context.LivroCategoria.Remove(categoria);
            }

            context.Livro.Remove(LivroEncontrado);

            context.SaveChanges();
            return LocalRedirect("/Livro");
        }
    }
}