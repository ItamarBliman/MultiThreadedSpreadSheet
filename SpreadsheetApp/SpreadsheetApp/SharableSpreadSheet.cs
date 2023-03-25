using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class SharableSpreadSheet
{
    public List<List<Cell>> matrix;
    private Semaphore semaphore_search;
    private int nUsers;
    private Semaphore semaphore_change;

    public SharableSpreadSheet(int nRows, int nCols, int nUsers=-1)
    {
        // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
        // construct a nRows*nCols spreadsheet

        int row, col;

        if(nRows < 1 || nCols < 1)
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");

        matrix = new List<List<Cell>>(nRows);
        for (row = 0; row < nRows; row++)
        {
            matrix.Add(new List<Cell>(nCols));
            for (col = 0; col < nCols; col++)
                matrix[row].Add(new Cell());
        }
        semaphore_change = new Semaphore(1, 1);

        setConcurrentSearchLimit(nUsers);
    }
    public String getCell(int row, int col)
    {
        // return the string at [row,col]

        if (row < 0 || row >= matrix.Count || col < 0 || col >= matrix[0].Count)
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        String cellString;
        Cell cell = matrix[row][col];

        Monitor.Enter(cell);
        cellString = cell.value;
        Monitor.Exit(cell);

        return cellString;
    }
    public void setCell(int row, int col, String str)
    {
        // set the string at [row,col]

        if (row < 0 || row >= matrix.Count || col < 0 || col >= matrix[0].Count)
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");

        Cell cell = matrix[row][col];

        Monitor.Enter(cell);
        cell.value = str;
        Monitor.Exit(cell);
    }
    public Tuple<int,int> searchString(String str)
    {
        // return first cell indexes that contains the string (search from first row to the last row)
        return searchInRange(0, matrix[0].Count - 1, 0, matrix.Count - 1, str);
    }
    public void exchangeRows(int row1, int row2)
    {
        // exchange the content of row1 and row2

        semaphore_change.WaitOne();

        if (row1 < 0 || row1 >= matrix.Count || row2 < 0 || row2 >= matrix.Count)
        {
            semaphore_change.Release();
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        }

        String temp;
        for(int i = 0; i < matrix[0].Count; i++)
        {
            temp = getCell(row1, i);
            setCell(row1, i, getCell(row2, i));
            setCell(row2, i, temp);
        }
        semaphore_change.Release();
    }
    public void exchangeCols(int col1, int col2)
    {
        // exchange the content of col1 and col2

        semaphore_change.WaitOne();

        if (col1 < 0 || col1 >= matrix[0].Count || col2 < 0 || col2 >= matrix[0].Count)
        {
            semaphore_change.Release();
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        }

        String temp;
        for (int i = 0; i < matrix.Count; i++)
        {
            temp = getCell(i, col1);
            setCell(i, col1, getCell(i, col2));
            setCell(i, col1, temp);
        }

        semaphore_change.Release();
    }
    public int searchInRow(int row, String str)
    {
        // perform search in specific row

        Tuple<int, int> item = searchInRange(0, matrix[0].Count - 1, row, row, str);

        return item.Item2;
    }
    public int searchInCol(int col, String str)
    {
        // perform search in specific col

        Tuple<int, int> item = searchInRange(col, col, 0, matrix.Count - 1, str);

        return item.Item1;
    }
    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
    {
        int row, col;
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2

        if(semaphore_search != null)
            semaphore_search.WaitOne();

        for (row = row1; row <= row2; row++)
            for (col = col1; col <= col2; col++)
                if (str.Equals(getCell(row, col)))
                {
                    if (semaphore_search != null)
                        semaphore_search.Release();
                    return new Tuple<int, int>(row, col);
                }

        if (semaphore_search != null)
            semaphore_search.Release();

        throw new KeyNotFoundException("\"" + str + "\" couldn't be found in the spreadsheet.");
    }
    public void addRow(int row1)
    {
        //add a row after row1
        semaphore_change.WaitOne();

        if (row1 < 0 || row1 > matrix.Count)
        {
            semaphore_change.Release();
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        }

        List<Cell> list = new List<Cell>(matrix[0].Count);
        for (int col = 0; col < matrix[0].Count; col++)
            list.Add(new Cell());

        if (row1 != matrix.Count)
        {
            lockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);
            matrix.Insert(row1, list);
            unLockRange(0, matrix[0].Count - 1, 0, row1 - 1);
            unLockRange(0, matrix[0].Count - 1, row1 + 1, matrix.Count - 1);
        }
        else
            matrix.Insert(row1, list);

        semaphore_change.Release();
    }
    public void addCol(int col1)
    {
        //add a column after col1

        semaphore_change.WaitOne();

        if (col1 < 0 || col1 > matrix[0].Count)
        {
            semaphore_change.Release();
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        }

        if (matrix[0].Count == col1)
            for (int row = 0; row < matrix.Count; row++)
                matrix[row].Insert(col1, new Cell());
        else
        {
            lockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);
            for (int row = 0; row < matrix.Count; row++)
                matrix[row].Insert(col1, new Cell());
            unLockRange(0, col1 - 1, 0, matrix.Count - 1);
            unLockRange(col1 + 1, matrix[0].Count - 1, 0, matrix.Count - 1);
        }

        semaphore_change.Release();
    }
    public Tuple<int, int>[] findAll(String str, bool caseSensitive)
    {
        // perform search and return all relevant cells according to caseSensitive param

        List<Tuple<int, int>> listOfTuples = new List<Tuple<int, int>>();
        int row, col;

        if (semaphore_search != null)
            semaphore_search.WaitOne();

        for (row = 0; row < matrix.Count; row++)
            for (col = 0; col < matrix[0].Count; col++)
            {
                if (str == null)
                {
                    if (getCell(row, col) == null)
                        listOfTuples.Add(new Tuple<int, int>(row, col)); 
                }
                else if (caseSensitive)
                {
                    if (str.Equals(getCell(row, col)))
                        listOfTuples.Add(new Tuple<int, int>(row, col));
                }
                else if (str.Equals(getCell(row, col), StringComparison.OrdinalIgnoreCase))
                    listOfTuples.Add(new Tuple<int, int>(row, col));
            }

        if (semaphore_search != null)
            semaphore_search.Release();

        if (listOfTuples.Count == 0)
            throw new KeyNotFoundException("\"" + str + "\" couldn't be found in the spreadsheet.");

        return listOfTuples.ToArray();

    }
    public void setAll(String oldStr, String newStr, bool caseSensitive)
    {
        // replace all oldStr cells with the newStr str according to caseSensitive param

        Tuple<int, int>[] Tuples = findAll(oldStr, caseSensitive);

        for (int i = 0; i < Tuples.Length; i++)
            setCell(Tuples[i].Item1, Tuples[i].Item2, newStr);
    }
    public Tuple<int, int> getSize()
    {
        int nRows = matrix.Count, nCols = matrix[0].Count;
        // return the size of the spreadsheet in nRows, nCols
        return new Tuple<int, int> (nRows, nCols);
    }
    public void setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        // This function is used just in the creation

        if (nUsers < -1 || nUsers == 0)
            throw new ArgumentOutOfRangeException("Index out of spreadsheet range.");
        this.nUsers = nUsers;

        semaphore_search = null;
        if(nUsers != -1)
            semaphore_search = new Semaphore(nUsers, nUsers);
    }

    public void save(String fileName)
    {
        int row, col;

        semaphore_change.WaitOne();

        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        lockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);

        String str = matrix.Count + "," + matrix[0].Count + "," + nUsers + "\n";

        for (row = 0; row < matrix.Count; row++)
        {
            for (col = 0; col < matrix[0].Count; col++)
            {
                if(matrix[row][col].value != null)
                    str += matrix[row][col].value;
                str += "\t";
            }
            str += "\n";
        }

        unLockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);

        semaphore_change.Release();

        try
        {
            File.WriteAllText(fileName, str);
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException(ex.Message);
        }
    }
    public void load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
        int row, col, nrow, ncol, nusers;
        string readText;

        semaphore_change.WaitOne();

        try
        {
            readText = File.ReadAllText(fileName);
        }
        catch (Exception ex)
        {
            semaphore_change.Release();
            throw new FileNotFoundException(ex.Message);
        }

        string[] lines = readText.Split('\n'), line;

        line = lines[0].Split(',');
        nrow = int.Parse(line[0]);
        ncol = int.Parse(line[1]);
        nusers = int.Parse(line[2]);

        List<List<Cell>> newMatrix = new List<List<Cell>>(nrow), oldMatrix;
        for (row = 0; row < nrow; row++)
        {
            newMatrix.Add(new List<Cell>(ncol));
            line = lines[row + 1].Split('\t');
            for (col = 0; col < ncol; col++)
            {
                if(line[col].Equals(""))
                    newMatrix[row].Add(new Cell());
                else
                    newMatrix[row].Add(new Cell(line[col]));
            }
        }

        lockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);

        semaphore_change.Release();

        oldMatrix = matrix;
        matrix = newMatrix;
        setConcurrentSearchLimit(nusers);

        for (row = 0; row < oldMatrix.Count; row++)
            for (col = 0; col < oldMatrix[0].Count; col++)
                Monitor.Exit(oldMatrix[row][col]);
    }

    private void lockRange(int col1, int col2, int row1, int row2)
    {
        int row, col;
        for (row = row1; row <= row2; row++)
            for (col = col1; col <= col2; col++)
                Monitor.Enter(matrix[row][col]);
    }

    private void unLockRange(int col1, int col2, int row1, int row2)
    {
        int row, col;
        for (row = row1; row <= row2; row++)
            for (col = col1; col <= col2; col++)
                Monitor.Exit(matrix[row][col]);
    }

    override
    public String ToString()
    {
        int row, col;
        String str = "";

        lockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);

        for (row = 0; row < matrix.Count; row++)
        {
            for (col = 0; col < matrix[0].Count; col++)
            {
                if (matrix[row][col].value != null)
                    str += matrix[row][col].value;
                str += "\t";
            }
            str += "\n";
        }

        unLockRange(0, matrix[0].Count - 1, 0, matrix.Count - 1);

        return str;
    }

    public class Cell
    {
        public String value;

        public Cell(String str=null)
        {
            value = str;
        }

            public override string ToString()
        {
            return value;
        }
    }
}

