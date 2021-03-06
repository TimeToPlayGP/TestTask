﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestTask
{
    public class Program
    {
        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Указаны неккоректные данные для поиска файлов!");
                Console.WriteLine("Нажмите любую клавишу для завершения!");
                Console.ReadKey();

                Environment.Exit(0);
            }

            IList<LetterStats> singleLetterStats;
            IList<LetterStats> doubleLetterStats;

            //Освобождение ресурсов
            using (IReadOnlyStream inputStream1 = GetInputStream(args[0]),
            inputStream2 = GetInputStream(args[1]))
            {
                singleLetterStats = FillSingleLetterStats(inputStream1);
                doubleLetterStats = FillDoubleLetterStats(inputStream2);
            }

            RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
            RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

            PrintStatistic(singleLetterStats);

            Console.WriteLine("Исходя из задания, учет пары букв не регистрозависим,");
            Console.WriteLine("утверждаем, что АА=Аа=аА=аа, и выводим пары букв в верхнем регистре.");

            PrintStatistic(doubleLetterStats);

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для завершения!");
            Console.ReadKey();
            // TODO : Необжодимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();

            //Список букв
            IList<LetterStats> singleLetters = new List<LetterStats>();

            while (!stream.IsEof)
            {
                //Перебор по символам
                char c = stream.ReadNextChar();

                //Если входит в набор
                if (Regex.IsMatch(c.ToString(), "[а-яА-Я]"))
                {
                    int count = 0;
                    //Проходимся по списку, если имеется уже такая буква, то увеличиваем
                    //Если нет, то добавляем в список букву с количеством 1 
                    foreach (LetterStats item in singleLetters)
                    {
                        if (item.Letter == c.ToString())
                        {
                            LetterStats copyLetter = singleLetters[count];
                            IncStatistic(ref copyLetter);
                            singleLetters[count] = copyLetter;
                            c = default;
                            break;
                        }
                        count++;
                    }
                    if (c != default) singleLetters.Add(new LetterStats() { Letter = c.ToString(), Count = 1 });
                }
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
            }
            return singleLetters;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            //Список букв
            IList<LetterStats> doubleLetters = new List<LetterStats>();

            stream.ResetPositionToStart();

            //Предыдущая буква
            char cBack = stream.ReadNextChar();

            while (!stream.IsEof)
            {
                //Перебор по символам
                char c = stream.ReadNextChar();

                //Буквы в верхнем регистре, предыдущая и текущая буквы
                char upperLetterC = char.ToUpper(c);
                char upperLetterCBack = char.ToUpper(cBack);

                //Если входит в набор
                if (Regex.IsMatch(cBack.ToString(), "[а-яА-Я]") && Regex.IsMatch(c.ToString(), "[а-яА-Я]"))
                {
                    if (upperLetterCBack == upperLetterC)
                    {
                        int count = 0;
                        //Проходимся по списку, если имеется уже такая буква, то увеличиваем
                        //Если нет, то добавляем в список букву с количеством 1 
                        foreach (LetterStats item in doubleLetters)
                        {
                            if (char.ToUpper(item.Letter[0]) == upperLetterCBack)
                            {
                                LetterStats copyLetter = doubleLetters[count];
                                IncStatistic(ref copyLetter);
                                doubleLetters[count] = copyLetter;
                                c = default;
                                break;
                            }
                            count++;
                        }
                        if (c != default)
                            doubleLetters.Add(new LetterStats()
                            { Letter = upperLetterCBack + upperLetterC.ToString(), Count = 1 });
                    }
                }
                cBack = c;
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - НЕ регистрозависимый.
            }
            return doubleLetters;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.
            switch (charType)
            {
                case CharType.Consonants:
                    RemoveLettrs(letters, false);
                    break;
                case CharType.Vowel:
                    RemoveLettrs(letters, true);
                    break;
            }
        }

        /// <summary>
        /// Удаление букв по типу
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="isVowel">Удалить гласные буквы?</param>
        private static void RemoveLettrs(IList<LetterStats> letters, bool isVowel)
        {
            //Гласные буквы
            string vowelLetters = "уеыаоэяию";          
            //Вхождение букв - храним позиции в стэке
            Stack<int> countMassiv = new Stack<int>();
            //Количество найденных букв
            int count = 0;
            foreach (LetterStats item in letters)
            {
                //Если совпадает, заносим в стэк индекс
                if (vowelLetters.Contains(item.Letter[0].ToString().ToLower()) == isVowel)
                {
                    countMassiv.Push(count);
                }
                count++;
            }
            count = countMassiv.Count;
            for (int i = 0; i < count; i++)
            {
                //Удаляем совпадение
                letters.RemoveAt(countMassiv.Pop());
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            int countLetters = 0;

            List<LetterStats> sortLetters = (List<LetterStats>)letters;
            sortLetters.Sort(new SortLetters());

            foreach (LetterStats item in sortLetters)
            {
                countLetters += item.Count;
                Console.WriteLine($"Буква - {item.Letter} : {item.Count}");
            }

            Console.WriteLine($"Итого найденно букв: {countLetters}");
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(ref LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
