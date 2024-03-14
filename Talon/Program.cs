using System;
using System.Linq;
using System.Xml.Linq;

namespace Talon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var results = XElement.Load("../../results.xml");
            var students = XElement.Load("../../students.xml");
            var themes = XElement.Load("../../themes.xml");

            var all_results = results.Elements("Result").Select(res => new
            {
                StudentId = (int)res.Element("StudentId"),
                Date = (DateTime)res.Element("Date"),
                ThemeId = (int)res.Element("ThemeId"),
                Score = (int)res.Element("Score")
            });

            var all_students = students.Elements("Student").Select(stud => new
            {
                StudentId = (int)stud.Element("StudentId"),
                StudentName = (string)stud.Element("StudentName"),
                StudentSurname = (string)stud.Element("StudentSurname"),
                SpecialtyName = (string)stud.Element("SpecialtyName"),
                GroupName = (string)stud.Element("GroupName")
            });

            var all_themes = themes.Elements("Theme").Select(thm => new
            {
                ThemeId = (int)thm.Element("ThemeId"),
                SubjectName = (string)thm.Element("SubjectName"),
                ThemeName = (string)thm.Element("ThemeName"),
                MaxScore = (int)thm.Element("MaxScore")
            });

            foreach (var i in all_results) { Console.WriteLine(i); }
            foreach (var i in all_students) { Console.WriteLine(i); }
            foreach (var i in all_themes) { Console.WriteLine(i); }

            // task 1
            // xml файл де результати систематизовані для кожної дати за схемою:
            // <прізвище з ініціалом імені студента, назва теми, кількість балів(отриманих за тему>;
            // результати впорядкувати за прізвищем у лексикографічному порядку.

            var queryForTask1 = from result in all_results
                                join student in all_students on result.StudentId equals student.StudentId
                                join theme in all_themes on result.ThemeId equals theme.ThemeId
                                orderby student.StudentSurname
                                group new { result, student, theme } by new { result.Date } into grup
                                select new
                                {
                                    ExamDate = grup.Key.Date,
                                    Results = grup.Select(data => new
                                    {
                                        StudentFullName = data.student.StudentSurname + " " + data.student.StudentName[0] + ".",
                                        ThemeName = data.theme.ThemeName,
                                        FinalScore = data.result.Score
                                    })
                                };

            var Xtask1 = new XElement("Task1",
                from item in queryForTask1
                select new XElement("Date",
                new XAttribute("ExamDate", item.ExamDate),
                from otem in item.Results
                select new XElement("Result",
                new XElement("StudentFullName", otem.StudentFullName),
                new XElement("ThemeName", otem.ThemeName),
                new XElement("FinalScore", otem.FinalScore))));

            Xtask1.Save("../../Task1.xml");

            // task 2
            // xml файл, подібний до попереднього завдання, але доповнений назвою групи.

            var queryForTask2 = from result in all_results
                                join student in all_students on result.StudentId equals student.StudentId
                                join theme in all_themes on result.ThemeId equals theme.ThemeId
                                orderby student.StudentSurname
                                group new { result, student, theme } by new { result.Date } into grup
                                select new
                                {
                                    ExamDate = grup.Key.Date,
                                    Results = grup.Select(data => new
                                    {
                                        StudentFullName = data.student.StudentSurname + " " + data.student.StudentName[0] + ".",
                                        StudentGroupName = data.student.GroupName,
                                        ThemeName = data.theme.ThemeName,
                                        FinalScore = data.result.Score
                                    })
                                };

            var Xtask2 = new XElement("Task2",
                from item in queryForTask2
                select new XElement("Date",
                new XAttribute("ExamDate", item.ExamDate),
                from otem in item.Results
                select new XElement("Result",
                new XElement("StudentFullName", otem.StudentFullName),
                new XElement("StudentGroupName", otem.StudentGroupName),
                new XElement("ThemeName", otem.ThemeName),
                new XElement("FinalScore", otem.FinalScore))));

            Xtask2.Save("../../Task2.xml");

            // task 3
            // xml файл, в якому для кожного предмета подати результати за схемою:
            // <прізвище з ініціалом імені студента, сумарний результат за усі теми>;
            // впорядкувати за рейтингом результатів в межах групи.

            var queryForTask3 = from result in all_results
                                join student in all_students on result.StudentId equals student.StudentId
                                join theme in all_themes on result.ThemeId equals theme.ThemeId
                                orderby student.GroupName, result.Score descending
                                group new { result, student, theme } by new { theme.SubjectName, student.GroupName } into grup
                                select new
                                {
                                    Subject = grup.Key.SubjectName,
                                    Group = grup.Key.GroupName,
                                    Results = grup.Select(data => new
                                    {
                                        StudentFullName = data.student.StudentSurname + " " + data.student.StudentName[0] + ".",
                                        TotalScore = data.result.Score
                                    }).OrderByDescending(gr => gr.TotalScore)
                                };

            var Xtask3 = new XElement("Task3",
                from item in queryForTask3
                select new XElement("Subject",
                new XAttribute("SubjectName", item.Subject),
                new XElement("Group",
                new XAttribute("GroupName", item.Group),
                from otem in item.Results
                group otem by otem.StudentFullName into StudentResult
                select new XElement("Result",
                new XElement("StudentFullName", StudentResult.Key),
                new XElement("TotalScore", StudentResult.Sum(gr => gr.TotalScore))))));

            Xtask3.Save("../../Task3.xml");

            // task 4
            // xml файл, в якому на основі результатів попереднього завдання для кожного предмета подати
            // результати (впорядковані за назвою спеціальності) за схемою:
            // <прізвище з ініціалом імені студента, відсоток сумарного результату за усі теми від макс. можливої суми балів>.

            var queryForTask4 = from result in all_results
                                join student in all_students on result.StudentId equals student.StudentId
                                join theme in all_themes on result.ThemeId equals theme.ThemeId
                                orderby student.SpecialtyName, result.Score descending
                                group new { result, student, theme } by new { student.SpecialtyName, student.StudentId } into grup
                                select new
                                {
                                    Specialty = grup.Key.SpecialtyName,
                                    Results = grup.Select(data => new
                                    {
                                        StudentFullName = data.student.StudentSurname + " " + data.student.StudentName[0] + ".",
                                        ScorePercentage = (double)grup.Sum(r => r.result.Score) / grup.Sum(t => t.theme.MaxScore) * 100
                                    }).Distinct()
                                };

            var Xtask4 = new XElement("Task4",
                from item in queryForTask4
                select new XElement("Specialty",
                new XAttribute("Specialty", item.Specialty),
                from otem in item.Results
                select new XElement("Result",
                new XElement("StudentFullName", otem.StudentFullName),
                new XElement("ScorePercentage", otem.ScorePercentage.ToString("F2") + "%"))));

            Xtask4.Save("../../Task4.xml");
        }
    }
}
