using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Formats.Asn1;
using System.IO.Compression;
//create Command
var bundleCommand = new Command("bundle", "bundle code files to a single file");
var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command");
var bundleOption = new Option<FileInfo>("--output", "file path & name");
var languageOption = new Option<string>("--language", "programming language")
{
    IsRequired = true,
};
var noteOption = new Option<bool>("--note", "Include source code note");
var sortOption = new Option<string>("--sort", "Sort the code files");
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines");
var authorOption = new Option<string>("--author", "Author ");

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

//aliases-Shortcuts
bundleOption.AddAlias("-o");
languageOption.AddAlias("-l");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeEmptyLinesOption.AddAlias("-r");
authorOption.AddAlias("-a");


noteOption.SetDefaultValue(false);
removeEmptyLinesOption.SetDefaultValue(false);
createRspCommand.SetHandler(() =>
{
    var file = new FileInfo("file.rsp");
    try
    {
        using (StreamWriter Writer = new StreamWriter(file.FullName))
        {
            Console.WriteLine("Enter file name");
            string output;
            output = Console.ReadLine();
            Writer.Write($"--output {output} ");
            Console.WriteLine("Enter language if you went to include every language enter all");       
            Writer.Write($"--language {Console.ReadLine()} ");

            Console.WriteLine("Include source code origin as a comment? (yes/no)");
            var note = Console.ReadLine();
            Writer.Write(note.Trim().ToLower() == "yes" ? "--note " : "");

            Console.WriteLine("Enter the sort order for code files ('name' or 'type'): ");
            Writer.Write($"--sort {Console.ReadLine()} ");

            Console.WriteLine("Remove empty lines? (yes/no)");
            var removeEmptyLinesInput = Console.ReadLine();
            Writer.Write(removeEmptyLinesInput.Trim().ToLower() == "yes" ? "--remove-empty-lines " : "");

            Console.WriteLine("Enter the name of author");
            Writer.Write($"--author {Console.ReadLine()}");
        }
        Console.WriteLine($"File was created");
    }
    catch (Exception)
    {
        Console.WriteLine($"Error creating response file");
    }
});



bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
{
    try
    {
        //current directory
        DirectoryInfo directory = new DirectoryInfo(".");
        FileInfo[] files;
        List<string> excludedDirectories = new List<string> { "bin", "debug" };
        files = directory.GetFiles()
            .Where(file => !excludedDirectories.Any(dir => file.FullName.ToLower().Contains(dir)))
            .ToArray();
        if (language.ToLower() == "all")
        {
            files = directory.GetFiles();
        }
        else
        {
            files = directory.GetFiles("*." + language);
        }
        if (!string.IsNullOrEmpty(sort))
        {

            if (sort.ToLower() == "type")
            {
                files = files.OrderBy(file => Path.GetExtension(file.Name)).ToArray();
            }
            else
            {
                //default order by name 
                files = files.OrderBy(file => file.Name).ToArray();
            }


        }

        string outputPath = output.FullName;
        // Check if the directory exists
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Error: Directory {outputDirectory} does not exist.");
            return;
        }
        using (StreamWriter writer = File.AppendText(outputPath))
        {
            if (files.Length == 0)
            {
                Console.WriteLine("Error: No files found");
                return;
            }
            foreach (var file in files)
            {
                if (!string.IsNullOrEmpty(author))
                {
                    writer.WriteLine($"// Author: {author}");
                }
                string code = File.ReadAllText(file.FullName);
                writer.WriteLine($"// File: {file.Name}");
               

                if (note)
                {
                    writer.WriteLine($" //Source code : {file.FullName}");

                }
                if (removeEmptyLines)
                {
                    // remove empty lines
                    code = string.Join(Environment.NewLine, code.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                }
                writer.WriteLine(code);
            }

            Console.WriteLine("File was created");
        }

    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: file path not found" + " " + ex.Message);
    }
}, bundleOption, languageOption, noteOption, sortOption,removeEmptyLinesOption,authorOption);

var rootCommand = new RootCommand("RootCommand for file bundle cli");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);




