using System;
using System.Linq;
using System.Xml.Linq;

namespace Program
{
    // Розробити засоби для звiтування за розрозробку проєктiв.
    // Працiвник характеризується iндивiдуальним номером, прiзвищем та iм’ям, назвою пiдроздiлу ком-
    // панiї та числовим iдентифiкатором посади.
    // Посада характеризується числовим iдентифiкатором, назвою, та оплатою за 1 год працi.
    // Проєкт характеризується стрiнговим iдентифiкатором та назвою.
    // Усi перелiченi данi (характеристики) задано окремими xml-файлами.
    // Окремий запис звiту мiстить дату, iндивiдуальний номер працiвника, iдентифiкатор проєкту, кiль-
    // кiсть вiдпрацьованих годин, змiст виконаної роботи (один стрiнг). Записи звiту задано xml-файлом.

    internal class Program
    {
        public static void Main(string[] args)
        {
            var path_workers = "../../worker.xml";
            var path_project = "../../project.xml";
            var path_posada = "../../posada.xml";
            var path_checks = "../../checks.xml";

            var workers = XElement.Load(path_workers);
            var projects = XElement.Load(path_project);
            var posadas = XElement.Load(path_posada);
            var checks = XElement.Load(path_checks);


            var all_projects = projects.Elements("Project").Select(pr => new
            {
                ProjectId = (int)pr.Element("ProjectId"),
                ProjectName = (string)pr.Element("ProjectName")
            });

            var all_posadas = posadas.Elements("Posada").Select(po => new
            {
                PosadaId = (int)po.Element("PosadaId"),
                PosadaName = (string)po.Element("Name"),
                PosadaPrice = (uint)po.Element("Price")
            });

            var all_workers = workers.Elements("Worker").Select(wo => new
            {
                WorkerId = (int)wo.Element("WorkerId"),
                WorkerName = (string)wo.Element("Name"),
                WorkerSurname = (string)wo.Element("Surname"),
                WorkerPosadaId = (int)wo.Element("PosadaId"),
                Pidrozdil = (string)wo.Element("Pidrozdil")
            });

            var all_checks = checks.Elements("Check").Select(ch => new
            {
                Date = (DateTime)ch.Element("Date"),
                WorkerId = (int)ch.Element("WorkerId"),
                ProjectId = (int)ch.Element("ProjectId"),
                WorkedHours = (uint)ch.Element("Hours"),
                Zmist = (string)ch.Element("Zmist")
            });

            foreach (var project in all_projects)
            {
                Console.WriteLine($"Project ID: {project.ProjectId}, Project Name: {project.ProjectName}");
            }
            foreach (var posada in all_posadas)
            {
                Console.WriteLine($"Posada ID: {posada.PosadaId}, Posada Name: {posada.PosadaName}, Posada Price: {posada.PosadaPrice}");
            }
            foreach (var worker in all_workers)
            {
                Console.WriteLine($"Worker ID: {worker.WorkerId}, Name: {worker.WorkerName}, Surname: {worker.WorkerSurname}, Posada ID: {worker.WorkerPosadaId}, Pidrozdil: {worker.Pidrozdil}");
            }
            foreach (var check in all_checks)
            {
                Console.WriteLine($"Date: {check.Date}, Worker ID: {check.WorkerId}, Project ID: {check.ProjectId}, Worked Hours: {check.WorkedHours}, Zmist: {check.Zmist}");
            }

            // ----- task a -----
            // xml-файл, де звiти систематизованi за схемою
            // <назва проєкту, перелiк прiзвищ(з iнiцiалами) працiвникiв разом iз сумарною кiлькiстю годин,
            // вiдпрацьованих кожним з них>;
            // вмiст впорядкувати у лексико-графiчному порядку за назвою проєкту i прiзвищем працiвникiв;
            
            var task_a = from check in all_checks
                         join project in all_projects on check.ProjectId equals project.ProjectId
                         join worker in all_workers on check.WorkerId equals worker.WorkerId
                         orderby project.ProjectName, worker.WorkerSurname
                         group check by new { project.ProjectName } into gr
                         select new
                         {
                             ProjectName = gr.Key.ProjectName,
                             Workers = gr.Select(g => new { Worker = all_workers.FirstOrDefault(s => s.WorkerId == g.WorkerId), g.WorkedHours })
                         };

            var XTask_a = new XElement("TaskA",
                from item in task_a
                select new XElement("Project",
                new XElement("ProjectName", item.ProjectName),
                from w in item.Workers
                select new XElement("Worker",
                new XElement("Name", w.Worker.WorkerName),
                new XElement("Surname", w.Worker.WorkerSurname),
                new XElement("WorkedHours", w.WorkedHours))));

            XTask_a.Save("../../taskA.xml");

            // ----- task b -----
            // xml-файл, описаний у попередньому завданнi, але подати крiм
            // вiдпрацьованих годин ще й зароблену суму грошей;

            var queryForB = from check in all_checks
                            join project in all_projects on check.ProjectId equals project.ProjectId
                            join worker in all_workers on check.WorkerId equals worker.WorkerId
                            join posada in all_posadas on worker.WorkerPosadaId equals posada.PosadaId
                            select new
                            {
                                ProjName = project.ProjectName,
                                TotalPrice = posada.PosadaPrice * check.WorkedHours,
                                FullName = worker.WorkerName + " " + worker.WorkerSurname,
                                WorkerId = worker.WorkerId + " " + worker.WorkerId,
                                Hours = check.WorkedHours
                            };

            var task_b = from item in queryForB
                         group item by new { item.ProjName } into gr
                         select new
                         {
                             ProjectName = gr.Key.ProjName,
                             Workers = gr.Select(g => new {
                                 Worker = all_workers.FirstOrDefault(s => s.WorkerId == s.WorkerId),
                                 g.TotalPrice,
                                 g.Hours
                             })
                         };


            var XTask_b = new XElement("TaskB",
                from item in task_b
                select new XElement("Project",
                new XElement("ProjectName", item.ProjectName),
                from w in item.Workers
                select new XElement("Worker",
                    new XElement("Name", w.Worker.WorkerName),
                    new XElement("Surname", w.Worker.WorkerSurname),
                    new XElement("WorkedHours", w.Hours),
                    new XElement("TotalPrice", w.TotalPrice))));

            XTask_b.Save("../../taskB.xml");

            // ----- task c -----
            // xml-файл, в якому для кожного проєкту(заданого iдентифiкатором) вказати сумарний час ро-
            // боти над ним працiвниками вiдповiдних посад;
            // вмiст впорядкувати у лексико-графiчному порядку за iдентифiкатором проєкту;

            var task_c = from check in all_checks
                         join project in all_projects on check.ProjectId equals project.ProjectId
                         join worker in all_workers on check.WorkerId equals worker.WorkerId
                         join posada in all_posadas on worker.WorkerPosadaId equals posada.PosadaId
                         select new
                         {
                             WorkerName = worker.WorkerName,
                             ProjectId = project.ProjectId,
                             ProjectName = project.ProjectName,
                             Hours = check.WorkedHours,
                             Seller = posada.PosadaPrice,
                             Position = posada.PosadaName
                         };

            var XTask_c = new XElement("TaskC",
                from i in task_c
                group i by i.ProjectId into p
                orderby p.Key
                select new XElement("project", new XAttribute("id", p.Key),
                    from j in p
                    group j by j.Position into w
                    orderby w.Key
                    select new XElement("position", new XAttribute("name", w.Key),
                    new XElement("hours", w.Sum(k => k.Hours)))));

            XTask_c.Save("../../taskC.xml");

            // ----- task d -----
            // xml - файл, де для кожного проєкту(заданого iдентифiкатором) вказати сумарний час роботи
            // над ним i освоєну суму грошей; цi результати впорядкувати за сумарним часом у спадному
            // порядку.

            var forTaskD = new XElement("forTaskD",
                from i in task_c
                group i by i.ProjectName into p
                orderby p.Key
                select new XElement("project",
                new XAttribute("name", p.Key),
                    new XElement("hours", p.Sum(k => k.Hours)),
                    new XElement("total_seller", p.Sum(k => k.Seller * k.Hours))));

            forTaskD.Save("../../taskD.xml");
        }
    }
}