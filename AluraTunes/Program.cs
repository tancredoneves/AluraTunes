using AluraTunes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluraTunes
{
    class Program
    {
        static void Main(string[] args)
        {
            //criar um contexto que recebe um DbEntities
            using (var contexto = new AluraTunesEntities())
            {
                //select simpre em um so tabela

                var query = from g in contexto.Generos
                            select g;

                foreach (var genero in query)
                {
                    Console.WriteLine("{0}\t{1}", genero.GeneroId, genero.Nome);
                }

                //select com join com duas tabelas

                var faixaEgenero = from g in contexto.Generos
                                   join f in contexto.Faixas
                                   on g.GeneroId equals f.GeneroId
                                   select new { f, g };

                //select top 10

                faixaEgenero = faixaEgenero.Take(10);

                //criar um log como é criada a query do select

                contexto.Database.Log = Console.WriteLine;

                foreach (var item in faixaEgenero)
                {
                    Console.WriteLine("{0}\t{1}", item.f.Nome, item.g.Nome);

                }


                //Com where

                var textoBusca = "Led";
                var queryA = from a in contexto.Artistas // join alb in contexto.Albums on a.ArtistaId equals alb.ArtistaId
                             where a.Nome.Contains(textoBusca)
                             select a; // new { NomeArtista = a.Nome, NomeAlbum = Alb.Titulo}
                //
                // com metodo para consulta simples
                //
                var query2 = contexto.Artistas.Where(a => a.Nome.Contains(textoBusca));
                //
                // sem o join, so com chaves estrangeiras 
                //
                var query3 = from alb in contexto.Albums
                             where alb.Artista.Nome.Contains(textoBusca)
                             select new
                             {
                                 NomeArtista = alb.Artista.Nome,
                                 NomeAlbum = alb.Titulo
                             };

                foreach (var artista in queryA)
                {
                    Console.WriteLine("{0}\t{1}", artista.ArtistaId, artista.Nome);
                }

                // Refiando consultas como metodo
                
                GetFaixas(contexto, "valorbuscaArtista", "ValorbuscaAlbum");

                // usando Count sintaxe Consulta
                var queryCount = from f in contexto.Faixas
                                 where f.Album.Artista.Nome == "Led Zeppelin"
                                 select f;

                //var quantidade = queryCount.Count();
                // usando Count sintaxe metodo
                var quantidade = contexto.Faixas
                                 .Count(f => f.Album.Artista.Nome == "Led Zeppelin");
                Console.WriteLine("Led Zeppelin tem {0}", quantidade);

                
                // totalizando uma consulta Linq SUM
                var querySum = from inf in contexto.ItemNotaFiscals
                               where inf.Faixa.Album.Artista.Nome == "Led Zeppelin" //evita 3 joins
                               select new { totalDoItem = inf.Quantidade * inf.PrecoUnitario };
                var totalDoArtista = querySum.Sum(q => q.totalDoItem);
                Console.WriteLine("total do artista:", totalDoArtista);

                //Group By e variaveis locais let

                var queryGroupBy = from inf in contexto.ItemNotaFiscals
                                   where inf.Faixa.Album.Artista.Nome == "Led Zeppelin" //evita 3 joins
                                   group inf by inf.Faixa.Album into agrupado
                                   let vendasPorAlbum = agrupado.Sum(a => a.Quantidade * a.PrecoUnitario)
                                   orderby vendasPorAlbum
                                   descending
                                   select new
                                   {
                                       TituloDoAlbum = agrupado.Key.Titulo,
                                       TotalProAlbum = vendasPorAlbum
                                   };
                foreach (var agrupado in queryGroupBy)
                {
                    Console.WriteLine("{0}\t{1}", agrupado.TituloDoAlbum.PadRight(40),
                                                  agrupado.TotalProAlbum);
                }




            }
            Console.ReadKey();
        }












        //criando um metodo de busca por artista ou album

        private static void GetFaixas(AluraTunesEntities contexto, string buscaArtista, string buscaAlbum)
        {
            var query4 = from f in contexto.Faixas
                         where f.Album.Artista.Nome.Contains(buscaArtista)
                         select f;
            ////Ordena por Titulo via sitaxes de consulta
            // var query4 = from f in contexto.Faixas where f.Album.Artista.Nome.Contains(buscaArtista) && (!string.IsNullOrEmpty(buscaAlbum) ? f.Album.Titulo.Contains(buscaAlbum): true) orderby f.Album.Titulo, f.Nome select f;

            // se album for não for nulo acrecenta o Where
            if (!string.IsNullOrEmpty(buscaAlbum))
            {
                query4 = query4.Where(q => q.Album.Titulo.Contains(buscaAlbum));
            }

            //Ordena por Titulo via sitaxes de metodo

            query4 = query4.OrderBy(q => q.Album.Titulo).ThenBy(q => q.Nome);

            foreach (var faixa in query4)
            {
                //PadRight(40) numero de caracteres
                Console.WriteLine("{0}\t{1}", faixa.Album.Titulo.PadRight(40), faixa.Nome);
            }
            
        }






    }
}
