using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CopyEmbeddedFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var targetDirectoryPath = @"C:\copyembeddedfiletest";

            var embeddedResources = new EmbeddedResources();

            var resourceNames = embeddedResources.List();

            //var assembly = embeddedResources.GetType().Assembly;
            //var exeDirectory = Path.GetDirectoryName(embeddedResources.GetType().Assembly.Location);
            var assembly = Assembly.GetEntryAssembly();

            if (assembly is null) throw new Exception("Assembly is null.");

            foreach (var resourceName in resourceNames)
            {
                using var resourceStream = assembly.GetManifestResourceStream(resourceName);
                    
                if (resourceStream is null) throw new Exception("Resource stream is null.");

                if (!Directory.Exists(targetDirectoryPath))
                {
                    Directory.CreateDirectory(targetDirectoryPath);
                }

                var resourceFileName = _GetFileNameFromResourceName(resourceName);

                var path = Path.Combine(targetDirectoryPath, resourceFileName);

                using var reader = new StreamReader(resourceStream, Encoding.UTF8);
                using var outputFileStream = new FileStream(path, FileMode.Create);
                
                Debug.WriteLine($"Writing {path}");

                reader.BaseStream.CopyTo(outputFileStream);

                //using (Stream outputStream = File.OpenWrite(Path.Combine(exeDirectory, fileName)))
                //{
                //    resourceStream.CopyTo(outputStream);
                //}
            }

            Debug.WriteLine("All done.");
        }

        private static string _GetFileNameFromResourceName(string resourceName)
        {
            // NOTE: this code assumes that all of the file names have exactly one
            // extension separator (i.e. "dot"/"period" character). I.e. all file names
            // do have an extension, and no file name has more than one extension.
            // Directory names may have periods in them, as the compiler will escape these
            // by putting an underscore character after them (a single character
            // after any contiguous sequence of dots). IMPORTANT: the escaping
            // is not very sophisticated; do not create folder names with leading
            // underscores, otherwise the dot separating that folder from the previous
            // one will appear to be just an escaped dot!

            StringBuilder sb = new StringBuilder();
            bool escapeDot = false, haveExtension = false;

            for (int i = resourceName.Length - 1; i >= 0; i--)
            {
                if (resourceName[i] == '_')
                {
                    escapeDot = true;
                    continue;
                }

                if (resourceName[i] == '.')
                {
                    if (!escapeDot)
                    {
                        if (haveExtension)
                        {
                            sb.Append('\\');
                            continue;
                        }
                        haveExtension = true;
                    }
                }
                else
                {
                    escapeDot = false;
                }

                sb.Append(resourceName[i]);
            }

            string fileName = Path.GetDirectoryName(sb.ToString());

            fileName = new string(fileName.Reverse().ToArray());

            return fileName;
        }
    }

    public class EmbeddedResources
    {
        public string[] List()
        {
            var resourceNames = this.GetType().Assembly.GetManifestResourceNames();

            return resourceNames;
        }
    }
}

    
