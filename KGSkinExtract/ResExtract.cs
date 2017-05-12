using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 

namespace KugouSkinRead
{
    struct KugouResRec 
    {
        public int Index;
        public int FileSize;
        public String FileName;
        public Int64 Position;
        public Byte[] Other;
    };

    class ResExtract
    {
        public List<KugouResRec> FileList;
        public String FileName;
        public BinaryReader FMemStream;

        //委托
        public delegate void dExtractProgress(long total, long current, String filename);
        //事件
        public event dExtractProgress onExtractProgress;

        public ResExtract()
        {
            FileList = new List<KugouResRec>();
        }

        public void clear()
        {
            FileList.Clear();
        }

        public void Destroy()
        {
            FMemStream.Close();
        }

        public void LoadFromFile(String FileName)
        {
            int         FileCount;
            int         RecLen;
            Byte        NameLen;
            Byte[]      bName;
            Int64       FileStartPosi;
            FileStream  fs;
            KugouResRec Item;

            Item = new KugouResRec();
            try
            {
                clear();
                this.FileName = FileName;
                fs = File.Open(FileName, FileMode.Open);
                this.FMemStream = new BinaryReader(fs);
                FileCount = this.FMemStream.ReadInt32();

                for (int i = 0; i < FileCount; i++)
                {
                    Item.Index = i;
                    RecLen = this.FMemStream.ReadInt32();
                    NameLen = this.FMemStream.ReadByte();
                    bName = this.FMemStream.ReadBytes(NameLen);
                    Item.FileName = System.Text.Encoding.Unicode.GetString(bName);
                    Item.Other = this.FMemStream.ReadBytes(RecLen - (4 + 1 + NameLen) - 4);
                    Item.FileSize = this.FMemStream.ReadInt32();
                    Item.Position = this.FMemStream.BaseStream.Position;
                    FileList.Add(Item);
                    if(i> 0x6aa)
                    {
                        int jj = i;
                    }
                }
                FileStartPosi = this.FMemStream.BaseStream.Position;
                for (int i = 0; i < FileList.Count; i++)
                {
                    Item = FileList[i];
                    Item.Position = FileStartPosi;
                    FileStartPosi += Item.FileSize;
                    FileList[i] = Item;
                }
            }
            catch (Exception) { }
        }

        public void Extract(int Index, byte[] bd)
        {
            KugouResRec Item;
            if(Index >=0 && Index < FileList.Count)
            {
                Item = FileList[Index];
                //bd = new byte[Item.FileSize];
                FMemStream.BaseStream.Seek(Item.Position, SeekOrigin.Begin);
                FMemStream.Read(bd, 0, Item.FileSize);
            }
        }

        public void Extract(String filename, byte[] bd)
        {
            for (int i = 0; i < FileList.Count;i++)
            {
                if(String.Compare(FileList[i].FileName, filename, true) == 0)
                {
                    Extract(i,bd);
                }
            }
        }

        public void ExtractAll(String fileDir)
        {
            try
            {
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                if (fileDir.LastIndexOf('\\') != fileDir.Length - 1)
                {
                    fileDir += "\\";
                }
                for (int i = 0; i < FileList.Count; i++)
                {
                    byte[] bd = new byte[FileList[i].FileSize];
                    Extract(i, bd);

                    String FileName = fileDir + FileList[i].FileName;

                    FileStream fs = File.Create(FileName);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(bd, 0, FileList[i].FileSize);
                    fs.Close();

                    if (onExtractProgress != null)
                        onExtractProgress(FileList.Count, i, FileName);
                }
            }
            catch(Exception){}
        }
    }
}
