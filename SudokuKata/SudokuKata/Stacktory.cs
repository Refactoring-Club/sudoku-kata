namespace SudokuKata;

class Stacktory
{
    public static (Stack<int[]> stateStack, Stack<int> rowIndexStack, Stack<int> colIndexStack, Stack<bool[]> usedDigitsStack, Stack<int> lastDigitStack)
        StackFactory()
    {
        // Top element is current state of the board
        Stack<int[]> stateStack = new Stack<int[]>();

        // Top elements are (row, col) of cell which has been modified compared to previous state
        Stack<int> rowIndexStack = new Stack<int>();
        Stack<int> colIndexStack = new Stack<int>();

        // Top element indicates candidate digits (those with False) for (row, col)
        Stack<bool[]> usedDigitsStack = new Stack<bool[]>();

        // Top element is the value that was set on (row, col)
        Stack<int> lastDigitStack = new Stack<int>();
        return (stateStack, rowIndexStack, colIndexStack, usedDigitsStack, lastDigitStack);
    }
}