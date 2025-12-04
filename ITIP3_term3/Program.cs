using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Collections.Generic;

public sealed class FileWrapper : IDisposable
{
    [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr open(string path, [MarshalAs(UnmanagedType.Bool)] bool read);

    [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void close(IntPtr file);

    [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool read(IntPtr file, int num, StringBuilder word);

    [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void write(IntPtr file, string text);

    [DllImport("file32.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int length(IntPtr file);

    private IntPtr _fileHandle;
    private bool _disposed = false;
    private readonly string _filePath;
    private readonly bool _readOnly;

    public FileWrapper(string path, bool readOnly)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("File path cannot be empty", nameof(path));

        _filePath = path;
        _readOnly = readOnly;

        try
        {
            _fileHandle = open(path, readOnly);
            if (_fileHandle == IntPtr.Zero)
                throw new Exception($"Failed to open file: {path}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error opening file {path}: {ex.Message}", ex);
        }
    }

    public IntPtr Handle => _fileHandle;

    public int WordCount
    {
        get
        {
            CheckDisposed();
            try
            {
                return length(_fileHandle);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting word count: {ex.Message}", ex);
            }
        }
    }

    public string ReadWord(int wordNumber)
    {
        CheckDisposed();

        if (wordNumber < 1 || wordNumber > WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordNumber),
                $"Word number must be between 1 and {WordCount}");

        try
        {
            StringBuilder wordBuilder = new StringBuilder(255);
            bool success = read(_fileHandle, wordNumber, wordBuilder);

            if (!success)
                throw new Exception($"Failed to read word #{wordNumber}");

            return wordBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading word #{wordNumber}: {ex.Message}", ex);
        }
    }

    public void WriteText(string text)
    {
        CheckDisposed();

        if (_readOnly)
            throw new InvalidOperationException("File is opened in read-only mode");

        try
        {
            write(_fileHandle, text);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error writing to file: {ex.Message}", ex);
        }
    }

    public string[] GetAllWords()
    {
        CheckDisposed();

        int count = WordCount;
        string[] words = new string[count];

        for (int i = 1; i <= count; i++)
        {
            words[i - 1] = ReadWord(i);
        }

        return words;
    }

    public void ProcessFile()
    {
        CheckDisposed();

        if (_readOnly)
            throw new InvalidOperationException("File is opened in read-only mode");

        try
        {
            string[] allWords = GetAllWords();

            if (allWords.Length == 0)
                return;

            var wordGroups = allWords
                .GroupBy(w => w)
                .Select(g => new
                {
                    Word = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var sortedWords = wordGroups
                .OrderBy(w => w.Word.Length)
                .ThenBy(w => w.Word)
                .ToList();

            StringBuilder newContent = new StringBuilder();
            foreach (var item in sortedWords)
            {
                newContent.Append($"{item.Word} {item.Count} ");
            }

            WriteText(newContent.ToString().Trim());
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing file: {ex.Message}", ex);
        }
    }

    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileWrapper));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_fileHandle != IntPtr.Zero)
            {
                try
                {
                    close(_fileHandle);
                }
                catch
                {
                    // Ignore errors during finalization
                }
                _fileHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~FileWrapper()
    {
        Dispose(false);
    }
}

class Program
{
    private static FileWrapper currentFile = null;
    private static bool fileOpened = false;

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== File Library Testing ===");
            Console.WriteLine();
            Console.WriteLine("File status: " + (fileOpened ? "Opened" : "Not opened"));
            Console.WriteLine();
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Open file");
            Console.WriteLine("2. Get word count");
            Console.WriteLine("3. Process file (unique words + sorting)");
            Console.WriteLine("4. View file contents");
            Console.WriteLine("5. Close file and exit");
            Console.WriteLine();
            Console.Write("Select action: ");

            string choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        OpenFile();
                        break;
                    case "2":
                        ShowWordCount();
                        break;
                    case "3":
                        ProcessFile();
                        break;
                    case "4":
                        ShowFileContents();
                        break;
                    case "5":
                        CloseFile();
                        Console.WriteLine("Program finished.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to continue...");
                        Console.ReadLine();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
    }

    static void OpenFile()
    {
        Console.Write("Enter file path: ");
        string path = Console.ReadLine();

        Console.Write("Opening mode (R - read, W - write): ");
        string mode = Console.ReadLine().ToUpper();

        bool readOnly = mode == "R";

        if (fileOpened)
        {
            currentFile.Dispose();
            fileOpened = false;
        }

        try
        {
            currentFile = new FileWrapper(path, readOnly);
            fileOpened = true;
            Console.WriteLine($"File opened successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening file: {ex.Message}");
            fileOpened = false;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    static void ShowWordCount()
    {
        if (fileOpened)
        {
            try
            {
                int count = currentFile.WordCount;
                Console.WriteLine($"Word count in file: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting word count: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("File is not opened.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    static void ProcessFile()
    {
        if (fileOpened)
        {
            try
            {
                Console.WriteLine($"Processing file...");
                currentFile.ProcessFile();
                Console.WriteLine($"File processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("File is not opened.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    static void ShowFileContents()
    {
        if (fileOpened)
        {
            try
            {
                Console.WriteLine($"File contents:");
                string[] words = currentFile.GetAllWords();

                for (int j = 0; j < words.Length; j++)
                {
                    Console.WriteLine($"  {j + 1}. {words[j]}");
                }

                if (words.Length == 0)
                {
                    Console.WriteLine("  (file is empty)");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("File is not opened.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    static void CloseFile()
    {
        if (fileOpened)
        {
            try
            {
                currentFile.Dispose();
                fileOpened = false;
                Console.WriteLine($"File closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing file: {ex.Message}");
            }
        }
    }
}