using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace dimensionalize
{
    class Program
    {
        // Recursively generate all permutations from a given depth.
        static void Permutation(string key, string[] state, List<string[]> values, Dictionary<string, string[]> map, int depth)
        {
            // Initialize inputs.
            if (key == null) key = "";
            if (state == null) state = new string[values.Count];

            // Determine current level being worked on.
            string[] current = values[depth];

            // For each of the values at the current level.
            for(int i = 0; i < current.Length; i++)
            {
                string[] match = current[i].Split('=');
                string matchKey = match[0];
                string matchMappedValue = (match.Length > 1) ? match[1] : matchKey;

                // Work out the column key.
                string tempKey = key + matchKey;

                // Set the state to the present value.
                state[depth] = matchMappedValue;

                // If not at the bottom of the list, keep permutating.
                if (depth < values.Count - 1) Permutation(tempKey, state, values, map, depth + 1);
                else {
                    // Copy the state array.
                    string[] finalState = new string[state.Length];
                    state.CopyTo(finalState, 0);

                    // Add the key and states at that point.
                    map.Add(tempKey, finalState);
                }
            }
        }

        static void Main(string[] args)
        {
            // Usage.
            if (args.Length <= 1)
            {
                Console.WriteLine("Usage: dimensionalize <filename> [options] <col-spec> [col-spec]...[col-spec]");
                Console.WriteLine("\t{0}\t{1}", "<col-spec>", "Defines the how to map a column in the source data file to dimensions.");
                Console.WriteLine("\t{0}\t{1}", "--sanitize", "Tidy up values stripping out quotes and commas.");
                Console.WriteLine("\t{0}\t{1}", "--quote-labels", "Put quotations around all text labels.");
                Console.WriteLine("\t{0}\t{1}", "--ignore-unknown-columns", "When a column does not map correctly, do not produce any output.");
                Console.WriteLine("\t{0}\t{1}", "--new-headings=custom1,custom2...etc.", "New column headings for first row of output.");
                return;
            }

            // File to read from command line arguments.
            string filename = args[0];

            // Read argument flags
            List<string> arguments = new List<string>();
            int offset = 1;
            for(; offset < args.Length; offset++)
            {
                string arg = args[offset];
                if (arg.StartsWith("--", StringComparison.CurrentCulture)) arguments.Add(arg);
                else break;
            }

            bool argSanitize = arguments.Contains("--sanitize"); // Sanitize value outputs.
            bool argQuotedLabels = arguments.Contains("--quote-labels"); // Quote label strings.
            bool failOnNoMatch = !arguments.Contains("--ignore-unknown-columns"); // Exit with an error message when an unmatched column is encountered.

            string[] dimensionHeadings = null;
            string a = arguments.Find(a => a.StartsWith("--new-headings=", StringComparison.CurrentCulture));
            if (a != null) dimensionHeadings = a.Substring("--new-headings=".Length).Split(",");

            // Column values.
            List<string[]> columns = new List<string[]>();
            Dictionary<string, string[]> permutated = new Dictionary<string, string[]>();

            // Capture the column values from the command line arguments.
            for (int i = offset; i < args.Length; i++)
            {
                string[] values = args[i].Split(',');
                columns.Add(values);
            }

            // Recursively create the permutations.
            Permutation(null, null, columns, permutated, 0);

            StreamReader reader = null;

            try
            {
                // Open the source file.
                reader = new StreamReader(filename);

                // Read the column heading.
                string line = reader.ReadLine();

                // Map the permutated columns into the correct order of data in file.
                Regex reg = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                List<string[]> map = new List<string[]>();
                map.Add(null); // Skip the first column heading.
                string[] headings = reg.Split(line);
                
                for(int i = 1; i < headings.Length; i++)
                {
                    string heading = headings[i];

                    // Look for a match.
                    if (permutated.ContainsKey(heading))
                    {
                        map.Add(permutated[heading]);
                    }
                    else
                    {
                        if (failOnNoMatch)
                        {
                            Console.Error.WriteLine("Error: cannot map column - {0}", heading);
                            return;
                        }

                        map.Add(null); // No match.
                    }
                }

                // Write new headings.
                if(dimensionHeadings != null)
                {
                    foreach(string heading in dimensionHeadings) Console.Write("{0},", heading);
                    Console.WriteLine((argQuotedLabels) ? String.Format("\"{0}\"", "value") : "value");
                }

                // Read the data.
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    string[] data = reg.Split(line);

                    // Columns of data.
                    for (int i = 1; i < data.Length; i++)
                    {
                        string[] fields = map[i];

                        if (fields != null)
                        {
                            Console.Write((argQuotedLabels) ? String.Format("\"{0}\"", data[0]) : data[0]); // Row heading.
                            foreach (string field in fields) Console.Write(",{0}", (argQuotedLabels)? String.Format("\"{0}\"", field) : field); // Fields.
                            Console.WriteLine(",{0}", (argSanitize)? data[i].Replace(",","").Replace("\"","") : data[i]); // Value.
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error: Could not open file - {0}", filename);
            }
            finally
            {
                if(reader != null) reader.Close();
            }
        }
    }
}
