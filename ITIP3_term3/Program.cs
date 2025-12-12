using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ITIP3
{
    public class FileLibrary : IDisposable
    {
        [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr open(string path, bool read);

        [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void close(IntPtr file);

        [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool read(IntPtr file, int num, StringBuilder word);

        [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void write(IntPtr file, string text);

        [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int length(IntPtr file);

        protected bool disposed = false;
        IntPtr file;
        string path;

        public FileLibrary(string _path, bool mode)
        {
            path = _path;
            try
            {
                file = open(path, mode);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0} Check parameters", e.Message);
            }
        }

        public void Open(string path, bool mode)
        {
            try
            {
                file = open(path, mode);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0} Check parameters", e.Message);
            }
        }

        public int CountWords()
        {
            return length(file);
        }

        public StringBuilder ReadWord(int index)
        {
            StringBuilder word = new StringBuilder(255);
            if (read(file, index, word))
            {
                return word;
            }
            else
            {
                throw new Exception($"Failure read word index {index}");
            }
        }

        // ФУНКЦИЯ 1: Оставить только уникальные слова с указанием количества повторений
        public void LeaveUniqueWordsWithCount()
        {
            // получаем все слова из файла
            List<string> words = new List<string>();
            for (int i = 0; i < this.CountWords(); i++)
            {
                words.Add(this.ReadWord(i + 1).ToString());
            }

            close(file);

            // считаем частоту слов и сохраняем порядок уникальных слов
            Dictionary<string, int> frequency = new Dictionary<string, int>();
            List<string> uniqueWordsInOrder = new List<string>();

            foreach (string word in words)
            {
                if (frequency.ContainsKey(word))
                {
                    frequency[word]++;
                }
                else
                {
                    frequency[word] = 1;
                    uniqueWordsInOrder.Add(word); // сохраняем порядок первого появления
                }
            }

            // формируем новый текст: слово(количество_повторений)
            List<string> resultWords = new List<string>();
            foreach (string word in uniqueWordsInOrder)
            {
                resultWords.Add($"{word}({frequency[word]})");
            }

            string resultText = string.Join(" ", resultWords);

            this.Open(path, false);
            write(file, resultText);
            close(file);
            this.Open(path, true);
        }

        // ФУНКЦИЯ 2: Отсортировать слова по количеству букв (простая сортировка)
        public void SortWordsByLength()
        {
            // получаем все слова из файла (включая повторения)
            List<string> words = new List<string>();
            for (int i = 0; i < this.CountWords(); i++)
            {
                words.Add(this.ReadWord(i + 1).ToString());
            }

            close(file);

            // сортируем слова по длине (от самого короткого к самому длинному)
            // слова одинаковой длины сохраняются в исходном порядке
            List<string> sortedWords = words.OrderBy(w => w.Length).ToList();

            string resultText = string.Join(" ", sortedWords);

            this.Open(path, false);
            write(file, resultText);
            close(file);
            this.Open(path, true);
        }

        // Реализация интерфейса IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // очистка управляемых ресурсов
                }

                // очистка неуправляемых ресурсов
                if (file != IntPtr.Zero)
                {
                    close(file);
                    file = IntPtr.Zero;
                }

                disposed = true;
            }
        }

        // Финализатор (деструктор)
        ~FileLibrary()
        {
            Dispose(false);
        }
    }

    class Program
    {
        static int CorrectNumber(int minNum, int maxNum)
        {
            int num;
            do
            {
                try
                {
                    num = Convert.ToInt32(Console.ReadLine());
                    if (num > maxNum || num < minNum)
                        throw new ArgumentOutOfRangeException();
                    else
                        break;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("Number out of range. Please enter number between {0} and {1}", minNum, maxNum);
                    Console.Write("Try again: ");
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                    Console.Write("Try again: ");
                }
            }
            while (true);
            return num;
        }

        static void Main(string[] args)
        {
            FileLibrary file = null;
            int menu;

            do
            {
                Console.WriteLine("-------- File Operations Menu --------");
                Console.WriteLine("1) Open file");
                Console.WriteLine("2) Get word count");
                Console.WriteLine("3) Leave only unique words with repetition count");
                Console.WriteLine("4) Sort words by length");
                Console.WriteLine("0) Exit");
                Console.Write("Select option: ");

                menu = CorrectNumber(0, 4);

                switch (menu)
                {
                    case 1: // Open file
                        {
                            try
                            {
                                Console.Write("Enter file path: ");
                                string path = Console.ReadLine();
                                Console.Write("Open for reading (1) or writing (0)? ");
                                bool readMode = CorrectNumber(0, 1) == 1;

                                file?.Dispose(); // Dispose previous file if exists
                                file = new FileLibrary(path, readMode);
                                Console.WriteLine("File opened successfully!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error opening file: {0}", ex.Message);
                            }
                            break;
                        }
                    case 2: // Get word count
                        {
                            if (file == null)
                            {
                                Console.WriteLine("Please open a file first!");
                                break;
                            }
                            try
                            {
                                int count = file.CountWords();
                                Console.WriteLine("Word count in file: {0}", count);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error getting word count: {0}", ex.Message);
                            }
                            break;
                        }
                    case 3: // Leave only unique words with repetition count
                        {
                            if (file == null)
                            {
                                Console.WriteLine("Please open a file first!");
                                break;
                            }
                            try
                            {
                                Console.WriteLine("Processing... This will modify the file!");
                                Console.WriteLine("Original words will be replaced with unique words and their counts.");

                                // Сохраняем исходное состояние для сравнения
                                int originalCount = file.CountWords();
                                Console.WriteLine("Original word count: {0}", originalCount);

                                file.LeaveUniqueWordsWithCount();
                                Console.WriteLine("File updated - only unique words with repetition count remain!");

                                // Показать результат
                                int newCount = file.CountWords();
                                Console.WriteLine("New word count: {0}", newCount);

                                if (newCount > 0)
                                {
                                    Console.WriteLine("New content (format: word(count)):");
                                    int showCount = Math.Min(10, newCount);
                                    for (int i = 1; i <= showCount; i++)
                                    {
                                        Console.WriteLine($"  {i}. {file.ReadWord(i)}");
                                    }
                                    if (newCount > 10)
                                    {
                                        Console.WriteLine($"  ... and {newCount - 10} more");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error processing file: {0}", ex.Message);
                            }
                            break;
                        }
                    case 4: // Sort words by length
                        {
                            if (file == null)
                            {
                                Console.WriteLine("Please open a file first!");
                                break;
                            }
                            try
                            {
                                Console.WriteLine("Processing... This will modify the file!");
                                Console.WriteLine("Words will be sorted by length (from shortest to longest).");

                                // Сохраняем исходное состояние для сравнения
                                int originalCount = file.CountWords();
                                Console.WriteLine("Original word count: {0}", originalCount);

                                // Показываем несколько исходных слов
                                if (originalCount > 0)
                                {
                                    Console.Write("Original first 5 words: ");
                                    for (int i = 1; i <= Math.Min(5, originalCount); i++)
                                    {
                                        Console.Write(file.ReadWord(i) + " ");
                                    }
                                    Console.WriteLine();
                                }

                                file.SortWordsByLength();
                                Console.WriteLine("File updated - words sorted by length!");

                                // Показать результат
                                int newCount = file.CountWords();
                                Console.WriteLine("New word count: {0}", newCount);

                                if (newCount > 0)
                                {
                                    Console.WriteLine("First 10 shortest words:");
                                    int showCount = Math.Min(10, newCount);
                                    for (int i = 1; i <= showCount; i++)
                                    {
                                        string word = file.ReadWord(i).ToString();
                                        Console.WriteLine($"  {i}. '{word}' (length: {word.Length})");
                                    }

                                    if (newCount > 10)
                                    {
                                        Console.WriteLine("\nLast 10 longest words:");
                                        showCount = Math.Min(10, newCount);
                                        int start = Math.Max(1, newCount - 9);
                                        for (int i = start; i <= newCount; i++)
                                        {
                                            string word = file.ReadWord(i).ToString();
                                            Console.WriteLine($"  {i}. '{word}' (length: {word.Length})");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error sorting file: {0}", ex.Message);
                            }
                            break;
                        }
                    case 0: // Exit
                        {
                            Console.WriteLine("Exiting program...");
                            file?.Dispose();
                            break;
                        }
                }

                if (menu != 0)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            while (menu != 0);
        }
    }
}