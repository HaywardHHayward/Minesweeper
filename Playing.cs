using static System.ConsoleKey;

namespace Minesweeper;

public static partial class Program {
    private static Minesweeper s_minesweeper = null!;

    private static void TextPlay() {
        while (true) {
        restart_input:
            Console.Clear();
            ConsoleInterface.PrintConsoleElement(s_minesweeper.ConsoleElement);
            Position = (0, s_minesweeper.RowAmount + 1);
            Console.WriteLine($"Mines remaining (according to number of tiles flagged): {s_minesweeper.MineAmount - s_minesweeper.FlagAmount}");
            bool validInput = false;
            bool checkingNotFlagging = false;
            do {
                Console.Write("Checking or flagging a tile?: ");
                string? input = Console.ReadLine()?.ToLower();
                switch (input) {
                    case "checking":
                    case "check":
                    case "c":
                        validInput = true;
                        checkingNotFlagging = true;
                        break;
                    case "flagging":
                    case "flag":
                    case "f":
                        validInput = true;
                        checkingNotFlagging = false;
                        break;
                    default:
                        WriteStatusMessage("Invalid input. Use (check/flag).");
                        break;
                }
            } while (!validInput);
            Position = (0, Position.y + 1);
            ConsoleInterface.ClearLastLine();
            int row;
            validInput = false;
            do {
                Console.Write($"Enter row to {(checkingNotFlagging ? "check" : "flag")} (input 'back' to go back): ");
                string? input = Console.ReadLine();
                bool parsed = int.TryParse(input, out row);
                if (!parsed) {
                    if (input is "back" or "b") {
                        goto restart_input;
                    }
                    WriteStatusMessage("Invalid input. Enter a nonnegative integer.");
                    continue;
                }
                if (!s_minesweeper.IsValidRow(row)) {
                    WriteStatusMessage($"Row input out of the range of the board. Input a number in between 0 and {s_minesweeper.RowAmount - 1}.");
                    continue;
                }
                validInput = true;
            } while (!validInput);
            Position = (0, Position.y + 1);
            ConsoleInterface.ClearLastLine();
            int col;
            validInput = false;
            do {
                Console.Write($"Enter column to {(checkingNotFlagging ? "check" : "flag")} (input 'back' to go back): ");
                string? input = Console.ReadLine();
                bool parsed = int.TryParse(input, out col);
                if (!parsed) {
                    if (input is "back" or "b") {
                        goto restart_input;
                    }
                    WriteStatusMessage("Invalid input. Enter a nonnegative integer.");
                    continue;
                }
                if (!s_minesweeper.IsValidColumn(col)) {
                    WriteStatusMessage($"Column input out of the range of the board. Input a number in between 0 and {s_minesweeper.ColumnAmount - 1}.");
                    continue;
                }
                validInput = true;
            } while (!validInput);
            Position = (0, Position.y + 1);
            ConsoleInterface.ClearLastLine();
            if (s_minesweeper.IsChecked(row, col)) {
                Console.WriteLine($"Cannot {(checkingNotFlagging ? "check" : "flag")} a tile which has already been checked. Press any key to retry.");
                Console.ReadKey();
                goto restart_input;
            }
            if (checkingNotFlagging) {
                if (s_minesweeper.IsFlagged(row, col)) {
                    Console.WriteLine("Cannot check a tile which has been flagged. Unflag the tile if you wish to check it. Press any key to retry.");
                    Console.ReadKey();
                    goto restart_input;
                }
                s_minesweeper.CheckTile(row, col);
                if (s_minesweeper.LostGame) {
                    Console.Clear();
                    ConsoleInterface.PrintConsoleElement(s_minesweeper.ConsoleElement);
                    Position = (0, s_minesweeper.RowAmount + 2);
                    Console.WriteLine("Oops! You hit a mine! You lost! Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                if (s_minesweeper.WonGame) {
                    Console.Clear();
                    ConsoleInterface.PrintConsoleElement(s_minesweeper.ConsoleElement);
                    Position = (0, s_minesweeper.RowAmount + 2);
                    Console.WriteLine("You found all the mines! You win! Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                continue;
            }
            s_minesweeper.FlagTile(row, col);
            if (!s_minesweeper.WonGame) {
                continue;
            }
            Console.Clear();
            ConsoleInterface.PrintConsoleElement(s_minesweeper.ConsoleElement);
            Position = (0, s_minesweeper.RowAmount + 2);
            Console.WriteLine("You found all the mines! You win! Press any key to exit...");
            Console.ReadKey();
            return;
        }
    }

    private static void KeyboardPlay() {
        ConsoleKey currentCheck = C;
        ConsoleKey currentFlag = F;
        ConsoleElement won = new(0, s_minesweeper.RowAmount + 1,
                                 "You found all the mines! You won! Press any key to exit...");
        ConsoleElement lost = new(0, s_minesweeper.RowAmount + 1,
                                  "Oops! You hit a mine! You lost! Press any key to exit...");
        bool playing = true;
        InitializeKeyHandler();
        ConsoleKeyInfo info = new('\0', None, false, false, false);
        while (playing) {
            ConsoleElement tutorial = new(0, s_minesweeper.RowAmount + 2,
                                          $"Press {currentCheck.ToString()} to check and {currentFlag.ToString()} to flag. Press TAB to rebind keys.");
            ConsoleElement minesRemaining = new(0, s_minesweeper.RowAmount + 1,
                                                $"Number of mines remaining (according to flag count): {s_minesweeper.MineAmount - s_minesweeper.FlagAmount}");
            if (info.Key is None or Tab || info.Key == currentCheck || info.Key == currentFlag) {
                ConsoleInterface.PrintConsoleElements([tutorial, minesRemaining, s_minesweeper.ConsoleElement]);
            }
            info = Console.ReadKey(true);
            ConsoleInterface.DoKeyInput(info);
            if (info.Key is not Tab && info.Key != currentCheck && info.Key != currentFlag) {
                continue;
            }
            ConsoleInterface.ClearConsoleElements([s_minesweeper.ConsoleElement, minesRemaining, tutorial]);
        }
        ConsoleInterface.PrintConsoleElements([s_minesweeper.ConsoleElement, s_minesweeper.WonGame ? won : lost]);
        Console.ReadKey();
        return;

        void InitializeKeyHandler() {
            ConsoleInterface.KeyHandlerTable[LeftArrow] = () => Position = (Position.x - 2, Position.y);
            ConsoleInterface.KeyHandlerTable[A] = () => Position = (Position.x - 2, Position.y);
            ConsoleInterface.KeyHandlerTable[RightArrow] = () =>
                Position = (int.Min(Position.x + 2, (s_minesweeper.ColumnAmount - 1) * 2), Position.y);
            ConsoleInterface.KeyHandlerTable[D] = () =>
                Position = (int.Min(Position.x + 2, (s_minesweeper.ColumnAmount - 1) * 2), Position.y);
            ConsoleInterface.KeyHandlerTable[UpArrow] = () => Position = (Position.x, Position.y - 1);
            ConsoleInterface.KeyHandlerTable[W] = () => Position = (Position.x, Position.y - 1);
            ConsoleInterface.KeyHandlerTable[DownArrow] = () =>
                Position = (Position.x, int.Min(Position.y + 1, s_minesweeper.RowAmount - 1));
            ConsoleInterface.KeyHandlerTable[D] = () =>
                Position = (Position.x, int.Min(Position.y + 1, s_minesweeper.RowAmount - 1));
            ConsoleInterface.KeyHandlerTable[C] = CheckTile;
            ConsoleInterface.KeyHandlerTable[F] = FlagTile;
            ConsoleInterface.KeyHandlerTable[Tab] = ChangeCheckAndFlag;
        }

        void ChangeCheckAndFlag() {
            Console.Clear();
            Console.WriteLine("Press the key you wish to use to check tiles.");
        check_key:
            ConsoleKey newCheck = Console.ReadKey(true).Key;
            if (newCheck is RightArrow or UpArrow or LeftArrow or DownArrow) {
                Console.WriteLine("You cannot rebind check to use the arrow keys. Try again.");
                Position = (0, Position.y - 1);
                goto check_key;
            }
            Position = (0, Position.y + 1);
            ConsoleInterface.ClearLastLine();
            Position = (0, Position.y - 1);
            Console.WriteLine(newCheck.ToString());
            Console.WriteLine("Press the key you wish to use to flag tiles.");
        flag_key:
            ConsoleKey newFlag = Console.ReadKey(true).Key;
            if (newFlag is RightArrow or UpArrow or LeftArrow or DownArrow) {
                Console.WriteLine("You cannot rebind flag to use the arrow keys. Try again.");
                Position = (0, Position.y - 1);
                goto flag_key;
            }
            Position = (0, Position.y + 1);
            ConsoleInterface.ClearLastLine();
            Position = (0, Position.y - 1);
            Console.WriteLine(newFlag.ToString());
            Console.WriteLine("Press any key to return to Minesweeper...");
            Console.ReadKey();
            ConsoleInterface.KeyHandlerTable[currentCheck] = () => { };
            ConsoleInterface.KeyHandlerTable[currentFlag] = () => { };
            ConsoleInterface.KeyHandlerTable[newCheck] = CheckTile;
            ConsoleInterface.KeyHandlerTable[newFlag] = FlagTile;
            currentCheck = newCheck;
            currentFlag = newFlag;
            Console.Clear();
        }

        void CheckTile() {
            int row = Position.y;
            int col = Position.x / 2;
            s_minesweeper.CheckTile(row, col);
            if (s_minesweeper.LostGame || s_minesweeper.WonGame) {
                playing = false;
            }
        }

        void FlagTile() {
            int row = Position.y;
            int col = Position.x / 2;
            s_minesweeper.FlagTile(row, col);
            if (s_minesweeper.WonGame) {
                playing = false;
            }
        }
    }
}