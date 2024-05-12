using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Text;


var languageOption = new Option<string>(new[] { "--language", "-l" }, "programming language to include in the bundle")
{
    IsRequired = true,
};
var outputOption = new Option<string>(new[] { "--output", "-o" }, "File name");
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Note Adding the source of the page");
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var sortOption = new Option<bool>(new[] { "--sort", "-s" }, "File copy sorting");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "name's author of file");
var removeLinesOption = new Option<bool>(new[] { "--removeLines", "-r" }, "Deleting empty rows");
var createRspCommand = new Command("create-rsp", "Create a response file with a ready command");

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeLinesOption);
static string RemoveEmptyLines(string filePath)
{
    try
    {
        string[] lines =System.IO.File.ReadAllLines(filePath);
        lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        string updatedFileContent = string.Join(Environment.NewLine, lines);
        System.IO.File.WriteAllText(filePath, updatedFileContent);
        return updatedFileContent;
}
    catch (Exception ex)
    {
        return null;
    }
}

bundleCommand.SetHandler((output, language, note, sort, author, removeLines) =>
{
    try
    {

        string newfile = output + ".txt";
        using (StreamWriter outputFile = new StreamWriter(newfile))
        {
            if (string.IsNullOrEmpty(output))                                                                                 //בדיקה שאכן הכניס ערך לoutput
            {
                throw new InvalidOperationException("The --output option is required. Please provide a valid output file name.");
            }
            string[] selectedLanguages = new[] { "c#", "python", "javascript", "java" };
            string[] LanguageSuffix = new[] { "cs", "py", "js", "java" };
            string fileContents = "";
            string filePath = "";
            if (language.ToLower() == "all")
            {
                string[] files;
                string directoryPath = Directory.GetCurrentDirectory();
                if (sort)                                                                                               //האם לשנות את אופן המיון
                {
                    //מיון לפי סיומת
                    files = Directory.GetFiles(directoryPath).OrderBy(file => Path.GetExtension(file), StringComparer.OrdinalIgnoreCase).ToArray();
                    foreach (string file in files)
                    {
                        Console.WriteLine(file);
                    }
                }
                else
                {
                    //מיון ABC
                    files = Directory.GetFiles(directoryPath).OrderBy(file => file).ToArray();
                    foreach (string file in files)
                    {
                        Console.WriteLine(file);
                    }
                }
                if (!(string.IsNullOrEmpty(author)))
                {
                    outputFile.WriteLine($"author: {author}\n");

                }
                foreach (string file in files)
                {
                    if (note)
                    {
                        outputFile.WriteLine($"File: {file}\n");
                    }
                    filePath = Path.GetFullPath(file);
                    if (removeLines)
                    {
                        fileContents = RemoveEmptyLines(filePath);
                    }
                    else
                    {
                        fileContents = System.IO.File.ReadAllText(filePath);

                    }
                    outputFile.WriteLine($"{fileContents}\n");
                }
                Console.WriteLine($"The files were bundled successfully into {newfile}");
            }

            else if (selectedLanguages.Contains(language.ToLower()))
            {
                //רשימת קבצים מהתקיה הנוכחית
                var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
                int indx = -1;
                for (int i = 0; i < selectedLanguages.Length; i++)
                {
                    if (selectedLanguages[i].ToLower() == language.ToLower())
                    {
                        indx = i;
                        break;
                    }
                }
           
                string[] files = Directory.GetFiles(Environment.CurrentDirectory, $"*.{LanguageSuffix[indx]}");
                if (!(string.IsNullOrEmpty(author)))
                {
                    outputFile.WriteLine($"author: {author}\n");

                }

                foreach (string fileTemp in files)
                {

                    if (note)
                    {
                        outputFile.WriteLine($"File: {fileTemp}\n");
                    }
                    filePath = Path.GetFullPath(fileTemp);

                    if (removeLines)
                    {
                        fileContents = RemoveEmptyLines(filePath);
                    }
                    else
                    {
                        fileContents = System.IO.File.ReadAllText(filePath);

                    }
                    outputFile.WriteLine($"{fileContents}\n");
                }

                Console.WriteLine($"The files were bundled successfully into {output}");
            }
            else
            {
              Console.WriteLine("Error: The language is incorrect");
            }
            outputFile.Close();
        }
    }

    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: File path is invalid");
    }
    catch (IOException ex)
    {
        // Handle the exception here
        Console.WriteLine("");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine("Insert --output");
    }
}, outputOption, languageOption, noteOption, sortOption, authorOption, removeLinesOption);

createRspCommand.SetHandler(() =>
{
    var responseFileContent = new StringBuilder();

    Console.WriteLine("Please provide the desired values for each option:");

    Console.Write("Language (--language, -l): ");
    var languageValue = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(languageValue))
    {
        Console.WriteLine("Invalid language value. Please provide a valid programming language.");
        return;
    }

    var knownLanguages = new List<string> { "c#", "python", "javascript", "java", "Java", "Python", "JavaScript", "C#", "all", "All" };
    var lowercaseLanguageValue = languageValue.ToLower();
    if (!knownLanguages.Contains(lowercaseLanguageValue) && lowercaseLanguageValue != "all")
    {
        Console.WriteLine("Unknown programming language. Please provide a valid programming language.");
        return;
    }

    Console.WriteLine("Entered language value: " + languageValue);
    responseFileContent.AppendLine($"--language {languageValue}");

    Console.Write("Output (--output, -o): ");
    var outputValue = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(outputValue))
    {
        Console.WriteLine("Invalid output value. Please provide a valid output.");
        return;
    }

    Console.WriteLine("Entered output value: " + outputValue);
    responseFileContent.AppendLine($"--output {outputValue}");

    Console.Write("Note (--note, -n): ");
    var noteValue = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(noteValue))
    {
        Console.WriteLine("Entered note value: " + noteValue);
        responseFileContent.AppendLine($"--note {(noteValue.ToLower() == "true" ? "true" : "false")}");
    }

    Console.Write("Sort (--sort, -s): ");
    var sortValue = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(sortValue))
    {
        Console.WriteLine("Entered sort value: " + sortValue);
        responseFileContent.AppendLine($"--sort {(sortValue.ToLower() == "true" ? "true" : "false")}");
    }

    Console.Write("Author (--author, -a): ");
    var authorValue = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(authorValue))
    {
        Console.WriteLine("Entered author value: " + authorValue);
        responseFileContent.AppendLine($"--author {authorValue}");
    }

    Console.Write("Remove Lines (--removeLines, -r): ");
    var removeLinesValue = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(removeLinesValue))
    {
        Console.WriteLine("Entered remove lines value: " + removeLinesValue);
        responseFileContent.AppendLine($"--removeLines {(removeLinesValue.ToLower() == "true" ? "true" : "false")}");
    }

    // שמירת response file
    System.IO.File.WriteAllText("response.rsp", responseFileContent.ToString());

    Console.WriteLine("Response file created successfully!");
});
var rootCommand = new RootCommand("Root command for file Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args).Wait();