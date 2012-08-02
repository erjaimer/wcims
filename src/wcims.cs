using System.Collections;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
///@brief Main class.
//http://www.csharp-examples.net/file-attributes/
//http://msdn.microsoft.com/en-us/library/system.io.fileattributes.aspx
public class Wcims
{

	private String _dir;
	private String _separator = ":::";
	private String _dirSeparator = "/";
	private ArrayList _list = new ArrayList();
	private String fileOutput = ""; // is equal _dir + _dirSeparator + ( _dir dirname ) + .txt
	
	///@brief Constructor.
	///@param dir is a Directory.
	///@bug not check if is a directory.
	///@bug _separator must depend to OS, in win32 is '\\' in Unix* are '/'.
	///@bug find method of String class or Directory class for to select dirname.
	public Wcims( String dir )
	{
		_dir = dir;
		string[] path = Regex.Split( dir , _dirSeparator); // separate path directory by _separator
		fileOutput = dir + _dirSeparator + path[ path.Length -1] + ".txt";// select dirName
	}

	///@brief Calculate md5sum to File.
	///source: http://sharpertutorials.com/calculate-md5-checksum-file/
	///@param fileName path to file.
	///@bug not checked if exists or not have read permissed .
	protected string GetMD5HashFromFile(string fileName)
	{
	  FileStream file = new FileStream(fileName, FileMode.Open);
	  MD5 md5 = new MD5CryptoServiceProvider();
	  byte[] retVal = md5.ComputeHash(file);
	  file.Close();
	  StringBuilder sb = new StringBuilder();
	  for (int i = 0; i < retVal.Length; i++)
	  {
	    sb.Append(retVal[i].ToString("x2"));
	  }
	  return sb.ToString();
	}

	///@brief detect whether its a directory or file.
	bool isDirectory(String path)
	{
		FileAttributes attr = File.GetAttributes(path);
		return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
	}
	
	///@brief add in this._list all files, directories and subdirectories below of this._dir
	///@call  proccessFolder(String)
	public void proccessFolder()
	{
		proccessFolder(_dir);
	}

	///@brief It's recursive function and it add in  this._list all files, directories and subdirectories below of this._dir.
	///@param path is a Directory path.
	///@bug not check if path is a directory.
	protected void proccessFolder(String directory)
	{
		String[] filePaths = Directory.	GetFileSystemEntries(directory);//find all directory,subdirectory and files to path
		try
		{
			foreach ( String path in filePaths )
			{
				_list.Add( path  );//add to list folder or file
				if( isDirectory( path ) ) // if is a directory then call to function again recursively
					proccessFolder( path );
			}
		}
		catch(System.IO.IOException)
		{
		    Console.WriteLine("exception:()");
		}
	}
	
	///@brief Write _list in a file, it's file will written in this._dir path with name to dir.
	///@bug not checked write permissed.
	///@bug not checked if exists file with equal name.
	public void writeFile()
	{
        TextWriter tw = new StreamWriter( fileOutput );
        Console.WriteLine("Writting file {0}",fileOutput);
        foreach( String path in _list ) // process array files and calculate md5
		{
			//Console.WriteLine("Writting file {0}",path);
			String sha1 = ( ! isDirectory( path ))?GetMD5HashFromFile( path ):"d"; // if is a directory not calculate md5
			tw.WriteLine("{0}{1}{2}",path,_separator,sha1); // write in file with separator
		}
		Console.WriteLine("Done!!");
        tw.Close();
	}
	
	///@brief check if directory has been changed.
	///@bug not checked if fileOutput if readable.
	///@bug file separator in windows entry is \\r\\n
	public bool checkDir()
	{
		if( !File.Exists( fileOutput ) )
			return false;
		ArrayList errors = new ArrayList();
		TextReader tr = new StreamReader(fileOutput);
		Console.WriteLine("Read {0} ... ",_dir);
		string[] lines = Regex.Split( tr.ReadToEnd() , "\n"); //the entry file is separate by  \\n
		foreach (string file in lines)
		{
			Console.WriteLine("PROCESS:{0}", file);
			String[] aux = Regex.Split( file , _separator); 
			if( aux.Length != 2 ) // would be warning or failure!! but it's ignored
				continue;
			String filepath = aux[0]; // filepath
			String md5 = aux[1];// md5 or d if is a directory
			//-----
			//	process directory for to find errors or changes
			//-----
			if( md5 == "d" && !isDirectory( filepath ) )
				errors.Add("\tDirectory " + filepath + " not exists" );
			else if( md5 == "d" && !Directory.Exists( filepath )  ) 
				errors.Add("\tDirectory " + filepath + " has been changed not is a directoy" );
			else if( md5 != "d" && !File.Exists( filepath)  )
				errors.Add("\tFile " + filepath +  "not exists" );
			else if( md5 != "d" && md5 != GetMD5HashFromFile( filepath ) )
				errors.Add("\tFile " + filepath + " not has been changed, not hash equal" );				
		}
		Console.WriteLine("Done!!");
		tr.Close();
		//show erros if exists
		Console.WriteLine("Errors: ");
		if( errors.Count == 0 )
			Console.WriteLine("not Errors has been found!!");
		else
			foreach( string error in errors) //show erros by stdout
				Console.WriteLine("{0}",error);
		return true;
	}
	static public void Main (String[] args)
	{
		
		if( args.Length < 1 || args.Length > 2 )
		{
			Console.WriteLine("expected 1 or 2 arguments, -h for more information");
			return;
		}
		switch( args[0] )
		{
			case "-h":
			case "--help":
				Console.WriteLine
				(
					"Uso:\n\twcims option folder"
					+"option:\n"
					+"\t-h | --help help: show this help and exit\n"
					+"\t-c | --check : save a file, format= path:md5 \n"
					+"\t-s | --save : check a directory by file\n"
				);
			break;
			case "-s":
			case "--save":
				if( args.Length != 2 )
				{
					Console.WriteLine("folder expected");
					return;
				}
				Wcims s = new Wcims(args[1]);
				s.proccessFolder();
				s.writeFile();
			break;
			case "-c":
			case "--check":
				if( args.Length != 2 )
				{
					Console.WriteLine("Folder expected");
					return;
				}
				Wcims s2 = new Wcims(args[1]);
				s2.checkDir();
			break;
			default:
				Console.WriteLine("Error expected arguments: -s | --save | -h | --help  | -c  | --check");
			break;
		}
	
	}
}
