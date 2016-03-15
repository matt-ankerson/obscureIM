using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obscureIM_client
{

    public class ConsoleHelper
    {
        private int outCol, outRow, outHeight;

        public ConsoleHelper(int outCol, int outRow, int outHeight)
        {
            this.outCol = outCol;
            this.outRow = outRow;
            this.outHeight = outHeight;
        }

        public void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public void WriteOut(string msg, bool appendNewLine)
        {
            //----------------------------------------
            // Credit to BlueMonkMN of StackOverflow
            // http://stackoverflow.com/questions/849876/c-sharp-simultanous-console-input-and-output

            int inCol, inRow;
            inCol = Console.CursorLeft;
            inRow = Console.CursorTop;

            int outLines = getMsgRowCount(outCol, msg) + (appendNewLine ? 1 : 0);
            int outBottom = outRow + outLines;
            if (outBottom > outHeight)
                outBottom = outHeight;
            if (inRow <= outBottom)
            {
                int scrollCount = outBottom - inRow;
                Console.MoveBufferArea(0, inRow, Console.BufferWidth, 1, 0, inRow + scrollCount);
                inRow += scrollCount;
            }
            if (outRow + outLines > outHeight)
            {
                int scrollCount = outRow + outLines - outHeight;
                Console.MoveBufferArea(0, scrollCount, Console.BufferWidth, outHeight - scrollCount, 0, 0);
                outRow -= scrollCount;
                Console.SetCursorPosition(outCol, outRow);
            }
            Console.SetCursorPosition(outCol, outRow);
            if (appendNewLine)
                Console.WriteLine(msg);
            else
                Console.Write(msg);
            outCol = Console.CursorLeft;
            outRow = Console.CursorTop;
            Console.SetCursorPosition(inCol, inRow);
        }

        private static int getMsgRowCount(int startCol, string msg)
        {
            string[] lines = msg.Split('\n');
            int result = 0;
            foreach (string line in lines)
            {
                result += (startCol + line.Length) / Console.BufferWidth;
                startCol = 0;
            }
            return result + lines.Length - 1;
        }
    }
}
