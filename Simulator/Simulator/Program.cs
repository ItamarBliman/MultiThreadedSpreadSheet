namespace Simulator
{
    internal class Program
    {
        private static SharableSpreadSheet sharableSpread;
        private static int rows;
        private static int cols;
        private static int nThreads;
        private static int nOperations;
        private static int mssleep;
        private static Random rnd;
        private static Boolean debugging = true;

        static void Main(string[] args)
        {
            argscheck(args);

            int i, row, col;

            rows = Int32.Parse(args[0]);
            cols = Int32.Parse(args[1]);
            nThreads = Int32.Parse(args[2]);
            nOperations = Int32.Parse(args[3]);
            mssleep = Int32.Parse(args[4]);

            rnd = new Random();

            sharableSpread = new SharableSpreadSheet(rows, cols);

            for (row = 0; row < rows; row++)
                for (col = 0; col < cols; col++)
                    sharableSpread.setCell(row, col, "testcell" + row.ToString() + col.ToString());

            Thread[] threads = new Thread[nThreads];

            Console.WriteLine(sharableSpread.ToString());

            for (i = 0; i < nThreads; i++)
            {
                threads[i] = new Thread(Threadadd);
                threads[i].Start();
            }

            for (i = 0; i < nThreads; i++)
                threads[i].Join();

            Console.WriteLine(sharableSpread.ToString());
        }


        private static void Threadadd()
        {
            int i, num, op1, op2, op3, op4;
            String time, comment="", temp;
            Tuple<int, int> tempTuple;
            for(i = 0; i < nOperations; i++)
            {
                num = rnd.Next(13);
                time = DateTime.Now.ToString("h:mm:ss tt");
                try
                {
                    switch (num)
                    {
                        case 0:
                            comment = "string \"" + sharableSpread.getCell(op1 = rnd.Next(rows), op2 = rnd.Next(cols)) + "\" is in cell [" + op1 + "," + op2 + "].";
                            break;
                        case 1:
                            sharableSpread.setCell(op1=rnd.Next(rows), op2=rnd.Next(cols), temp=("setCellTest" + i));
                            comment = "string \"" + temp + "\" inserted to cell [" + op1 + "," + op2 + "].";
                            break;
                        case 2:
                            tempTuple = sharableSpread.searchString(temp=("searchString" + i.ToString() + i.ToString()));
                            comment = "string \"" + temp + "\" found after searchString in cell [" + tempTuple.Item1 + "," + tempTuple.Item2 + "].";
                            break;
                        case 3:
                            sharableSpread.exchangeRows(op1=rnd.Next(rows), op2=rnd.Next(rows));
                            comment = "rows [" + op1 + "] and [" + op2 + "] exchanged successfully.";
                            break;
                        case 4:
                            sharableSpread.exchangeCols(op1=rnd.Next(cols), op2=rnd.Next(cols));
                            comment = "cols [" + op1 + "] and [" + op2 + "] exchanged successfully.";
                            break;
                        case 5:
                            op2 = sharableSpread.searchInRow(op1=rnd.Next(rows), temp=("testcell" + i.ToString() + i.ToString()));
                            comment = "string \"" + temp + "\" found after searchInRow in cell [" + op1 + "," + op2 + "].";
                            break;
                        case 6:
                            op1 = sharableSpread.searchInCol(op2=rnd.Next(cols), temp=("testcell" + i.ToString() + i.ToString()));
                            comment = "string \"" + temp + "\" found after searchInCol in cell [" + op1 + "," + op2 + "].";
                            break;
                        case 7:
                            tempTuple = sharableSpread.searchInRange(op1=rnd.Next(cols), op2=rnd.Next(cols), op3=rnd.Next(rows), op4=rnd.Next(rows), temp=("testcell" + i.ToString() + i.ToString()));
                            comment = "string \"" + temp + "\" found after searchInRange in cell [" + tempTuple.Item1 + "," + tempTuple.Item2 + "].";
                            break;
                        case 8:
                            sharableSpread.addRow(op1=rnd.Next(rows));
                            comment = "a new row was added after row [" + op1 + "].";
                            break;
                        case 9:
                            sharableSpread.addCol(op1=rnd.Next(cols));
                            comment = "a new col was added after col [" + op1 + "].";
                            break;
                        case 10:
                            comment = "string \"" + "testcell" + i.ToString() + i.ToString() + "\" found after findAll in: " + string.Join(",", sharableSpread.findAll("testcell" + i.ToString() + i.ToString(), true).Select(t => String.Format("[{0},{1}]", t.Item1, t.Item2))) + ".";
                            break;
                        case 11:
                            sharableSpread.setAll("testcell" + i.ToString() + i.ToString(), "testcellNew" + i.ToString() + i.ToString(), false);
                            comment = "string \"" + "testcell" + i.ToString() + i.ToString() + "\" was changed to \"" + "testcellNew" + i.ToString() + i.ToString() + "\" with setAll.";
                            break;
                        case 12:
                            tempTuple = sharableSpread.getSize();
                            comment = "spreadsheet size is rows=" + tempTuple.Item1 + ", cols=" + tempTuple.Item2 + ".";
                            break;
                    }
                    if(debugging)
                        Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: [" + time + "] " + comment);
                }
                catch (KeyNotFoundException ex)
                {
                    if (debugging)
                        Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: [" + time + "] " + ex.Message);
                }
                Thread.Sleep(mssleep);
            }
        }


private static void argscheck(string[] args)
        {
            int rows, cols, nThreads, nOperations, mssleep;

            if (args.Length != 5)
                throw new ArgumentOutOfRangeException("The number of arguments must be 5");
            try
            {
                rows = Int32.Parse(args[0]);
                cols = Int32.Parse(args[1]);
                nThreads = Int32.Parse(args[2]);
                nOperations = Int32.Parse(args[3]);
                mssleep = Int32.Parse(args[4]);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("The arguments must be integers");
            }
            if (rows <= 0 || cols <= 0 || nThreads <= 0 || nOperations <= 0 || mssleep <= 0)
            {
                throw new ArgumentOutOfRangeException("The arguments must be a positive integer");
            }
        }
    }
}
