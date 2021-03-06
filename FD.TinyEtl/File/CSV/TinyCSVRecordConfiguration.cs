///////////////////////////////////////////////////////////
//  TinyCSVRecordConfiguration.cs
//  Implementation of the Class TinyCSVRecordConfiguration
//  Generated by Enterprise Architect
//  Created on:      06-5��-2018 18:46:51
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace FD.TinyEtl {
    public class TinyCSVRecordConfiguration
    {
        public int BufferSize { get; internal set; }

        public Encoding GetEncoding(string fileName)
        {
            if (_encoding == null)
            {
                try
                {
                    //ChoETLLog.Info("Determining '{0}' file encoding...".FormatString(fileName));
                    Encoding = TinyFile.GetEncodingFromFile(fileName);
                    //ChoETLLog.Info("Found '{1}' encoding in '{0}' file.".FormatString(fileName, Encoding));
                }
                catch (Exception ex)
                {
                    Encoding = Encoding.UTF8;
                    //ChoETLLog.Error("Error finding encoding in '{0}' file. Default to UTF8.".FormatString(fileName));
                    //ChoETLLog.Error(ex.Message);
                }
            }

            return Encoding;
        }

        public Encoding GetEncoding(Stream inStream)
        {
            if (_encoding == null)
            {
                try
                {
                    //ChoETLLog.Info("Determining file encoding...");
                    Encoding = TinyFile.GetEncodingFromStream(inStream);
                    //ChoETLLog.Info("Found {0} encoding in file.".FormatString(Encoding));
                }
                catch (Exception ex)
                {
                    Encoding = Encoding.UTF8;
                    //ChoETLLog.Error("Error finding encoding in file. Default to UTF8.");
                    //ChoETLLog.Error(ex.Message);
                }
            }

            return Encoding;
        }


        private Encoding _encoding;
        public Encoding Encoding
        {
            get { return _encoding != null ? _encoding : Encoding.UTF8; }
            set { _encoding = value; }
        }
    }//end TinyCSVRecordConfiguration

}//end namespace FD.TinyEtl