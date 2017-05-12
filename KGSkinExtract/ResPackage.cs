using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics; 

namespace KugouSkinRead
{
    class ResPackage
    {
        public String Path;
        public ArrayList FileList;
        private DirectoryInfo folder;

        //委托
        public delegate void dPackageProgress(long total, long current, String filename);
        //事件
        public event dPackageProgress onPackageProgress;

        public ResPackage(String Path)
        {
            if ((Path.LastIndexOf('\\') != Path.Length - 1) && (Path.LastIndexOf('/') != Path.Length - 1))
            {
                Path += "\\";
            }
            this.Path = Path;
            this.folder = new DirectoryInfo(Path);
            this.FileList = GetFileNames(folder);
        }

        public void Pack(String FileName)
        {
            FileStream fs;
            BinaryWriter FMemStream;
            String[] fl = (String[])this.FileList.ToArray(typeof(String));
            Int32 Position = 0;

            fs = File.Create(FileName);
            FMemStream = new BinaryWriter(fs);
            FMemStream.Write((Int32)fl.Length);

            String dat = System.Environment.GetEnvironmentVariable("TEMP") + "\\~res.dat";
            String dat2 = System.Environment.GetEnvironmentVariable("TEMP") + "\\~res2.dat";

            Tools.CmdHelper.ExecuteCmd("echo a 2>\"" + dat + "\"");

            long current = 0;
            foreach (String file in fl)
            {
                FileStream resfile = File.Open(this.Path + file,FileMode.Open);
                FMemStream.Write((Int32)(file.Length * 2 + 13));
                FMemStream.Write((Byte)(file.Length * 2));
                FMemStream.Write(System.Text.Encoding.Unicode.GetBytes(file));
                FMemStream.Write(Position);
                FMemStream.Write((Int32)resfile.Length);
                Position += (Int32)resfile.Length;
                resfile.Close();                

                Tools.CmdHelper.ExecuteCmd("copy /b \"" + dat + "\"+\"" + this.Path + file + "\" \"" + dat2 + "\"");
                Tools.CmdHelper.ExecuteCmd("DEL \"" + dat + "\"");
                Tools.CmdHelper.ExecuteCmd("REN \"" + dat2 + "\" \"~res.dat\"");
                
                if (onPackageProgress != null)
                    onPackageProgress(fl.Length, ++current, file);
            }

            fs.Close();

            Tools.CmdHelper.ExecuteCmd("copy /b \"" + FileName + "\"+\"" + dat + "\" \"" + dat2 + "\"");
            Tools.CmdHelper.ExecuteCmd("DEL \"" + FileName + "\"");
            Tools.CmdHelper.ExecuteCmd("MOVE \"" + dat2 + "\" \"" + FileName + "\"");
   
        }

        private ArrayList GetFileNames(DirectoryInfo dirInfo)
        {
            ArrayList fns = new ArrayList();

            foreach (DirectoryInfo subdir in dirInfo.GetDirectories())
            {
                fns.AddRange(GetFileNames(subdir));
            }

            foreach( FileInfo fi in dirInfo.GetFiles())
            {
                fns.Add(fi.FullName.Substring(this.Path.Length));
            }
            return fns;
        }
    }
}
