using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primbot_v._2.Uno_Score_Tracking {
    class SaveFiles_Entries {
        public static bool EntryExists(string path, string value) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }
            using (StreamReader sr = new StreamReader(path)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    if (line == value) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool AddEntry(string path, string entry) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }
            try {
                File.AppendAllText(path, entry + Environment.NewLine);
            } catch {
                return false;
            }
            return true;
        }

        //returns false if entry does not exist
        public static bool DeleteEntry(string path, string entry) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }

            List<string> fileReturnLines = new List<string>();
            bool found = false;
            string line = "";
            using (StreamReader sr = new StreamReader(path)) {
                while ((line = sr.ReadLine()) != null) {
                    if (line == entry) {
                        found = true;
                        continue;
                    }
                    fileReturnLines.Add(line);
                }
            }
            File.WriteAllLines(path, fileReturnLines.ToArray());
            return found;
        }
    }
}
