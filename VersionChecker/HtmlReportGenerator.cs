using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using VersionChecker.Models;

namespace VersionChecker
{
    public class HtmlReportGenerator
    {
        private static string HtmlString = "<html> <head>   <meta charset=\"utf-8\">    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\">    <meta name=\"viewport\" content=\"width=device-width\">    <title> Dependency Graph</title><link rel=\"stylesheet\" href=\"http://fperucic.github.io/treant-js/Treant.css\">    <link rel =\"stylesheet\" href=\"https://s3.ap-south-1.amazonaws.com/portfoliotheme/assets/css/example7.css\"></head><body>    <div class=\"chart\" id=\"OrganiseChart6\"></div>    <script src=\"http://fperucic.github.io/treant-js/vendor/raphael.js\" ></script>    <script src =\"http://fperucic.github.io/treant-js/Treant.js\"></script>    <script> new Treant(tree_structure);    </script></body></html>";
        public static void GenerateReport2(string assemblyName, List<TreeNode> treeNodeList, string outputPath)
        {
            try
            {
                var doc = new HtmlDocument();
                var node = HtmlNode.CreateNode(HtmlString);
                doc.DocumentNode.AppendChild(node);


                var treeScript = "<script> var tree_structure = { \"chart\":{ \"container\":\"#OrganiseChart6\",        \"levelSeparation\": 60, \"siblingSeparation\": 100,        \"subTeeSeparation\": 100,\"rootOrientation\": \"WEST\",\"node\":{\"HTMLclass\": \"tennis-draw\",\"drawLineThrough\": true},\"connectors\":{ \"type\": \"straight\",\"style\": {                            \"stroke-width\": 2,                \"stroke\": \"#ccc\"            }                    }                }, ";
                var nodeStructure = "\"nodeStructure\" : {\"text\":{\"name\": { \"val\": " + "\"" + assemblyName + "\"" + "}},  \"HTMLclass\": \"winner\", \"children\":[";

                foreach (var treeNode in treeNodeList)
                {
                    nodeStructure += "{ \"text\":{\"name\":" + "\"" + treeNode.ParentNode + "\"" + "}, \"children\":[";

                    foreach (var childNode in treeNode.ChildNodes)
                    {
                        nodeStructure += "{ \"text\":{\"name\":" + "\"" + childNode + "\"" + "}},";
                    }

                    nodeStructure = nodeStructure.Trim(',') + "]},";
                }

                nodeStructure = nodeStructure.Trim(',') + "]}};";

                treeScript = treeScript + nodeStructure + "</script>";

                HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");

                var newNode = HtmlNode.CreateNode(treeScript);

                // Add new node as first child of body
                body.PrependChild(newNode);

                doc.Save(outputPath + "\\" + assemblyName + ".html");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at GenerateReport2 :" + ex.ToString());
            }
        }
    }
}