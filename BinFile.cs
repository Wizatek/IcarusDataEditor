using System;
using System.IO;
using System.Text;

namespace IcarusDataEditor
{
    class BinFile
    {
        public const byte FIELD_TYPE_NONE = 0xFF;
        public const byte FIELD_TYPE_FLOAT = 0;
        public const byte FIELD_TYPE_STRING = 1;

        Encoding enc = Encoding.UTF8;

        short fieldCount = 0;
        short recordCount = 0;

        byte[][] fieldNames;
        byte[] fieldTypes;

        int[,] offsets;

        byte[] dataBuffer;

        public short GetRowCount() => recordCount;
        public short GetColCount() => fieldCount;

        public byte GetFieldType(int col)
        {
            if (col < 0 || col >= fieldCount)
                return FIELD_TYPE_NONE;

            return fieldTypes[col];
        }

        public bool SetEncoding(string encodingName)
        {
            try
            {
                enc = Encoding.GetEncoding(encodingName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
                return false;            

            try
            {
                using(BinaryReader b = new BinaryReader(File.OpenRead(fileName)))
                {
                    fieldCount = b.ReadInt16();
                    
                    fieldNames = new byte[fieldCount][];
                    fieldTypes = new byte[fieldCount];
                    for(int i=0;i<fieldCount;i++)
                    {
                        fieldTypes[i] = b.ReadByte();
                        fieldNames[i] = b.ReadBytes(b.ReadByte());
                    }

                    recordCount = b.ReadInt16();
                    offsets = new int[recordCount, fieldCount];
                    for (int i = 0; i < recordCount; i++)
                        for (int j = 0; j < fieldCount; j++)
                            offsets[i, j] = b.ReadInt32();

                    int dataSize = b.ReadInt32();
                    dataBuffer = b.ReadBytes(dataSize);
                }

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool SaveFile(string fileName)
        {
            try
            {
                using(BinaryWriter w = new BinaryWriter(File.Create(fileName)))
                {
                    w.Write(fieldCount);
                    for(int i=0;i<fieldCount;i++)
                    {
                        w.Write(fieldTypes[i]);
                        w.Write((byte)fieldNames[i].Length);
                        w.Write(fieldNames[i]);
                    }

                    w.Write(recordCount);
                    for (int i = 0; i < recordCount; i++)
                        for (int j = 0; j < fieldCount; j++)
                            w.Write(offsets[i, j]);

                    w.Write(dataBuffer.Length);
                    w.Write(dataBuffer);
                }

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public string GetName(int col)
        {
            if (col < 0 || col >= fieldCount)
                return string.Empty;

            return enc.GetString(fieldNames[col]);
        }

        public string GetString(int row, int col)
        {
            if (row < 0 || row >= recordCount)
                return string.Empty;

            if (col < 0 || col >= fieldCount)
                return string.Empty;

            if (fieldTypes[col] != FIELD_TYPE_STRING)
                return string.Empty;

            int len = 0;
            int pos = offsets[row, col];
            while (dataBuffer[pos++] != 0)
                len++;

            string str = enc.GetString(dataBuffer, offsets[row, col], len);

            return str;
        }

        public float GetFloat(int row, int col)
        {
            if (row < 0 || row >= recordCount)
                return 0;

            if (col < 0 || col >= fieldCount)
                return 0;

            if (fieldTypes[col] != FIELD_TYPE_FLOAT)
                return 0;

            float data = BitConverter.ToSingle(dataBuffer, offsets[row, col]);
            return data;
        }

        public void SetData(int rowCount, int colCount, object[] data)
        {
            fieldCount = (short)colCount;
            recordCount = (short)rowCount;

            offsets = new int[recordCount, fieldCount];

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {

                        offsets[row, col] = (int)w.BaseStream.Position;

                        string str = data[(row * fieldCount) + col].ToString();
                        switch(GetFieldType(col))
                        {
                            case FIELD_TYPE_STRING:
                                {
                                    w.Write(enc.GetBytes(str));
                                    w.Write((byte)0x00);
                                }
                                break;

                            case FIELD_TYPE_FLOAT:
                                {
                                    float f = 0.0f;
                                    float.TryParse(str, out f);
                                    w.Write(f);
                                }
                                break;
                        }
                    }
                }
            }

            dataBuffer = ms.ToArray();
        }

    }
}
