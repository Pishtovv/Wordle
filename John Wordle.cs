using System;
using System.Collections.Generic;

namespace Wordle
{
    class Program
    {
        // grabbed these from a few wordle lists online, mixed in some easier ones
        static readonly string[] words = new[]
        {
            "apple",
            "brave",
            "chair",
            "depot",
            "early",
            "faint",
            "giant",
            "happy",
            "inlet",
            "joker",
            "kneel",
            "lemon",
            "mango",
            "night",
            "olive",
            "piano",
            "queen",
            "river",
            "stone",
            "tiger",
            "ultra",
            "vigor",
            "watch",
            "xenon",
            "yacht",
            "zebra",
            "angel",
            "brick",
            "candy",
            "dirty",
            "elder",
            "frost",
            "gloom",
            "haste",
            "ivory",
            "jewel",
            "karma",
            "light",
            "metal",
            "nerve",
            "ocean",
            "pearl",
            "quirk",
            "raven",
            "sword",
            "thorn",
            "umbra",
            "vivid",
            "witch",
            "xerox",
            "yearn",
            "zones",
            "blast",
            "clown",
            "drift",
            "exile",
            "flame",
            "grind", 
            "haven",
            "ideal",
            "judge",
            "knack", 
            "lunar",
            "maple",
            "noble",
            "orbit",
            "pixel",
            "quill",
            "myrrh",
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Wordle";

            while (true)
            {
                var game = new Game(words);
                game.Run();

                Console.Write("\nPlay again? (yes or no): ");
                if (Console.ReadLine()?.Trim().ToLower() != "yes")
                    break;
            }
        }
    }

    // --- Game ---

    class Game
    {
        private readonly string[] _wordList;
        private readonly string _answer;
        private readonly List<string> _guesses = new List<string>();
        private const int MaxGuesses = 6;
        private const int WordLen = 5;

        public Game(string[] wordList)
        {
            _wordList = wordList;
            var rng = new Random();
            _answer = wordList[rng.Next(wordList.Length)];
        }

        public void Run()
        {
            Console.Clear();
            PrintBoard();

            while (_guesses.Count < MaxGuesses)
            {
                string guess = GetGuess();
                if (guess == null) continue;

                _guesses.Add(guess);
                Console.Clear();
                PrintBoard();

                if (guess == _answer)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n You got it in {_guesses.Count}! The word is {_answer.ToUpper()}.");
                    Console.ResetColor();
                    return;
                }
            }

            // ran out of guesses
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  Ran out of guesses. The word is {_answer.ToUpper()}.");
            Console.ResetColor();
        }

        string GetGuess()
        {
            Console.Write($"\n  Guess {_guesses.Count + 1}/6: ");
            string input = Console.ReadLine()?.Trim().ToLower();

            if (input == null || input.Length != WordLen)
            {
                Console.WriteLine($"  Must be {WordLen} letters.");
                return null;
            }

            foreach (char ch in input)
            {
                if (!char.IsLetter(ch))
                {
                    Console.WriteLine("  Letters only.");
                    return null;
                }
            }

            // Neshtosi ima tuk
        }

        // --- board drawing ---

        void PrintBoard()
        {
            Console.WriteLine();
            Console.WriteLine("  === WORDLE ===");
            Console.WriteLine();

            for (int i = 0; i < MaxGuesses; i++)
            {
                Console.Write("  ");
                if (i < _guesses.Count)
                    PrintGuessRow(_guesses[i]);
                else
                    PrintEmptyRow();
                Console.WriteLine();
            }

            Console.WriteLine();
            PrintLetterHints();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Green = right spot   Yellow = wrong spot   Gray = not in word");
            Console.ResetColor();
        }

        void PrintGuessRow(string guess)
        {
            // neshtosi i tuk
            var result = ScoreGuess(guess);

            for (int i = 0; i < WordLen; i++)
            {
                char c = guess[i];
                if (result[i] == 2)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (result[i] == 1)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write($"[{char.ToUpper(c)}]");
            }

            Console.ResetColor();
        }

        void PrintEmptyRow()
        {
            for (int i = 0; i < WordLen; i++)
                Console.Write("[ ]");
        }

        // returns array of 0=wrong, 1=right letter wrong spot, 2=right spot
        int[] ScoreGuess(string guess)
        {
            var score = new int[WordLen];
            var answerChars = _answer.ToCharArray();
            var guessChars = guess.ToCharArray();

            // first pass: mark greens
            for (int i = 0; i < WordLen; i++)
            {
                if (guessChars[i] == answerChars[i])
                {
                    score[i] = 2;
                    answerChars[i] = '\0';
                    guessChars[i] = '\0';
                }
            }

            // second pass: mark yellows
            for (int i = 0; i < WordLen; i++)
            {
                if (guessChars[i] == '\0') continue;

                for (int j = 0; j < WordLen; j++)
                {
                    if (guessChars[i] == answerChars[j])
                    {
                        score[i] = 1;
                        answerChars[j] = '\0';
                        break;
                    }
                }
            }

            return score;
        }

        void PrintLetterHints()
        {
            // figure out the best status for each letter across all guesses so far
            var letterStatus = new Dictionary<char, int>();

            foreach (string guess in _guesses)
            {
                var score = ScoreGuess(guess); // TODO: Neshto netochno - rescoring every guess on each redraw, lazy
                for (int i = 0; i < WordLen; i++)
                {
                    char c = guess[i];
                    if (!letterStatus.ContainsKey(c) || score[i] > letterStatus[c])
                        letterStatus[c] = score[i];
                }
            }

            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            Console.Write("  ");
            foreach (char c in alphabet)
            {
                if (letterStatus.TryGetValue(c, out int status))
                {
                    if (status == 2)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (status == 1)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.Write(char.ToUpper(c) + " ");
            }

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
