///////////////////////////////////////////////////////////
//  TinyPath.cs
//  Implementation of the Class TinyPath
//  Generated by Enterprise Architect
//  Created on:      06-5��-2018 18:52:01
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace FD.TinyEtl {
	public static class TinyPath {
        public static string EntryAssemblyBaseDirectory = null;
        public static string EntryAssemblyName = null;

        private static readonly string _fileNameCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static readonly Regex _fileNameCleaner = new Regex(_fileNameCleanerExpression); //, RegexOptions.Compiled);
        private static readonly string _pathCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static readonly Regex _pathCleaner = new Regex(_pathCleanerExpression); //, RegexOptions.Compiled);

        static TinyPath()
        {
            //if (!ChoETLFrxBootstrap.IsSandboxEnvironment)
                _Initialize();
        }

        private static void _Initialize()
        {
            //if (System.Web.HttpContext.Current == null)
            //{
                string loc = Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly().Location : Assembly.GetCallingAssembly().Location;
                EntryAssemblyBaseDirectory = Path.GetDirectoryName(loc);
                EntryAssemblyName = Path.GetFileNameWithoutExtension(loc);
            //}
            //else
            //{
            //    EntryAssemblyBaseDirectory = System.Web.HttpRuntime.AppDomainAppPath;
            //    EntryAssemblyName = new DirectoryInfo(System.Web.HttpRuntime.AppDomainAppPath).Name;
            //}
        }

        public static string CleanPath(string path)
        {
            return _pathCleaner.Replace(path, "_");
        }

        public static string CleanFileName(string fileName)
        {
            return _fileNameCleaner.Replace(fileName, "_");
        }

        public static string GetFullPath(string path, string baseDirectory = null)
        {
            if (path.IsNullOrWhiteSpace())
                return path;

            if (Path.IsPathRooted(path))
                return path;
            else if (!baseDirectory.IsNullOrWhiteSpace())
                return GetFullPath(Path.Combine(baseDirectory, path));
            else if (!EntryAssemblyBaseDirectory.IsNullOrEmpty())
                return GetFullPath(Path.Combine(EntryAssemblyBaseDirectory, path));
            else
                return Path.GetFullPath(path);
        }
    }//end TinyPath

}//end namespace FD.TinyEtl