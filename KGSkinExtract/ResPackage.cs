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
            FileStream datfile = File.Open(dat, FileMode.Open);
            BinaryWriter datStream = new BinaryWriter(datfile);
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
                byte[] resdata = new byte[resfile.Length];
                resfile.Read(resdata, 0, (int)resfile.Length);
                datStream.Write(resdata, 0, resdata.Length);
                resfile.Close();                
                
                if (onPackageProgress != null)
                    onPackageProgress(fl.Length, ++current, file);
            }

            datStream.Close();
            datfile.Close();

            datfile = File.Open(dat, FileMode.Open);

            BinaryReader restmp = new BinaryReader(datfile);
            byte[] data = new byte[datfile.Length];
            restmp.Read(data, 0, (int)datfile.Length);
            FMemStream.Write(data, 0, data.Length);
            datfile.Close();
            fs.Close();
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
