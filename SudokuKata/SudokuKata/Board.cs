namespace SudokuKata;

class Board
{
    private readonly char[][] _grid;
    public Stack<int[]> State { get; }
    public int[] SolvedState { get; }

    private Board(char[][] grid, Stack<int[]> state, int[] solvedState)
    {
        _grid = grid;
        State = state;
        SolvedState = solvedState;
    }

    public char[][] AsArray()
    {
        return _grid;
    }
    
    public static Board CreateEmptyBoard()
    {
        string line = "+---+---+---+";
        string middle = "|...|...|...|";

        char[][] board = new char[][]
        {
            line.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            line.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            line.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            middle.ToCharArray(),
            line.ToCharArray()
        };


        Random rng = new Random();

        var (stateStack,
            rowIndexStack,
            colIndexStack,
            usedDigitsStack,
            lastDigitStack) = Stacktory.StackFactory();

        // Indicates operation to perform next
        // - expand - finds next empty cell and puts new state on stacks
        // - move - finds next candidate number at current pos and applies it to current state
        // - collapse - pops current state from stack as it did not yield a solution
        string command = "expand";
        while (stateStack.Count <= 9 * 9)
        {
            if (command == "expand")
            {
                int[] currentState = new int[9 * 9];

                if (stateStack.Count > 0)
                {
                    Array.Copy(stateStack.Peek(), currentState, currentState.Length);
                }

                int bestRow = -1;
                int bestCol = -1;
                bool[] bestUsedDigits = null;
                int bestCandidatesCount = -1;
                int bestRandomValue = -1;
                bool containsUnsolvableCells = false;

                for (int index = 0; index < currentState.Length; index++)
                    if (currentState[index] == 0)
                    {
                        int row = index / 9;
                        int col = index % 9;
                        int blockRow = row / 3;
                        int blockCol = col / 3;

                        bool[] isDigitUsed = new bool[9];

                        for (int i = 0; i < 9; i++)
                        {
                            int rowDigit = currentState[9 * i + col];
                            if (rowDigit > 0)
                                isDigitUsed[rowDigit - 1] = true;

                            int colDigit = currentState[9 * row + i];
                            if (colDigit > 0)
                                isDigitUsed[colDigit - 1] = true;

                            int blockDigit = currentState[(blockRow * 3 + i / 3) * 9 + (blockCol * 3 + i % 3)];
                            if (blockDigit > 0)
                                isDigitUsed[blockDigit - 1] = true;
                        } // for (i = 0..8)

                        int candidatesCount = isDigitUsed.Where(used => !used).Count();

                        if (candidatesCount == 0)
                        {
                            containsUnsolvableCells = true;
                            break;
                        }

                        int randomValue = rng.Next();

                        if (bestCandidatesCount < 0 ||
                            candidatesCount < bestCandidatesCount ||
                            (candidatesCount == bestCandidatesCount && randomValue < bestRandomValue))
                        {
                            bestRow = row;
                            bestCol = col;
                            bestUsedDigits = isDigitUsed;
                            bestCandidatesCount = candidatesCount;
                            bestRandomValue = randomValue;
                        }
                    } // for (index = 0..81)

                if (!containsUnsolvableCells)
                {
                    stateStack.Push(currentState);
                    rowIndexStack.Push(bestRow);
                    colIndexStack.Push(bestCol);
                    usedDigitsStack.Push(bestUsedDigits);
                    lastDigitStack.Push(0); // No digit was tried at this position
                }

                // Always try to move after expand
                command = "move";
            } // if (command == "expand")
            else if (command == "collapse")
            {
                stateStack.Pop();
                rowIndexStack.Pop();
                colIndexStack.Pop();
                usedDigitsStack.Pop();
                lastDigitStack.Pop();

                command = "move"; // Always try to move after collapse
            }
            else if (command == "move")
            {
                int rowToMove = rowIndexStack.Peek();
                int colToMove = colIndexStack.Peek();
                int digitToMove = lastDigitStack.Pop();

                int rowToWrite = rowToMove + rowToMove / 3 + 1;
                int colToWrite = colToMove + colToMove / 3 + 1;

                bool[] usedDigits = usedDigitsStack.Peek();
                int[] currentState = stateStack.Peek();
                int currentStateIndex = 9 * rowToMove + colToMove;

                int movedToDigit = digitToMove + 1;
                while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                    movedToDigit += 1;

                if (digitToMove > 0)
                {
                    usedDigits[digitToMove - 1] = false;
                    currentState[currentStateIndex] = 0;
                    board[rowToWrite][colToWrite] = '.';
                }

                if (movedToDigit <= 9)
                {
                    lastDigitStack.Push(movedToDigit);
                    usedDigits[movedToDigit - 1] = true;
                    currentState[currentStateIndex] = movedToDigit;
                    board[rowToWrite][colToWrite] = (char)('0' + movedToDigit);

                    // Next possible digit was found at current position
                    // Next step will be to expand the state
                    command = "expand";
                }
                else
                {
                    // No viable candidate was found at current position - pop it in the next iteration
                    lastDigitStack.Push(0);
                    command = "collapse";
                }
            } // if (command == "move")
        }

        Console.WriteLine();
        Console.WriteLine("Final look of the solved board:");
        Console.WriteLine(string.Join(Environment.NewLine, board.Select(s => new string(s)).ToArray()));

        // Board is solved at this point.
        // Now pick subset of digits as the starting position.
        int remainingDigits = 30;
        int maxRemovedPerBlock = 6;
        int[,] removedPerBlock = new int[3, 3];
        int[] positions = Enumerable.Range(0, 9 * 9).ToArray();
        int[] state = stateStack.Peek();

        int[] finalState = new int[state.Length];
        Array.Copy(state, finalState, finalState.Length);

        int removedPos = 0;
        while (removedPos < 9 * 9 - remainingDigits)
        {
            int curRemainingDigits = positions.Length - removedPos;
            int indexToPick = removedPos + rng.Next(curRemainingDigits);

            int row = positions[indexToPick] / 9;
            int col = positions[indexToPick] % 9;

            int blockRowToRemove = row / 3;
            int blockColToRemove = col / 3;

            if (removedPerBlock[blockRowToRemove, blockColToRemove] >= maxRemovedPerBlock)
                continue;

            removedPerBlock[blockRowToRemove, blockColToRemove] += 1;

            int temp = positions[removedPos];
            positions[removedPos] = positions[indexToPick];
            positions[indexToPick] = temp;

            int rowToWrite = row + row / 3 + 1;
            int colToWrite = col + col / 3 + 1;

            board[rowToWrite][colToWrite] = '.';

            int stateIndex = 9 * row + col;
            state[stateIndex] = 0;

            removedPos += 1;
        }
        
        return new Board(board, stateStack, finalState);
    }
}