using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pleer
{
    internal class Logic
    {
        public bool isExist(string filepath)
        {
            bool isExist = File.Exists(filepath);
            return isExist;
        }
        public List<string> collection_mp3 = new List<string>();
        public List<string> collection_mp4 = new List<string>();

        public List<string> ReturnCollection_mp3(string filepath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(filepath);
                collection_mp3 = dir.GetFiles("*.mp3").Select(file => file.Name).ToList();
                return collection_mp3;
            }
            catch (Exception)
            {
                throw new Exception("");
            }
        }

        public List<string> ReturnCollection_mp4(string filepath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(filepath);
                collection_mp4 = dir.GetFiles("*.mp4").Select(file => file.Name).ToList();
                return collection_mp4;
            }
            catch (Exception)
            {
                throw new Exception("");
            }
        }
        public string FolderPath(string folderpath)
        {
            return folderpath;
        }
    }
}
