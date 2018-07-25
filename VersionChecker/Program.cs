using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersionChecker.Models;

namespace VersionChecker
{
    class Program
    {
       
        private static string createText = string.Empty;
        static ConcurrentDictionary<string, AssemblyStructureModel> assemblyModelCollection = new ConcurrentDictionary<string, AssemblyStructureModel>();

        public static void Main()
        {
            Console.WriteLine("Give the input dll file path");
            var filePath = Console.ReadLine();
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Add a single parameter that is your" +
                " path to the file you want inspected.");
            }
            try
            {
                Console.WriteLine("Give the output folder path");
                var outputPath = Console.ReadLine();
                outputPath = outputPath.Trim('\\');

                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    DirectoryInfo d = new DirectoryInfo(filePath);
                    FileInfo[] Files = d.GetFiles("*.dll");

                    foreach (FileInfo file in Files)
                    {
                        if (file.Name.ToLower().StartsWith("servicemanager"))
                        {
                            var tempFile = filePath.Trim('\\') + "\\" + file.Name;
                            var tempAssembly = Assembly.LoadFile(tempFile);
                            var assemblies = tempAssembly.GetReferencedAssemblies();

                            createText = createText + Environment.NewLine;

                            var parentAssemblyName = tempAssembly.GetName().FullName.Split(',')[0];

                            createText += Environment.NewLine + parentAssemblyName.ToUpper() + ":" + Environment.NewLine;

                            if (assemblies.GetLength(0) > 0)
                            {
                                foreach (var assembly in assemblies)
                                {
                                    createText = createText + Environment.NewLine + assembly;

                                    UpdateAssemblyModelCollection(assembly.Name, assembly.Version.ToString(), parentAssemblyName);

                                }
                            }

                        }

                    }



                    Console.WriteLine("Generating report 1");
                    WriteToFile(string.Format(@"{0}\report1.txt", outputPath), createText);
                    Console.WriteLine("Finished generating report 1");

                    var conflictReport = GenerateConflictReport(outputPath);
                    Console.WriteLine("Generating report 2");
                    WriteToFile(string.Format(@"{0}\report2.txt", outputPath), conflictReport);
                    Console.WriteLine("Finished generating report 2");
                }

                else
                {
                    Console.WriteLine("Invalid Output path");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred: {0}", e.Message);
            }
            finally { }

            Console.ReadLine();

        }



        private static string GenerateConflictReport(string outputPath)
        {
            var conflictReport = string.Empty;
            try
            {

                if (assemblyModelCollection != null && assemblyModelCollection.Any())
                {
                    foreach (var assembly in assemblyModelCollection)
                    {
                        var references = assembly.Value.References;
                        if (references != null && references.Keys.Count() > 0)
                        {

                            var groupingCount = references.GroupBy(s => s.Value).Count();
                            if (groupingCount > 1)
                            {
                                var groupingList = references.GroupBy(s => s.Value).ToList();

                                conflictReport = conflictReport + Environment.NewLine;
                                conflictReport += assembly.Key + ":" + Environment.NewLine;

                                var treeNode = new List<TreeNode>();

                                foreach (var listItem in groupingList)
                                {
                                    if(!listItem.Key.StartsWith("2018.2"))
                                    {
                                        var tempNode = new TreeNode() { ParentNode = listItem.Key };
                                        foreach (var tempRef in listItem)
                                        {
                                            if (tempNode != null && tempNode.ChildNodes != null && tempNode.ChildNodes.Any())
                                            {
                                                tempNode.ChildNodes.Add(tempRef.Key);
                                            }
                                            else
                                            {
                                                tempNode.ChildNodes = new List<string>() { tempRef.Key };
                                            }
                                            conflictReport += tempRef.Key + ", Version: " + tempRef.Value + Environment.NewLine;
                                        }

                                        treeNode.Add(tempNode);
                                    }
                                    
                                }

                                HtmlReportGenerator.GenerateReport2(assembly.Key, treeNode, outputPath);

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                conflictReport = $"Issue int generating conflict report: {ex.ToString()}";
            }

            return conflictReport;
        }


        private static bool WriteToFile(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with exception:" + ex.ToString());
            }

            return false;
        }

        private static void UpdateAssemblyModelCollection(string assemblyName, string version, string parentAssemblyName)
        {
            if (assemblyModelCollection != null && assemblyModelCollection.Any())
            {
                var existingAssemblyModel = assemblyModelCollection.Where(s => s.Key.Equals(assemblyName.ToUpper())).FirstOrDefault();
                if (existingAssemblyModel.Value != null)
                {
                    existingAssemblyModel.Value.Name = assemblyName;
                    existingAssemblyModel.Value.References.Add(parentAssemblyName, version);
                    existingAssemblyModel.Value.Count = existingAssemblyModel.Value.Count + 1;
                }
                else
                {
                    var reference = new Dictionary<string, string>() { };
                    reference.Add(parentAssemblyName, version);

                    assemblyModelCollection.TryAdd(assemblyName.ToUpper(), new AssemblyStructureModel() { Name = assemblyName, References = reference, Count = 1 });

                }
            }
            else
            {
                var reference = new Dictionary<string, string>() { };
                reference.Add(parentAssemblyName, version);

                assemblyModelCollection.TryAdd(assemblyName.ToUpper(), new AssemblyStructureModel() { Name = assemblyName, References = reference, Count = 1 });
            }
        }
    
    }
}
