using System;
using System.Linq;
using System.Xml.Linq;

namespace bibliotechna_systema
{
    // Розробити засоби для бiблiографiчної системи (БС).
    // Книга в БС характеризується числовим iдентифiкатором, прiзвищем та iнiцiалами автора i назвою.
    // Жанр характеризується числовим iдентифiкатором та назвою.
    // Видавництво характеризується стрiнговим iдентифiкатором та повною назвою.
    // Усi перелiченi данi (характеристики) задано окремими xml-файлами.
    // Окремий запис про книгу в БС мiстить числовий iдентифiкатор книги, iдентифiкатор жанру, iден-
    // тифiкатор видавництва i дату надходження. Записи звiту задано xml-файлом.

    internal class Program
    {
        public static void Main(string[] args)
        {
            var path_books = "../../book.xml";
            var path_checks = "../../check.xml";
            var path_vydavnytstvo = "../../vydavnytstvo.xml";
            var path_zhanr = "../../zhanr.xml";

            var books = XElement.Load(path_books);
            var checks = XElement.Load(path_checks);
            var vydavnytstvo = XElement.Load(path_vydavnytstvo);
            var zhanr = XElement.Load(path_zhanr);


            var all_books = books.Elements("Book").Select(w => new
            {
                BookId = (int)w.Element("BookId"),
                AuthorName = (string)w.Element("AuthorName"),
                AuthorSurname = (string)w.Element("AuthorSurname"),
                BookName = (string)w.Element("BookName")
            });

            var all_checks = checks.Elements("Check").Select(p => new
            {
                BookId = (int)p.Element("BookId"),
                ZhanrId = (int)p.Element("ZhanrId"),
                VydavnytstvoId = (int)p.Element("VydavnytstvoId"),
                Date = (DateTime)p.Element("Date")
            });

            var all_vydavnytstvas = vydavnytstvo.Elements("Vydavnytstvo").Select(w => new
            {
                VydavnytstvoId = (int)w.Element("VydavnytstvoId"),
                VydavnytstvoName = (string)w.Element("VydavnytstvoName")
            });

            var all_zhanrs = zhanr.Elements("Zhanr").Select(w => new
            {
                ZhanrId = (int)w.Element("ZhanrId"),
                ZhanrName = (string)w.Element("ZhanrName")
            });

            foreach (var i in all_books) { Console.WriteLine(i); }
            foreach (var i in all_checks) { Console.WriteLine(i); }
            foreach (var i in all_vydavnytstvas) { Console.WriteLine(i); }
            foreach (var i in all_zhanrs) { Console.WriteLine(i); }

            // task A
            // xml-файл, де iнформацiя про книги подана в такому виглядi:
            // <прiзвище та iнiцiали автора, перелiк назв його книг з вiдповiдними повними назвами видавництв>;
            // вмiст впорядкувати у лексико-графiчному порядку за прiзвищем та назвою книги;

            var queryForTaskA = from check in all_checks
                                join vyd in all_vydavnytstvas on check.VydavnytstvoId equals vyd.VydavnytstvoId
                                join book in all_books on check.BookId equals book.BookId
                                join zh in all_zhanrs on check.ZhanrId equals zh.ZhanrId
                                orderby book.AuthorName, book.BookName
                                group check by new { book.AuthorSurname } into gr
                                select new
                                {
                                    AuthorsName = gr.Key.AuthorSurname,
                                    Books = gr.Select(g => new { Books = all_books.FirstOrDefault(s => s.BookId == g.BookId), g.VydavnytstvoId })
                                };

            var XTask_a = new XElement("TaskA",
                from item in queryForTaskA
                select new XElement("Author",
                new XAttribute("AuthorSurname", item.AuthorsName),
                from w in item.Books
                select new XElement("Book",
                new XElement("BookName", w.Books.BookName),
                new XElement("VydavnytstvoId", w.VydavnytstvoId)
                )));

            XTask_a.Save("../../taskA.xml");

            // task B
            // xml-файл, описаний у попередньому завданнi, але для автора подати лише одну книжку – яка надiйшла останньою;
            // вмiст впорядкувати за зазначеною датою, починаючи вiд найближчої;

            var queryForTaskB = from check in all_checks
                                join vyd in all_vydavnytstvas on check.VydavnytstvoId equals vyd.VydavnytstvoId
                                join book in all_books on check.BookId equals book.BookId
                                join zh in all_zhanrs on check.ZhanrId equals zh.ZhanrId
                                orderby book.AuthorName, check.Date
                                group check by new { book.AuthorSurname } into gr
                                select new
                                {
                                    AuthorsName = gr.Key.AuthorSurname,
                                    Books = gr.Select(g => new { Books = all_books.FirstOrDefault(s => s.BookId == g.BookId), g.VydavnytstvoId, g.Date }).OrderByDescending(x => x.Date).ToList()
                                };

            var XTask_b = new XElement("TaskB",
                from item in queryForTaskB
                select new XElement("Author",
                new XAttribute("AuthorSurname", item.AuthorsName),
                new XElement("LatestBookDate", item.Books[0].Date),
                new XElement("LatestBookVydavnytstvoId", item.Books[0].VydavnytstvoId)
                ));

            XTask_b.Save("../../taskB.xml");

            // task C
            // xml-файл, в якому для кожного видавництва (заданого повною назвою) вказати перелiк авторiв отриманих книг;
            // вмiст впорядкувати у лексико-графiчному порядку за повною назвою видавництва, а також прiзвищем;

            var queryForTaskC = from check in all_checks
                                join vyd in all_vydavnytstvas on check.VydavnytstvoId equals vyd.VydavnytstvoId
                                join book in all_books on check.BookId equals book.BookId
                                join zh in all_zhanrs on check.ZhanrId equals zh.ZhanrId
                                orderby vyd.VydavnytstvoName, book.AuthorSurname
                                group check by new { vyd.VydavnytstvoName } into gr
                                select new
                                {
                                    Vydavnytstvo = gr.Key.VydavnytstvoName,
                                    Authors = gr.Select(g => new { AuthorsList = all_books.FirstOrDefault(s => s.AuthorSurname == s.AuthorSurname) })
                                };

            var XTask_C = new XElement("TaskC",
                from item in queryForTaskC
                select new XElement("Vydavnytstvo",
                new XAttribute("VydavnytstvoName", item.Vydavnytstvo),
                from w in item.Authors
                select new XElement("Authors",
                new XElement("AuthorsName", w.AuthorsList.AuthorName)
                )));

            XTask_C.Save("../../taskC.xml");

            // task D
            // xml-файл з перелiком назв жанрiв з найбiльшою кiлькiстю книг; цi результати впорядкувати
            // за назвою жанру у лексико-графiчному порядку.

            var queryForTaskD = from check in all_checks
                                join vyd in all_vydavnytstvas on check.VydavnytstvoId equals vyd.VydavnytstvoId
                                join book in all_books on check.BookId equals book.BookId
                                join zh in all_zhanrs on check.ZhanrId equals zh.ZhanrId
                                orderby zh.ZhanrName
                                group check by new { zh.ZhanrName } into gr
                                select new
                                {
                                    ZhName = gr.Key.ZhanrName,
                                    TotalBooks = gr.Count()
                                    //Authors = gr.Select(g => new { AuthorsList = all_books.FirstOrDefault(s => s.AuthorSurname == s.AuthorSurname) })
                                };

            var max_books = (from item in queryForTaskD
                             select item.TotalBooks).Max();

            var XTask_D = new XElement("TaskD",
                from item in queryForTaskD
                where item.TotalBooks == max_books
                select new XElement("Zhanr",
                    new XAttribute("ZhanrName", item.ZhName),
                    new XElement("TotalCount", item.TotalBooks))
                );

            XTask_D.Save("../../taskD.xml");
        }
    }
}