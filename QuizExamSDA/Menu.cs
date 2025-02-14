﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Quiz
{
    class Menu
    {

        public delegate void AccOperation();

        public static User AuthRegChoiceMenu(string key, Dictionary<string, Operation> dict)
        {
            if (!dict.ContainsKey(key))
            {
                Messages.TextErrorChoice();
                return null;
            }

            return dict[key]();
        }

        public static User Entrance()
        {
            string pattern = "^[0-9a-zA-Z]+$";
            bool checkLogin, checkPassword;
            string login, password;
            do
            {
                Messages.AuthorizationText();
                login = Messages.Login();
                checkLogin = Regex.IsMatch(login, pattern);
                password = Messages.Password();
                checkPassword = Regex.IsMatch(password, pattern);
                if (!checkLogin)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Логин не соответствует алфавитно-цифровому формату");
                    Console.ResetColor();
                }
                if (!checkPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Пароль не соответствует алфавитно-цифровому формату");
                    Console.ResetColor();
                }
                if (!checkLogin || !checkPassword) Console.ReadKey();
            } while (!checkLogin || !checkPassword);
            return new User(login, password);
        }
        public static User Registration()
        {
            string pattern = "^[0-9a-zA-Z]+$";
            bool checkLogin, checkPassword, checkDate;
            string login, password, date_of_birth;
            DateTime d_of_birth;
            do
            {
                Messages.RegistrationText();
                login = Messages.Login();
                checkLogin = Regex.IsMatch(login, pattern);
                password = Messages.Password();
                checkPassword = Regex.IsMatch(password, pattern);
                date_of_birth = Messages.Date_of_birth();
                checkDate = DateTime.TryParse(date_of_birth, out DateTime dtResult);
                d_of_birth = dtResult;
                if (!checkLogin)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Логин не соответствует алфавитно-цифровому формату");
                    Console.ResetColor();
                }
                if (!checkPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Пароль не соответствует алфавитно-цифровому формату");
                    Console.ResetColor();
                }
                if (!checkDate)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Дата  введена некорректно");
                    Console.ResetColor();
                }
                if (!checkLogin || !checkPassword || !checkDate) Console.ReadKey();
            } while (!checkLogin || !checkPassword || !checkDate);

            return new User(login, password, d_of_birth);
        }

        public static string TestChoiceMenu(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key))
            {
                Messages.TextErrorChoice();
                return null;
            }

            return dict[key];
        }

        public static bool ChangesChoiceMenu(string key, Dictionary<string, Changes> dict, Dictionary<string, string> changeText, User user)
        {
            bool checkLogin = true;
            bool checkDate = true;
            string str;
            string pattern = "^[0-9a-zA-Z]+$";
            if (!dict.ContainsKey(key))
            {
                Messages.TextErrorChoice();
                return false;
            }
            do
            {
                Console.Write($"{changeText[key]}");
                str = Console.ReadLine();
                if (key == "1")
                {
                    checkLogin = Regex.IsMatch(str, pattern);
                    if (!checkLogin)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Логин не соответствует алфавитно-цифровому формату");
                        Console.ResetColor();
                    }
                }
                if (key == "2")
                {
                    checkDate = DateTime.TryParse(str, out DateTime dtResult);
                    if (!checkDate)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Дата  введена некорректно");
                        Console.ResetColor();
                    }
                }
                if (!checkLogin || !checkDate) Console.ReadKey();
            } while (!checkLogin || !checkDate);
            return dict[key](user, str);
        }

        public static void StartQuiz(User user, Dictionary<string, string> testNameMenu, Dictionary<string, string> path, DataBaseConnect db) //Начало викторины
        {
            Questions test = new Questions();
            string str;
            do
            {
                Messages.TestNameMenu();
                var nameTestKey = Console.ReadLine();
                str = TestChoiceMenu(nameTestKey, testNameMenu);

            } while (str == null);
            Console.WriteLine(GetTest(test, path[str], out uint score, out string testName));
            db.ScoreHistory(user, testName, score);
            Console.WriteLine();
            Messages.TextNext();
            Console.ReadKey();
        }

        public static string GetTest(Questions questions, string path, out uint score, out string testName)
        {
            score = 0;
            string numberOfQuestions = null;
            testName = null;
            var questCounter = 0;
            var deserializedQuestions = questions.QuestionsDeserialization($"{path}");
            foreach (var quest in deserializedQuestions)
            {
                questCounter++;
                testName = quest.TestName;
                Console.WriteLine(quest.Question);
                foreach (var ans in quest.Answers)
                {
                    Console.WriteLine($"{ans.Key} - {ans.Value}");
                    numberOfQuestions = ans.Key;
                }

                Console.Write("Укажите правильный ответ:");
                var answer = Console.ReadLine();
                do
                {

                    if (Regex.IsMatch(answer, PatternString(numberOfQuestions)))
                    {
                        var ant = Convert.ToUInt32(answer);
                        if (ant == quest.TrueAnswer) score++;
                    }
                    else
                    {
                        Messages.TextErrorChoice();
                        Console.Write("Укажите правильный ответ:");
                        answer = Console.ReadLine();
                    }
                } while (!Regex.IsMatch(answer, PatternString(numberOfQuestions)));

            }

            return $"Ваш результат: {score} из {questCounter}";
        }

        public static string PatternString(string numberOfQuestions)
        {
            return $"^[1-{numberOfQuestions}]" + "{1}$";
        }




        public static void AllQuizResultShow(User user, DataBaseConnect db)
        {
            var storyList = db.ShowScoreHistory(user);
            Console.WriteLine("Результаты моих викторин (Дата, Категория теста, Результат):  ");
            Console.WriteLine();
            foreach (var story in storyList)
            {
                Console.WriteLine($"{story.Date} - {story.TestName} - {story.Score}");
            }
            Console.WriteLine();
            Messages.TextNext();
            Console.ReadKey();
        }

        public static void Top20ResultShow(Dictionary<string, string> testNameMenu, DataBaseConnect db)
        {
            string str;
            do
            {
                Messages.TestNameMenu();
                var nameTestKey = Console.ReadLine();
                str = TestChoiceMenu(nameTestKey, testNameMenu);

            } while (str == null);
            var top20 = db.ShowTop20(str);
            Console.WriteLine($"Топ 20 по викторине {str}:  ");
            Console.WriteLine();
            foreach (var result in top20)
            {
                Console.WriteLine($"{result.Login}-{result.Score}");
            }
            Console.WriteLine();
            Messages.TextNext();
            Console.ReadKey();
        }

        public static void ChangeSettings(User user, Dictionary<string, Changes> changesMenu, Dictionary<string, string> changeText)
        {
            string key;
            bool choice;
            do
            {
                Messages.ChangesTextMenu();
                key = Console.ReadLine();
                choice = ChangesChoiceMenu(key, changesMenu, changeText, user);
            } while (!choice);
            Console.WriteLine();
            Messages.TextNext();
            Console.ReadKey();
        }
    }
}
